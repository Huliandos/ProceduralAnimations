using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGrabController : MonoBehaviour
{
    [Header("Testing")]
    //all beneath this is experimental and due to change
    [SerializeField]
    [Tooltip("Finger Input that ranges from 0-1. 0 is not bend at all, 1 is bend all the way. In this order: thumb, index, middle, ring, pinky")]
    [Range(0, .999f)]   //ToDo: Change so value 1 is included, too
    float[] fingerAxis;   //Change that this will change via player Input later

    [Header("Automatically fetches fields")]
    [SerializeField]
    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: thumb, index, middle, ring, pinky")]
    Transform[] fingerGoalTargets;

    [Tooltip("Reference to each finger. In this order: thumb, index, middle, ring, pinky")]
    Transform[] fingers;

    [Tooltip("Reference to each finger tip. In this order: thumb, index, middle, ring, pinky")]
    Transform[] fingerTips;

    [Tooltip("Targets for each leg. In this order: thumb, index, middle, ring, pinky")]
    Transform[] fingerTargets;



    [Header("Static fields")]
    [SerializeField]
    int numberOfFingers = 5;

    [SerializeField]
    [Tooltip("Finger Input that ranges from 0-1. 0 is not bend at all, 1 is bend all the way")]
    float minBendAngle = 20, maxBendAngle = 170;

    [SerializeField]
    [Tooltip("Chosen Value for total finger length. Compute Later. In this order: thumb, index, middle, ring, pinky")]
    float[] fingerLength = new float[] { 3, 3, 3, 3, 3 };

    [Tooltip("For procedural finger movement. In this order: thumb, index, middle, ring, pinky")]
    Vector3[] raycastHitStartPosition;
    float[] raycastHitStartInputAxis;

    [Tooltip("For procedural finger movement. In this order: thumb, index, middle, ring, pinky")]
    bool[] raycastDidHit;

    // Start is called before the first frame update
    void Awake()
    {
        fingerAxis = new float[numberOfFingers]; 
        raycastDidHit = new bool[numberOfFingers];
        raycastHitStartPosition = new Vector3[numberOfFingers];
        raycastHitStartInputAxis = new float[numberOfFingers];

        fingers = new Transform[numberOfFingers];
        fingerTips = new Transform[numberOfFingers];
        for (int i = 0; i < numberOfFingers; i++)
        {
            raycastHitStartInputAxis[i] = -1;

            fingers[i] = transform.GetComponentsInDirectChildren<Transform>()[i];
            fingerTips[i] = fingers[i].Find("FingerElement/FingerElement/FingerElement/FingerEnd");
        }

        fingerTargets = new Transform[numberOfFingers];
        fingerTargets[0] = transform.parent.Find("ThumbRotationOffset/ThumbTurnpoint");
        fingerTargets[1] = transform.parent.Find("IndexTurnpoint");
        fingerTargets[2] = transform.parent.Find("MiddleTurnpoint");
        fingerTargets[3] = transform.parent.Find("RingTurnpoint");
        fingerTargets[4] = transform.parent.Find("PinkyTurnpoint");
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hit = new RaycastHit[numberOfFingers];
        for (int i = 0; i < numberOfFingers; i++)
        {
            Physics.Raycast(fingerTips[i].position, fingerTips[i].transform.forward, out hit[i], fingerLength[i] / 2, LayerMask.GetMask("Ground"));
            Debug.DrawRay(fingerTips[i].position, fingerTips[i].transform.forward * (fingerLength[i] / 2), Color.yellow);
        }

        //if Raycast didn't hit an object OR finger Axis is still above its starting value when hitting the raycast OR (Raycast connected before AND finger is not exactly at raycast position)
        for (int i = 0; i < numberOfFingers; i++)
        {
            if (hit[i].transform == null || fingerAxis[i] < raycastHitStartInputAxis[i])
            {
                if (raycastDidHit[i])
                {
                    raycastHitStartPosition[i] = Vector3.zero;
                    raycastHitStartInputAxis[i] = 0;
                    raycastDidHit[i] = false;

                    //fingerLength[i]*2, cause the rotation point is always at the second finger joint
                    fingerTargets[i].GetComponentInDirectChildren<Transform>().localPosition = new Vector3(0, fingerLength[i]/3*2, 0);
                }

                float rotationTurnpoint = Mathf.Lerp(minBendAngle, maxBendAngle, fingerAxis[i]);

                fingerTargets[i].localEulerAngles = new Vector3(rotationTurnpoint, 0, 0);
            }
            else
            {
                if (!raycastDidHit[i])
                {
                    raycastHitStartPosition[i] = fingerTips[i].position;
                    raycastHitStartInputAxis[i] = fingerAxis[i];
                    raycastDidHit[i] = true;
                }

                Vector3 targetPosition = Vector3.Lerp(raycastHitStartPosition[i], hit[i].point, (fingerAxis[i] - raycastHitStartInputAxis[i]) / (1 - raycastHitStartInputAxis[i]));

                fingerTargets[i].GetComponentInDirectChildren<Transform>().position = targetPosition;       //change this to goal node later
            }
        }
    }
}
