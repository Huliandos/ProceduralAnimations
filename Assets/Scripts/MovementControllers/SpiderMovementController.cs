using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovementController : MonoBehaviour
{
    [Tooltip("Reference to each leg. In this order: BL, FL, BR, FR")]
    Transform[] legs;
    
    [Tooltip("Targets for each leg. In this order: BL, FL, BR, FR")]
    Transform[] legTargets;

    [Tooltip("Poles for each leg. In this order: BL, FL, BR, FR")]
    Transform[] legPoles;

    [SerializeField]
    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: BL, FL, BR, FR")]
    Transform[] legGoalTargets;

    [Tooltip("Goal Targets for each leg, to check if a leg has to be moved. In this order: BL, FL, BR, FR")]
    Vector3[] legGoalTargetOffsets;

    int numberOfLegs;

    float bodyHeightOffset = 1.5f; 

    // Start is called before the first frame update
    void Awake()
    {
        #region init Spider Data
        numberOfLegs = legGoalTargets.Length;
        legGoalTargetOffsets = new Vector3[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legGoalTargetOffsets[i] = legGoalTargets[i].position + new Vector3(0, -legGoalTargets[i].position.y, 0);
        }

        legs = new Transform[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++){
            legs[i] = transform.GetComponentsInDirectChildren<Transform>()[i];
        }

        legTargets = new Transform[numberOfLegs];
        legTargets[0] = transform.parent.Find("TargetBL");
        legTargets[1] = transform.parent.Find("TargetFL");
        legTargets[2] = transform.parent.Find("TargetBR");
        legTargets[3] = transform.parent.Find("TargetFR");

        legPoles = new Transform[numberOfLegs];
        for (int i = 0; i < numberOfLegs; i++)
        {
            legPoles[i] = legs[i].Find("Pole");
        }
        #endregion

        #region init spider leg Position
        //BL
        Vector3 offset = new Vector3(1.625f, 0, -.25f);
        RaycastHit hit;
        Physics.Raycast(legs[0].position + offset, -transform.up, out hit, 3, LayerMask.GetMask("Ground"));

        if (hit.point == null)
        {
            //handling
        }
        else
        {
            legTargets[0].position = hit.point;
            legPoles[0].localPosition = new Vector3(legTargets[0].localPosition.x * 2, legTargets[0].position.y + 3.5f, legTargets[0].localPosition.z * 2);
        }

        //FL
        offset = new Vector3(1.625f, 0, .25f);
        Physics.Raycast(legs[1].position + offset, -transform.up, out hit, 3, LayerMask.GetMask("Ground"));

        if (hit.point == null)
        {
            //handling
        }
        else
        {
            legTargets[1].position = hit.point;
            legPoles[1].localPosition = new Vector3(legTargets[1].localPosition.x * 2, legTargets[1].position.y + 3.5f, legTargets[1].localPosition.z * 2);
        }

        //BR
        offset = new Vector3(-1.625f, 0, 1);
        Physics.Raycast(legs[2].position + offset, -transform.up, out hit, 3, LayerMask.GetMask("Ground"));

        if (hit.point == null)
        {
            //handling
        }
        else
        {
            legTargets[2].position = hit.point;
            legPoles[2].localPosition = new Vector3(-legTargets[2].localPosition.x * 2, legTargets[2].position.y + 3.5f, -legTargets[2].localPosition.z * 2);
        }

        //FR
        offset = new Vector3(-1.625f, 0, -1);
        Physics.Raycast(legs[3].position + offset, -transform.up, out hit, 3, LayerMask.GetMask("Ground"));

        if (hit.point == null)
        {
            //handling
        }
        else
        {
            legTargets[3].position = hit.point;
            legPoles[3].localPosition = new Vector3(-legTargets[3].localPosition.x * 2, legTargets[3].position.y + 3.5f, -legTargets[3].localPosition.z * 2);
        }
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        //constant forward movement
        transform.position += -transform.forward * 0.01f;

        //goal Target position setting
        for (int i = 0; i < numberOfLegs; i++)
        {
            Vector3 downWithoutXRotation = new Vector3(0, -transform.up.y, -transform.up.z);

            //Debug.Log("-transform.up: " + (-transform.up) + " downWitoutZRotation: " + downWithoutZRotation);
            Debug.DrawRay(transform.position + legGoalTargetOffsets[i], downWithoutXRotation, Color.yellow);

            RaycastHit hit;
            Physics.Raycast(transform.position + legGoalTargetOffsets[i], downWithoutXRotation, out hit, 3, LayerMask.GetMask("Ground"));

            if (hit.point == null)
            {
                //handling
            }
            else {
                legGoalTargets[i].position = hit.point;
            }
        }

        for (int i = 0; i < numberOfLegs; i++) {
            //projecting point onto plane defined by objects forward axis and up axis as normal 
            Vector3 projectedLegGoalTarget = legGoalTargets[i].position - transform.position;   //Vector between point on plane and point to project
            float distance = Vector3.Dot(projectedLegGoalTarget, transform.up);
            projectedLegGoalTarget = projectedLegGoalTarget - transform.up * distance;

            Vector3 projectedLegTarget = legTargets[i].position - transform.position;   //Vector between point on plane and point to project
            distance = Vector3.Dot(projectedLegTarget, transform.up);
            projectedLegTarget = projectedLegTarget - transform.up * distance;

            if ((projectedLegGoalTarget - projectedLegTarget).magnitude > 1.25f)
            {
                StartCoroutine(smoothMoveLegTargets(legTargets[i], legGoalTargets[i].position));
                //legTargets[i].position = legGoalTargets[i].position;
            }
        }

        adjustBodyHeight();
        adjustBodyRotation();
    }

    void adjustBodyHeight()
    {
        float bodyYPosition = 0;
        foreach (Transform legPos in legTargets) {
            bodyYPosition += legPos.transform.position.y;
        }
        bodyYPosition /= legTargets.Length;
        bodyYPosition += bodyHeightOffset;

        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, bodyYPosition, transform.position.z), .05f);
    }

    void adjustBodyRotation()
    {
        /*
        Debug.Log("B: " + (legTargets[0].position.y + legTargets[2].position.y) +
            " F: " + (legTargets[1].position.y + legTargets[3].position.y) +
            " diff: " + ((legTargets[0].position.y + legTargets[2].position.y) / 2 - (legTargets[1].position.y + legTargets[3].position.y) / 2) +
            " angle: " + (Mathf.Atan2((legTargets[0].position.y + legTargets[2].position.y)/2 - (legTargets[1].position.y + legTargets[3].position.y)/2,
                (legTargets[0].position.x + legTargets[1].position.x)/2 - (legTargets[2].position.x + legTargets[3].position.x)/2) * 180 / Mathf.PI));
        */

        transform.rotation = Quaternion.Lerp(transform.rotation, 
            Quaternion.Euler(new Vector3(-Mathf.Atan2((legTargets[0].position.y + legTargets[2].position.y) / 2 - (legTargets[1].position.y + legTargets[3].position.y) / 2,
                (legTargets[0].position.x + legTargets[1].position.x) / 2 - (legTargets[2].position.x + legTargets[3].position.x) / 2),
            0,
           Mathf.Atan2((legTargets[0].position.y + legTargets[1].position.y) / 2 - (legTargets[2].position.y + legTargets[3].position.y) / 2,
                (legTargets[0].position.x + legTargets[1].position.x) / 2 - (legTargets[2].position.x + legTargets[3].position.x) / 2)) * 180 / Mathf.PI), 
            .05f);
        

        //move according to extremest leg
        //back legs height difference > front legs height difference
        /*
        if (Mathf.Pow(legTargets[2].position.y - legTargets[0].position.y, 2) > Mathf.Pow(legTargets[1].position.y - legTargets[3].position.y, 2))
        {
            //Debug.Log(new Vector3(0, 0, Mathf.Atan2(legTargets[0].position.y, legTargets[2].position.y)));
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(legTargets[0].position.y - legTargets[2].position.y, legTargets[0].position.x - legTargets[2].position.x)) * 180 / Mathf.PI), .05f);
        }
        else
        {
            //Debug.Log(new Vector3(0, 0, Mathf.Atan2(legTargets[1].position.y, legTargets[3].position.y)));
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(legTargets[1].position.y - legTargets[3].position.y, legTargets[1].position.x - legTargets[3].position.x)) * 180 / Mathf.PI), .05f);
        }
        */
    }

    IEnumerator smoothMoveLegTargets(Transform legTarget, Vector3 legGoalTarget) {
        float t = 0;
        Vector3 legTargetStartingPos = legTarget.position;

        while (t <= 1)
        {
            t += .1f;
            t = Mathf.Round(t * 10) / 10;   //to solve rounding error where t gets incremented by more than .1f

            //raises and lowers the leg linear
            //ToDo? Change movement to parabola
            if (t<=.5f)
                legTarget.position = Vector3.Lerp(legTargetStartingPos, legGoalTarget + new Vector3(0, 1f, 0), t);
            else
                legTarget.position = Vector3.Lerp(legTargetStartingPos + new Vector3(0, 1f, 0), legGoalTarget, t);

            yield return new WaitForSeconds(Time.deltaTime);
        }

        yield return null;
    }
}
