using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpiderProceduralAnimation : MonoBehaviour
{
    public Transform[] legTargets;
    public float stepSize = 0.15f;
    public int smoothness = 8;
    public float stepHeight = 0.15f;
    public bool bodyOrientation = true;
    public Transform look_target;

    private float raycastRange = 1f;
    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private Vector3 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;

    private float velocityMultiplier = 15f;
    static Vector3[] MatchToSurfaceFromAbove(Vector3 point, float halfRange, Vector3 up)
    {
        Vector3[] res = new Vector3[2];
        res[1] = Vector3.zero;
        RaycastHit hit;
        //Ray ray = new Ray(point + halfRange * up, -up);
        Ray ray = new Ray(point + halfRange * up / 2f, -up);

        //if (Physics.Raycast(ray, out hit, 2f * halfRange))
        if (Physics.SphereCast(ray, 0.125f, out hit, 2f * halfRange))
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }

    void Start()
    {
        lastBodyUp = transform.up;

        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = new bool[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }
        lastBodyPos = transform.position;
    }

    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for (int i = 1; i <= smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));
            legTargets[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        legMoving[0] = false;
    }

    void CrawlWalk(int indexToMove, Vector3[] desiredPositions)
    {
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];


        if (indexToMove != -1 && !legMoving[0])//어느 다리가 움직여야하지만 legMoving이 false인 상태
        {
            Step(indexToMove, desiredPositions[indexToMove]);
        }

    }

    void TrotWalk(int indexToMove, Vector3[] desiredPositions)
    {
        if(indexToMove != -1)
        {
            indexToMove = indexToMove <= 1 ? 0 : 2;
        }
        
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove && (i+1) != indexToMove || -1 == indexToMove)//움직이지 않은 다리 고정
                legTargets[i].position = lastLegPositions[i];
        
        Debug.Log(indexToMove.ToString() + " " + legMoving[0].ToString() + " "+ legMoving[1].ToString() + " " + legMoving[2].ToString() + " " + legMoving[3].ToString());
        if (indexToMove != -1 && !legMoving[0])//어느 다리가 움직여야하지만 legMoving이 false인 상태
        {
            Step(indexToMove, desiredPositions[indexToMove]);
            Step(indexToMove+1, desiredPositions[indexToMove+1]);
        }
    }

    void Step(int indexToMove , Vector3 desiredPosition)
    {
        Vector3 targetPoint = desiredPosition + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) * (desiredPosition - legTargets[indexToMove].position) + velocity * velocityMultiplier;
        Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, transform.up);
        legMoving[0] = true;
        StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
    }

    void FixedUpdate()
    {
        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;


        Vector3[] desiredPositions = new Vector3[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up).magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }

        //CrawlWalk(indexToMove, desiredPositions);
        TrotWalk(indexToMove, desiredPositions);


        //몸통 기울기 조정
        lastBodyPos = transform.position;
        if (nbLegs > 3 && bodyOrientation) 
        {
            Vector3 v1 = legTargets[0].localPosition - legTargets[1].localPosition;
            Vector3 v2 = legTargets[2].localPosition - legTargets[3].localPosition;
            Vector3 normal = Vector3.Cross(v1, v2).normalized;
            Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
            transform.up = up;
            lastBodyUp = up;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, look_target.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]), stepSize);
        }
    }
}
