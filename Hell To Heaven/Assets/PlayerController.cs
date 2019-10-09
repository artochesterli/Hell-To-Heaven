using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public Vector3 MoveSpeed;

    public float GroundCheckOffset;
    public Vector2 GroundCheckBodySize;
    public float GroundCheckDis;
    public float GroundHitDis;
    public float WallCheckOffset;
    public Vector2 WallCheckBodySize;
    public float WallCheckDis;
    public float WallHitDis;

    public LayerMask WallLayer;
    public LayerMask GroundLayer;

    public GameObject Ground;
    public float GroundDis;
    public GameObject Wall;
    public float WallDis;
    public Vector3 WallNormal;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        CheckWall();
        Move();
    }

    private void Move()
    {
        characterController.Move(MoveSpeed*Time.deltaTime);
    }

    private void CheckGround()
    {
        RaycastHit hit;

        //Ray ray = new Ray(transform.position + GroundCheckOffset, Vector3.down);
        //Debug.DrawRay(transform.position,Vector3.down,Color.red,GroundCheckDis + GroundCheckOffset);

        if (Physics.BoxCast(transform.position,new Vector3(GroundCheckBodySize.x/2, 0.01f, GroundCheckBodySize.y/2), Vector3.down, out hit, Quaternion.Euler(0, 0, 0), GroundCheckDis + GroundCheckOffset, GroundLayer))
        {
            GroundDis = hit.distance - GroundCheckOffset;
            if (GroundDis < GroundHitDis)
            {
                Ground = hit.collider.gameObject;
            }
            else
            {
                Ground = null;
            }


        }
        else
        {
            Ground = null;
        }
    }

    private void CheckWall()
    {
        RaycastHit hit;
        if (Physics.BoxCast(transform.position, new Vector3(WallCheckBodySize.x/2, WallCheckBodySize.y/2, 0.01f), transform.forward, out hit, transform.rotation, WallCheckDis + WallCheckOffset, WallLayer))
        {
            WallDis = hit.distance - WallCheckOffset;
            Debug.Log(WallDis);
            if(WallDis < WallHitDis)
            {
                Wall = hit.collider.gameObject;
                WallNormal = hit.normal;
                MoveSpeed -= Vector3.Project(MoveSpeed, WallNormal);
            }
            else
            {
                Wall = null;
            }

        }
        else
        {
            Wall = null;
        }
    }
}
