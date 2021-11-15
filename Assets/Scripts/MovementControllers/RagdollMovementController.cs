using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollMovementController : MonoBehaviour
{
    [SerializeField]
    Rigidbody spine, leftHand, rightHand, leftFoot, rightFoot;

    float movementSpeed = .02f;
    // Start is called before the first frame update
    void Start()
    {
        spine.useGravity = false;
        spine.isKinematic = true;

        leftHand.useGravity = false;
        rightHand.useGravity = false;
        leftFoot.useGravity = false;
        rightFoot.useGravity = false;

        StartCoroutine(footMovement(1, leftFoot));
        StartCoroutine(footMovement(0, rightFoot));

        StartCoroutine(handMovement(1, leftHand, true));
        StartCoroutine(handMovement(0, rightHand, false));
    }

    // Update is called once per frame
    void Update()
    {
        spine.MovePosition(new Vector3(spine.transform.position.x, spine.transform.position.y, spine.transform.position.z - movementSpeed));
    }

    IEnumerator footMovement(float stepDelay, Rigidbody footToMove)
    {
        yield return new WaitForSeconds(stepDelay);

        while (true)
        {

            footToMove.AddForce(new Vector3(0, 200, -500));

            /*
            float t = 0;
            Vector3 footStartingPos = footToMove.position;
            Vector3 footGoalPos = footToMove.position + new Vector3(0, 0, -1);
            
            while (t <= 1)
            {
                t += .1f;
                t = Mathf.Round(t * 10) / 10;   //to solve rounding error where t gets incremented by more than .1f

                //raises and lowers the leg linear
                //ToDo? Change movement to parabola
                if (t <= .5f)
                    footToMove.MovePosition(Vector3.Lerp(footStartingPos, footGoalPos + new Vector3(0, 1f, 0), t));
                else
                    footToMove.MovePosition(Vector3.Lerp(footStartingPos + new Vector3(0, 1f, 0), footGoalPos, t));

                yield return new WaitForSeconds(Time.deltaTime);
            }
            */

            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator handMovement(float startDelay, Rigidbody handToMove, bool moveForward)
    {
        yield return new WaitForSeconds(startDelay);

        float force;

        if (moveForward) force = -400;
        else force = 400;

        while (true)
        {
            handToMove.AddForce(new Vector3(0, 0, force));

            force *= -1;

            yield return new WaitForSeconds(1f);
        }
    }
}
