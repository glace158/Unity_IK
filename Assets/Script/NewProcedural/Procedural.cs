using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Procedural : MonoBehaviour
{
    public Transform[] legTargets;
    public float stepSize = 0.15f;

    public int smoothness = 8;

    public float stepHeight = 0.15f;

    private int nbLegs;
    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private bool[] legMoving;
    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 lastBodyPos;
    
    private float raycastRange = 1f;

    private float velocityMultiplier = 15f;
    void Start()
    {
        InitLegPosition();
    }

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

    void InitLegPosition(){
        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
        }
        lastBodyPos = transform.position;
    }

    void BodyVelocity(){
        velocity = transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        
        if (velocity.magnitude < 0.000025f){
            velocity = lastVelocity;
        }
        else{
            lastVelocity = velocity;
        }
    }

    void LegMove(){
        Vector3[] desiredPositions = new Vector3[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            //로컬공간에서 월드 공간 postion
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            //defaultLegPositions에서 부터 마지막 발위치까지의 투영된벡터
            //발위치의 방향을 계산
            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up).magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }
    }

    void Walk(int indexToMove, Vector3[] desiredPositions){
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];


        if (indexToMove != -1 && !legMoving[0])//어느 다리가 움직여야하지만 legMoving이 false인 상태
        {
            Step(indexToMove, desiredPositions[indexToMove]);
        }
    }

    void Step(int indexToMove , Vector3 desiredPosition)
    {
        Vector3 targetPoint = desiredPosition + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) * (desiredPosition - legTargets[indexToMove].position) + velocity * velocityMultiplier;
        Vector3[] positionAndNormal = MatchToSurfaceFromAbove(targetPoint, raycastRange, transform.up);
        StartCoroutine(PerformStep(indexToMove, positionAndNormal[0]));
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
    }

    void LegFixedPosition(){
        for (int i = 0; i < nbLegs; ++i)
        {
            legTargets[i].position = lastLegPositions[i];
        }
    }

    void FixedUpdate()
    {
        LegFixedPosition();
    }


}
