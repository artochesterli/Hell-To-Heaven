using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    GroundMove,
    Fall,
    Glide,
}

public class PlayerActions : MonoBehaviour
{
    public GameObject Wings;
    public GameObject Camera;
    public bool Dashing;

    private FSM<PlayerActions> PlayerActionsFSM;

    // Start is called before the first frame update
    void Start()
    {
        PlayerActionsFSM = new FSM<PlayerActions>(this);
        PlayerActionsFSM.TransitionTo<Idle>();

    }

    // Update is called once per frame
    void Update()
    {
        PlayerActionsFSM.Update();
    }
}

public abstract class PlayerActionState : FSM<PlayerActions>.State
{
    public override void Init()
    {
        base.Init();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log(this.GetType());
    }

    public override void Update()
    {
        base.Update();
    }


    protected float GetXInput()
    {
        return Input.GetAxis("Horizontal");
    }

    protected float GetZInput()
    {
        return Input.GetAxis("Vertical");
    }

    protected bool InputJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    protected bool InputAttach()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }

    protected bool AttachToWall()
    {
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        if(InputAttach() && Controller.Wall)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void GravityEffect()
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        float Gravity = Data.NormalGravity;

        Controller.MoveSpeed.y -= Gravity * Time.deltaTime;
    }

    protected void GlideEffect()
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        if (Controller.MoveSpeed.y < -Data.GlideMaxFallSpeed)
        {
            Controller.MoveSpeed.y += Data.GlideFallSpeedChange * Time.deltaTime;
            if(Controller.MoveSpeed.y > -Data.GlideMaxFallSpeed)
            {
                Controller.MoveSpeed.y = -Data.GlideMaxFallSpeed;
            }
        }
        else
        {
            Controller.MoveSpeed.y -= Data.GlideFallSpeedChange * Time.deltaTime;
            if (Controller.MoveSpeed.y < -Data.GlideMaxFallSpeed)
            {
                Controller.MoveSpeed.y = -Data.GlideMaxFallSpeed;
            }
        }
    }

    protected bool AbleToGlide()
    {
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        return Controller.MoveSpeed.y <= 0 && !Controller.Ground;
    }

    protected bool PlayerInAir()
    {
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        return !Controller.Ground;
    }

    protected bool PlayerFallGround()
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        if(Controller.MoveSpeed.y<=0 && Controller.Ground)
        {
            Controller.MoveSpeed.y = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool PlayerJump(bool Dash, Vector3 Direction)
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        if (InputJump())
        {
            float JumpSpeed;

            if (Dash)
            {
                JumpSpeed = Data.DashJumpSpeed;
                Controller.MoveSpeed = Direction * JumpSpeed;
            }
            else
            {
                JumpSpeed = Data.NormalJumpSpeed;
                Controller.MoveSpeed.y = JumpSpeed;
            }
            return true;


        }

        return false;

    }

    protected bool PlayerMove(bool Glide)
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        Vector3 MoveVector = new Vector3(GetXInput(), 0, GetZInput());
        if(MoveVector.x==0 && MoveVector.z == 0)
        {
            Controller.MoveSpeed = new Vector3(0, Controller.MoveSpeed.y, 0);
            return false;
        }

        Quaternion PlaneRotation = Quaternion.Euler(0, Context.Camera.transform.eulerAngles.y, 0);

        MoveVector = PlaneRotation * MoveVector;

        float TurnSpeed;
        float MoveSpeed;

        if (Glide)
        {
            TurnSpeed = Data.GlideTurnSpeed;
            MoveSpeed = Data.GlideSpeed;

        }
        else
        {
            TurnSpeed = Data.GroundTurnSpeed;
            MoveSpeed = Data.GroundMoveSpeed;
        }

        float Angle = Vector3.SignedAngle(Context.gameObject.transform.forward, MoveVector, Vector3.up);

        float RotationAngle;

        if (Mathf.Abs(Angle) < TurnSpeed * Time.deltaTime)
        {
            RotationAngle = Angle;
        }
        else
        {
            if (Angle > 0)
            {
                RotationAngle = TurnSpeed * Time.deltaTime;
            }
            else
            {
                RotationAngle = -TurnSpeed * Time.deltaTime;
            }
        }

        if (Glide)
        {
            //Debug.Log(RotationAngle);
            Context.gameObject.transform.Rotate(0, RotationAngle, 0);
        }
        else
        {
            Context.gameObject.transform.eulerAngles =new Vector3(Context.gameObject.transform.eulerAngles.x, Context.gameObject.transform.eulerAngles.y + Angle, Context.gameObject.transform.eulerAngles.z);
        }

        Vector2 PlaneSpeed= new Vector2(Context.gameObject.transform.forward.x, Context.gameObject.transform.forward.z) * MoveSpeed;

        Controller.MoveSpeed = new Vector3(PlaneSpeed.x, Controller.MoveSpeed.y, PlaneSpeed.y);

        return true;
    }

}

public class Idle : PlayerActionState
{
    public override void Init()
    {
        base.Init();
    }

    public override void OnEnter()
    {
        base.OnEnter();

    }

    public override void Update()
    {
        base.Update();

        if (PlayerMove(false))
        {
            TransitionTo<GroundMove>();
            return;
        }

        if (PlayerInAir())
        {
            TransitionTo<Fall>();
        }

        if (AttachToWall())
        {
            TransitionTo<Attached>();
            return;
        }

        if (PlayerJump(false,Vector3.up))
        {
            TransitionTo<Fall>();
            return;
        }
    }
}

public class GroundMove : PlayerActionState
{
    public override void Update()
    {
        base.Update();
        PlayerMove(false);

        if (PlayerInAir())
        {
            TransitionTo<Fall>();
        }

        if (AttachToWall())
        {
            TransitionTo<Attached>();
            return;
        }

        if (PlayerJump(false, Vector3.up))
        {
            TransitionTo<Fall>();
            return;
        }
    }
}

public class Fall : PlayerActionState
{
    public override void Update()
    {
        base.Update();
        GravityEffect();

        if (PlayerFallGround())
        {
            TransitionTo<Idle>();
            return;
        }

        if (AttachToWall())
        {
            TransitionTo<Attached>();
            return;
        }

        if (AbleToGlide() && PlayerMove(false))
        {
            TransitionTo<Glide>();
            return;
        }
    }
}

public class Glide : PlayerActionState
{
    public override void OnEnter()
    {
        base.OnEnter();
        foreach(Transform child in Context.Wings.transform)
        {
            child.GetComponent<TrailRenderer>().enabled = true;
        }
    }

    public override void Update()
    {
        base.Update();
        GlideEffect();
        if (!PlayerMove(true))
        {
            TransitionTo<Fall>();
            return;
        }

        if (AttachToWall())
        {
            TransitionTo<Attached>();
            return;
        }

        if (PlayerFallGround())
        {
            TransitionTo<GroundMove>();
            return;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        foreach (Transform child in Context.Wings.transform)
        {
            child.GetComponent<TrailRenderer>().enabled = false;
        }
    }
}

public class Attached : PlayerActionState
{
    public override void OnEnter()
    {
        base.OnEnter();
        var Controller = Context.gameObject.GetComponent<PlayerController>();

        Controller.MoveSpeed = Vector3.zero;

        Context.gameObject.transform.eulerAngles = new Vector3(0, Vector3.SignedAngle(Vector3.forward, Controller.WallNormal,Vector3.up), 0);
    }

    public override void Update()
    {
        base.Update();

        if(PlayerJump(true, Context.Camera.transform.rotation * Vector3.forward))
        {
            TransitionTo<Dash>();
            return;
        }

        if(!InputAttach())
        {
            TransitionTo<Fall>();
            return;
        }
    }
}

public class Dash : PlayerActionState
{
    private float DashTimeCount;

    public override void OnEnter()
    {
        base.OnEnter();
        Context.Dashing = true;
        DashTimeCount = 0;
        foreach (Transform child in Context.Wings.transform)
        {
            child.GetComponent<TrailRenderer>().enabled = true;
        }
    }

    public override void Update()
    {
        base.Update();

        if (PlayerFallGround())
        {
            TransitionTo<Idle>();
            return;
        }

        if (CountTime())
        {
            TransitionTo<Fall>();
            return;
        }
    }

    private bool CountTime()
    {
        var Data = Context.gameObject.GetComponent<PlayerData>();

        DashTimeCount += Time.deltaTime;
        if(DashTimeCount > Data.DashTime)
        {
            return true;
        }

        return false;
    }

    public override void OnExit()
    {
        base.OnExit();
        Context.Dashing = false;
        foreach (Transform child in Context.Wings.transform)
        {
            child.GetComponent<TrailRenderer>().enabled = false;
        }
    }
}


