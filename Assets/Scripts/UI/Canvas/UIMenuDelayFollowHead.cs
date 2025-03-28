using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuDelayFollowHead : MonoBehaviour
{
    public float distance;
    public float offsetY;
    public float offsetX;
    public float movementSpeed = 2f;
    public float rotationSpeed = 5f;
    private Transform playerHead;

    private bool canFollow = true;
    // Start is called before the first frame update
    void Start()
    {
        playerHead = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (canFollow)
        {
            Follow();
        }

    }
    public void Follow()
    {
        Vector3 camForward = Camera.main.transform.forward + Camera.main.transform.position;
        camForward.y = Camera.main.transform.position.y;
        Vector3 camForwardOffset = new Vector3(camForward.x + offsetX, camForward.y + offsetY, camForward.z);
        Vector3 direction = (camForwardOffset - Camera.main.transform.position).normalized;
        Vector3 targetPosition = Camera.main.transform.position + direction * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);

        Vector3 playerPosition = new Vector3(playerHead.position.x, transform.position.y, playerHead.position.z);
        Vector3 directionFromPlayerToCanvas = transform.position - playerPosition;
        Quaternion targetRotation = Quaternion.LookRotation(directionFromPlayerToCanvas);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

    }
    public void ControlFollow(bool canFollow)
    {
        this.canFollow = canFollow;
    }
}
