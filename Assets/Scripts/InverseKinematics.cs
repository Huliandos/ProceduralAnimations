using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InverseKinematics : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Total number of bones connected to this IK chain")]
    int chainLength = 1;
    
    [SerializeField]
    [Tooltip("Target position for the bone")]
    Transform target;
    
    [SerializeField]
    [Tooltip("Bend direction for chain")]
    Transform pole;

    #region solverParams
    [Header("Solver Params")]

    [SerializeField]
    [Tooltip("Solver iterations per Update. Higher value leads to higher percision")]
    int iterations = 10;
    
    [SerializeField]
    [Tooltip("Distance at which solver stops. Lower value leads to more iterations and higher precision")]
    float distanceToGoal = .001f;
    
    [SerializeField]
    [Tooltip("Strength of snapping back to start position")]
    [Range(0, 1)]
    float snapBackStrength = 1;
    #endregion

    #region classVariables
    float[] bonesLength;
    float completeLength;
    Transform[] bones;
    Vector3[] positions;
    //rotation
    Vector3[] startDirectionToSuccesor;
    Quaternion[] startRotationBone;
    Quaternion startRotationTarget;
    Quaternion startRotationRoot;
    #endregion

    float timeSpend;
    int numOfIterations;

    int numsOfFramesPassed;

    // Start is called before the first frame update
    void Awake()
    {
        init();
    }

    void init() {
        //initial arrays
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];
        completeLength = 0;

        startDirectionToSuccesor = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];

        //init Target
        if (!target) {
            target = new GameObject(gameObject.name + " target").transform;
            target.position = transform.position;
        }
        startRotationTarget = target.rotation;

        //init data
        Transform currentTrans = transform;
        for (int i = bones.Length - 1; i >= 0; i--) {
            bones[i] = currentTrans;
            startRotationBone[i] = currentTrans.rotation;

            //if leaf bone (last bone)
            if (i == bones.Length - 1)
            {
                startDirectionToSuccesor[i] = target.position - currentTrans.position;
            }
            else {
                startDirectionToSuccesor[i] = bones[i + 1].position - currentTrans.position;
                bonesLength[i] = startDirectionToSuccesor[i].magnitude;
                completeLength += bonesLength[i];
            }

            //preping for next iteration
            currentTrans = currentTrans.parent;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        resolveIK();
    }

    void resolveIK() {
        float startTimeFrame = Time.realtimeSinceStartup;

        if (target == null)
            return;

        if (bonesLength.Length != chainLength)
            init();

        //get positions
        for (int i = 0; i < bones.Length; i++)
            positions[i] = bones[i].position;

        Quaternion rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        Quaternion rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);

        //calc positions
        //case 1: is the target out of reach or at max length
        if ((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
        {
            //stretch it
            Vector3 direction = (target.position - positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < positions.Length; i++)
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
        }
        //case 2: is the target within reach
        else
        {
            for (int i = 0; i < positions.Length - 1; i++)
                positions[i + 1] = Vector3.Lerp(positions[i+1], positions[i] + rootRotDiff * startDirectionToSuccesor[i], snapBackStrength);

            //FABRIK (Forwards and Backwards reaching inverse Kinematics) Algorithm
            int iteration;
            for (iteration = 0; iteration < iterations; iteration++)
            {
                //forwards
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                        positions[i] = target.position;
                    else
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i];
                }

                //backwards
                for (int i = 1; i < positions.Length; i++)
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];

                //if target is within delta reach
                if ((positions[positions.Length - 1] - target.position).sqrMagnitude < distanceToGoal * distanceToGoal)
                    break;
            }
            numOfIterations += iteration;
        }

        //bend towards pole
        if (pole) {
            //for all bones between leaf bone and root bone
            for (int i = 1; i < positions.Length - 1; i++) {
                Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
                Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        //set positions
        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].position = positions[i];

            if (i == positions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
                //bones[i].position += new Vector3(0, 2, 0);
                //bones[i].rotation = Quaternion.FromToRotation(startDirectionToSuccesor[i], positions[i + 1] - positions[i]) * startRotationBone[i];
            }
            else
                bones[i].rotation = Quaternion.FromToRotation(startDirectionToSuccesor[i], positions[i + 1] - positions[i]) * startRotationBone[i];
        }

        //Debug.Log(Time.realtimeSinceStartup - startTimeFrame);
        timeSpend += Time.realtimeSinceStartup - startTimeFrame;
        numsOfFramesPassed++;

        if (numsOfFramesPassed % 100 == 0) { 
            Debug.Log("Average time spend: " + (timeSpend / numsOfFramesPassed));
            Debug.Log("Average iteration num: " + (numOfIterations / numsOfFramesPassed));

        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Transform currentTrans = transform;
        for (int i = 0; i < chainLength && currentTrans != null && currentTrans.parent != null; i++) {
            float scale = Vector3.Distance(currentTrans.position, currentTrans.parent.position) * .1f;
            //Debug.Log("Scale: " + scale);
            Handles.matrix = Matrix4x4.TRS(currentTrans.position, Quaternion.FromToRotation(Vector3.up, currentTrans.parent.position - currentTrans.position), new Vector3(scale, Vector3.Distance(currentTrans.parent.position, currentTrans.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * .5f, Vector3.one);

            //preping for next iteration
            currentTrans = currentTrans.parent;
        }
#endif
    }
}
