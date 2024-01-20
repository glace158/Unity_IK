using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    public Transform[] legTargets;
    public float stepSize = 1f;

    private Vector3[] defaultLegPositions;
    private Vector3[] lastLegPositions;
    private bool[] legMoving;
    private int nbLegs;
    // Start is called before the first frame update
    void Start()
    {
        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector3[nbLegs];
        lastLegPositions = new Vector3[nbLegs];
        legMoving = new bool[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].position;
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int indexToMove = -1;
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];
    }


    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(defaultLegPositions[i], stepSize);
        }
    }
}
