using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuFollowHead : MonoBehaviour
{
    private Transform playerHead;
    // Start is called before the first frame update
    void Start()
    {
        playerHead = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        FollowHeadRotation();
    }
    private void FollowHeadRotation()
    {
        //让Canvas始终朝向玩家并且只旋转Y轴
        Vector3 playerPosition = new Vector3(playerHead.position.x, transform.position.y, playerHead.position.z);
        //获得玩家指向Canvas的方向
        Vector3 directionFromPlayerToCanvas = transform.position - playerPosition;
        //让Canvas的z轴方向与玩家朝向Canvas的方向一致
        //LookAt方向是将Canvas的z轴正方向指向玩家，而UI的正面朝向玩家时，Canvas的z轴负方向是朝向玩家的，所以不能用LookAt
        transform.rotation = Quaternion.LookRotation(directionFromPlayerToCanvas);
    }
}
