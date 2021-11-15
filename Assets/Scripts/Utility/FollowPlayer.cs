using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField]
    Transform player;

    Rigidbody rb;
    float yaw, pitch, mouseSpeed = 2f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //ToDo: change to smooth move lerp later
        transform.position = player.position;
    }

    private void FixedUpdate()
    {
        yaw += mouseSpeed * Input.GetAxis("Mouse X");
        pitch += mouseSpeed * Input.GetAxis("Mouse Y");
        
        if (pitch >= 20) pitch = 20;
        else if (pitch <= -20) pitch = -20;
        rb.transform.eulerAngles = new Vector3(pitch, yaw, 0);
    }
}
