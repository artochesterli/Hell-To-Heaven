using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraData : MonoBehaviour
{
    public float MaxDistanceToPlayer;
    public float MinDistanceToPlayer;
    public float DisChangeSpeed;
    public float OffsetTriggerDis;
    public float MaxOffset;
    public float MinDisFromBlock;
    public float VerticalOffsetChangeSpeed;
    public float RotationSpeed;

    public float NormalFieldOfView;
    public float DashFieldOfView;
    public float FieldOfViewIncreaseSpeed;
    public float FieldOfViewDecreaseSpeed;

    public float MinAngleY;
    public float MaxAngleY;

    public LayerMask Blocks;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
