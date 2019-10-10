using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject Player;


    private float MouseX;
    private float MouseY;
    private float CurrentDis;
    private float CurrentTargetDis;
    private float CurrentFieldOfView;
    // Start is called before the first frame update
    void Start()
    {
        CurrentDis = GetComponent<CameraData>().MaxDistanceToPlayer;
        CurrentTargetDis = CurrentDis;
        CurrentFieldOfView = GetComponent<CameraData>().NormalFieldOfView;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        RotateCamera();
        SetCameraPos();
        SetFieldOfView();
    }

    private void RotateCamera()
    {
        var Data = GetComponent<CameraData>();

        MouseX += Input.GetAxis("Mouse X") * Data.RotationSpeed * Time.deltaTime;
        MouseY -= Input.GetAxis("Mouse Y") * Data.RotationSpeed * Time.deltaTime;

        MouseY = Mathf.Clamp(MouseY, Data.MinAngleY, Data.MaxAngleY);

        transform.rotation = Quaternion.Euler(MouseY, MouseX, 0);
    }

    private void SetFieldOfView()
    {
        var Data = GetComponent<CameraData>();

        if (Player.GetComponent<PlayerActions>().Dashing)
        {
            CurrentFieldOfView += Data.FieldOfViewIncreaseSpeed * Time.deltaTime;
            if (CurrentFieldOfView >= Data.DashFieldOfView)
            {
                CurrentFieldOfView = Data.DashFieldOfView;
            }
        }
        else
        {
            CurrentFieldOfView -= Data.FieldOfViewDecreaseSpeed * Time.deltaTime;

            if(CurrentFieldOfView <= Data.NormalFieldOfView)
            {
                CurrentFieldOfView = Data.NormalFieldOfView;
            }
        }

        GetComponent<Camera>().fieldOfView = CurrentFieldOfView;
    }

    private void SetCameraPos()
    {
        var Data = GetComponent<CameraData>();

        transform.position = Player.transform.position + transform.rotation * Vector3.back * Data.MaxDistanceToPlayer;

        RaycastHit Hit;

        if (Physics.Raycast(Player.transform.position, transform.position - Player.transform.position, out Hit, Data.MaxDistanceToPlayer+Data.MinDisFromBlock, Data.Blocks))
        {

            if(Hit.distance < Data.MinDisFromBlock)
            {
                CurrentTargetDis = Hit.distance;
            }
            else
            {
                CurrentTargetDis = Hit.distance - Data.MinDisFromBlock;
            }

            if (CurrentTargetDis < Data.NearWallThreshold)
            {
                Debug.Log("ttt");
                CurrentTargetDis -= Data.NearWallDeduction;
            }

        }
        else
        {
            CurrentTargetDis = Data.MaxDistanceToPlayer;
        }

        if (CurrentDis < CurrentTargetDis)
        {
            CurrentDis+= Data.DisChangeSpeed * Time.deltaTime;
            if (CurrentDis > CurrentTargetDis)
            {
                CurrentDis = CurrentTargetDis;
            }
        }
        else
        {
            CurrentDis -= Data.DisChangeSpeed * Time.deltaTime;
            if(CurrentDis < CurrentTargetDis)
            {
                CurrentDis = CurrentTargetDis;
            }


        }

        transform.position = Player.transform.position + transform.rotation * Vector3.back * CurrentDis;

        if ( transform.position.y < Player.transform.position.y)
        {
            transform.position += Vector3.up * (Player.transform.position.y - transform.position.y);
        }
    }
}
