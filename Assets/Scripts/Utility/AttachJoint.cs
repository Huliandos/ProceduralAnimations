using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachJoint : MonoBehaviour
{
    Joint joint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Interactable")
        {
            if (joint == null && other.GetComponent<FixedJoint>() == null)
            {
                GetComponent<AudioSource>().Play();

                joint = other.gameObject.AddComponent<FixedJoint>();

                joint.connectedBody = GetComponent<Rigidbody>();

                joint.breakForce = 100;
            }
            //joint.breakTorque = 10;
        }
    }

    public void releaseBody() {
        if (joint)
        {
            joint.connectedBody = null;

            Destroy(joint);
        }
        else {
            joint = null;
        }
    }
}