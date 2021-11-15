using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuddyMovementController : MonoBehaviour
{
    [Tooltip("Reference to each leg. In this order: L, R")]
    Transform[] legs;

    [Tooltip("Targets for each leg. In this order: L, R")]
    Transform[] legTargets;

    [Tooltip("Poles for each leg. In this order: L, R")]
    Transform[] legPoles;

    [SerializeField]
    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    Transform[] legGoalTargets;

    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: L, R")]
    Vector3[] legGoalTargetOffsets;

    bool[] legMoving;

    int numberOfLegs;

    float bodyHeightOffset = .7f;

    [Tooltip("Reference to each arm. In this order: L, R")]
    Transform[] arms;

    [Tooltip("Targets for each arm. In this order: L, R")]
    Transform[] armTargets;

    [SerializeField]
    [Tooltip("Goal Targets for each arm, to check if a arm has to be moved. In this order: L, R")]
    Transform[] armGoalTargets;

    float maxArmBodyDistance = 2.5f;
    int numberOfArms;

    
    float yaw, rotationSpeed = 2f;

    void Awake()
    {
        #region init leg Data
        numberOfLegs = legGoalTargets.Length;
        legGoalTargetOffsets = new Vector3[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legGoalTargetOffsets[i] = legGoalTargets[i].position + new Vector3(0, -legGoalTargets[i].position.y, 0);
        }

        legs = new Transform[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legs[i] = transform.GetComponentsInDirectChildren<Transform>()[i];
        }
        
        legTargets = new Transform[numberOfLegs];
        legTargets[0] = transform.parent.Find("TargetLeftLeg");
        legTargets[1] = transform.parent.Find("TargetRightLeg");

        legPoles = new Transform[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legPoles[i] = legs[i].Find("Pole");
        }
        #endregion

        #region init arm Data
        numberOfArms = armGoalTargets.Length;

        arms = new Transform[numberOfArms];
        for (int i = 2; i < 2+numberOfArms; i++)
        {
            arms[i-2] = transform.GetComponentsInDirectChildren<Transform>()[i];
        }

        armTargets = new Transform[numberOfArms];
        armTargets[0] = transform.parent.Find("TargetLeftArm");
        armTargets[1] = transform.parent.Find("TargetRightArm");
        #endregion

        legMoving = new bool[numberOfLegs];
    }

    // Update is called once per frame
    void Update()
    {
        #region legMovement
        //goal Target position setting
        for (int i = 0; i < numberOfLegs; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + transform.TransformDirection(legGoalTargetOffsets[i]), -transform.up, out hit, 3, LayerMask.GetMask("Ground"));

            if (hit.point == null)
            {
                //handling
            }
            else
            {
                legGoalTargets[i].position = hit.point;
            }
        }

        for (int i = 0; i < numberOfLegs; i++)
        {
            //if other leg ist moving
            foreach (bool isMoving in legMoving) {
                if (isMoving)
                    return;
            }

            //projecting point onto plane defined by objects forward axis and up axis as normal 
            Vector3 projectedLegGoalTarget = legGoalTargets[i].position - transform.position;   //Vector between point on plane and point to project
            float distance = Vector3.Dot(projectedLegGoalTarget, transform.up);
            projectedLegGoalTarget = projectedLegGoalTarget - transform.up * distance;

            Vector3 projectedLegTarget = legTargets[i].position - transform.position;   //Vector between point on plane and point to project
            distance = Vector3.Dot(projectedLegTarget, transform.up);
            projectedLegTarget = projectedLegTarget - transform.up * distance;

            //if distance between goal and target is too big
            if ((projectedLegGoalTarget - projectedLegTarget).magnitude > .25f)
            {
                legMoving[i] = true;
                StartCoroutine(smoothMoveLegTargets(legTargets[i], legGoalTargets[i].position, i));
                //legTargets[i].position = legGoalTargets[i].position;
            }
        }
        #endregion
    }

    private void FixedUpdate()
    {
        #region characterMovement
        RaycastHit hit;
        Physics.Raycast(transform.position, -transform.forward, out hit, .3f);

        if (hit.point.magnitude <= 0 && Input.GetAxis("Vertical") > 0)
        {
            if(Input.GetAxis("Fire3") > 0) //LShift
                transform.position += -transform.forward * 0.045f;
            else
                transform.position += -transform.forward * 0.02f;

            if (legGoalTargets[0].position.z != -0.36f) {
                for (int i = 0; i < numberOfLegs; i++) {
                    legGoalTargetOffsets[i] = new Vector3(legGoalTargetOffsets[i].x, legGoalTargetOffsets[i].y, -0.18f);
                }
            }
        }

        Physics.Raycast(transform.position, transform.forward, out hit, .3f);

        if (hit.point.magnitude <= 0 && Input.GetAxis("Vertical") < 0)
        {
            transform.position += transform.forward * 0.02f;

            if (legGoalTargets[0].position.z != 0.36f)
            {
                for (int i = 0; i < numberOfLegs; i++)
                {
                    legGoalTargetOffsets[i] = new Vector3(legGoalTargetOffsets[i].x, legGoalTargetOffsets[i].y, 0.18f);
                }
            }
        }
        
        yaw += rotationSpeed * Input.GetAxis("Horizontal");
        
        transform.eulerAngles = new Vector3(0, yaw, 0);
        #endregion

        if (Input.GetAxis("Fire1") > 0)
        {
            armGoalTargets[0].localPosition = Vector3.Lerp(armGoalTargets[0].localPosition, transform.InverseTransformDirection(Camera.main.transform.forward.normalized * maxArmBodyDistance), .1f);
        }
        else if (Input.GetAxis("Fire1") <= 0) {
            arms[0].GetComponentInChildren<AttachJoint>().releaseBody();
        }
        if (Input.GetAxis("Fire2") > 0)
        {
            armGoalTargets[1].localPosition = Vector3.Lerp(armGoalTargets[1].localPosition, transform.InverseTransformDirection(Camera.main.transform.forward.normalized * maxArmBodyDistance), .1f);
        }
        else if (Input.GetAxis("Fire2") <= 0)
        {
            arms[1].GetComponentInChildren<AttachJoint>().releaseBody();
        }

        for (int i = 0; i < numberOfArms; i++)
            armTargets[i].position = Vector3.Lerp(armTargets[i].position, armGoalTargets[i].position, .1f);

        adjustBodyHeight();

    }

    void adjustBodyHeight() {
        //Vector3 goal = new Vector3(transform.position.x, (legGoalTargets[0].position.y + legGoalTargets[1].position.y) / 2 + .7f, transform.position.z);
        //transform.position = Vector3.Lerp(transform.position, goal, .5f);
        
        /*
        if (legGoalTargets[0].position.y > legGoalTargets[1].position.y)
            transform.position = Vector3.Lerp(transform.position, legGoalTargets[0].position + new Vector3(-legGoalTargets[0].position.x + transform.position.x, .7f, -legGoalTargets[0].position.z + transform.position.z), .2f);
        else
            transform.position = Vector3.Lerp(transform.position, legGoalTargets[1].position + new Vector3(-legGoalTargets[1].position.x + transform.position.x, .7f, -legGoalTargets[1].position.z + transform.position.z), .2f);
        */

        float bodyYPosition = 0;
        foreach (Transform legPos in legTargets)
        {
            bodyYPosition += legPos.transform.position.y;
        }
        bodyYPosition /= legTargets.Length;
        bodyYPosition += bodyHeightOffset;

        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, bodyYPosition, transform.position.z), .05f);
    }

    IEnumerator smoothMoveLegTargets(Transform legTarget, Vector3 legGoalTarget, int arrayPos)
    {
        float t = 0;
        Vector3 legTargetStartingPos = legTarget.position;

        while (t <= 1)
        {
            if (Input.GetAxis("Fire3") > 0) //LShift
                t += .5f;
            else
                t += .2f;
            t = Mathf.Round(t * 10) / 10;   //to solve rounding error where t gets incremented by more than .1f

            //raises and lowers the leg linearly
            //ToDo? Change movement to parabola
            
            if (t <= .5f)
                legTarget.position = Vector3.Lerp(legTargetStartingPos, legGoalTarget + new Vector3(0, .25f, 0), t);
            else
                legTarget.position = Vector3.Lerp(legTargetStartingPos + new Vector3(0, .25f, 0), legGoalTarget, t);

            yield return new WaitForSeconds(Time.deltaTime);
        }
        legMoving[arrayPos] = false;

        yield return null;
    }
}
