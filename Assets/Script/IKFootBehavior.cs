using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKFootBehavior : MonoBehaviour
{
    [SerializeField] private Transform footTransformRF;
    [SerializeField] private Transform footTransformRB;
    [SerializeField] private Transform footTransformLB;
    [SerializeField] private Transform footTransformLF;
    private Transform[] allFootTransforms;
    [SerializeField] private Transform footTargetTransformRF;
    [SerializeField] private Transform footTargetTransformRB;
    [SerializeField] private Transform footTargetTransformLB;
    [SerializeField] private Transform footTargetTransformLF;
    private Transform[] allFootTargetTransforms;
    [SerializeField] private GameObject footRigRF;
    [SerializeField] private GameObject footRigRB;
    [SerializeField] private GameObject footRigLB;
    [SerializeField] private GameObject footRigLF;
    private TwoBoneIKConstraint[] allFootIKConstrains;
    private LayerMask groundLayerMask;
    private float maxHitDistance = 5f;
    private float addedHeight = 3f; 

    private bool[] allGroundSpherecastHits;
    private LayerMask hitLayer;
    private Vector3[] allHitNormals;
    private float angleAboutX;
    private float angleAboutZ;

    private float yOffSet;

    [SerializeField] private Animator animator;
    private float[] allFootWeights;
    
    private Vector3 aveHitNormal;

    [SerializeField, Range(-0.5f, 2)] private float upperFootYLimit = 0.3f;
    [SerializeField, Range(-2,0.5f)] private float lowerFootYLimit = -0.1f;
    private int[] checkLocalTargetY;
    private CapsuleCollider capsuleCollider;

    // Start is called before the first frame update
    private void Start()
    {
        allFootTargetTransforms = new Transform[4];
        allFootTargetTransforms[0] = footTargetTransformRF;
        allFootTargetTransforms[1] = footTargetTransformRB;
        allFootTargetTransforms[2] = footTargetTransformLB;
        allFootTargetTransforms[3] = footTargetTransformLF;

        allFootTransforms = new Transform[4];
        allFootTransforms[0] = footTransformRF;
        allFootTransforms[1] = footTransformRB;
        allFootTransforms[2] = footTransformLB;
        allFootTransforms[3] = footTransformLF;

        allFootIKConstrains = new TwoBoneIKConstraint[4];
        allFootIKConstrains[0] = footRigRF.GetComponent<TwoBoneIKConstraint>();
        allFootIKConstrains[1] = footRigRB.GetComponent<TwoBoneIKConstraint>();
        allFootIKConstrains[2] = footRigLB.GetComponent<TwoBoneIKConstraint>();
        allFootIKConstrains[3] = footRigLF.GetComponent<TwoBoneIKConstraint>();

        groundLayerMask = LayerMask.NameToLayer("Ground");

        allGroundSpherecastHits = new bool[5];

        allHitNormals = new Vector3[4];

        allFootWeights = new float[4]; 

        checkLocalTargetY = new int[4];

        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        RotateCharacterFeet();
        RotateCharacterBody();
    }

    private void CheckGroundBelow(out Vector3 hitPoint, out bool gotGroundSpherecastHit, out Vector3 hitNormal, out LayerMask hitLayer, 
        out float currentHitDistance, Transform objectTransform, int checkForLayerMask, float maxHitDistance, float addedHeight)
    {
        RaycastHit hit;
        Vector3 startSpherecast = objectTransform.position + new Vector3(0f, addedHeight, 0f);
        if(checkForLayerMask == -1)
        {
            //Debug.LogError("Layer does not exist!");
            gotGroundSpherecastHit = false;
            currentHitDistance = 0f;
            hitLayer = LayerMask.NameToLayer("Player");
            hitNormal = Vector3.up;
            hitPoint = objectTransform.position;
        }
        else
        {
            int layerMask = (1 << checkForLayerMask);
            if(Physics.SphereCast(startSpherecast, .2f, Vector3.down, out hit, maxHitDistance, layerMask, QueryTriggerInteraction.UseGlobal))
            {
                hitLayer = hit.transform.gameObject.layer;
                currentHitDistance = hit.distance - addedHeight;
                hitNormal = hit.normal;
                gotGroundSpherecastHit = true;
                hitPoint = hit.point;
            }
            else
            {
                gotGroundSpherecastHit = false;
                currentHitDistance = 0f;
                hitLayer = LayerMask.NameToLayer("Player");
                hitNormal = Vector3.up;
                hitPoint = objectTransform.position;

            }
        }
        
    }

    Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 hitNormal){
        return vector - hitNormal * Vector3.Dot(vector, hitNormal);
    }

    private void ProjectedAxisAngles(out float angleAboutX, out float angleAboutZ, Transform footTargetTransform, Vector3 hitNormal)
    {
        Vector3 xAxisProjected = ProjectOnContactPlane(footTargetTransform.forward, hitNormal).normalized;
        Vector3 zAxisProjected = ProjectOnContactPlane(footTargetTransform.right, hitNormal).normalized;

        angleAboutX = Vector3.SignedAngle(footTargetTransform.forward, xAxisProjected, footTargetTransform.right);
        angleAboutZ = Vector3.SignedAngle(footTargetTransform.right, zAxisProjected, footTargetTransform.forward);
    }

    private void RotateCharacterFeet()
    {   
        allFootWeights[0] = animator.GetFloat("RF Foot Weight");
        allFootWeights[1] = animator.GetFloat("RB Foot Weight");
        allFootWeights[2] = animator.GetFloat("LB Foot Weight");
        allFootWeights[3] = animator.GetFloat("LF Foot Weight");

        for (int i = 0; i < 4; i++){
            allFootIKConstrains[i].weight = allFootWeights[i];


            CheckGroundBelow(out Vector3 hitPoint, out allGroundSpherecastHits[i], out Vector3 hitNormal, out hitLayer, out _,
                allFootTransforms[i], groundLayerMask, maxHitDistance, addedHeight);
            allHitNormals[i] = hitNormal;
            
            if (allGroundSpherecastHits[i] == true)
            {
                yOffSet = 0.15f;

                /*
                if(allFootTransforms[i].position.y < allFootTargetTransforms[i].position.y - 0.1f)
                {
                    yOffSet += allFootTargetTransforms[i].position.y - allFootTransforms[i].position.y;
                }
                */
                ProjectedAxisAngles(out angleAboutX, out angleAboutZ, allFootTransforms[i], allHitNormals[i]);

                allFootTargetTransforms[i].position = new Vector3(allFootTransforms[i].position.x, hitPoint.y + yOffSet, allFootTransforms[i].position.z);

                allFootTargetTransforms[i].rotation = allFootTransforms[i].rotation;

                allFootTargetTransforms[i].localEulerAngles = new Vector3(allFootTargetTransforms[i].localEulerAngles.x + angleAboutX,
                    allFootTargetTransforms[i].localEulerAngles.y, allFootTargetTransforms[i].localEulerAngles.z + angleAboutZ);
            }
            else
            {
                allFootTargetTransforms[i].position = allFootTransforms[i].position;
                allFootTargetTransforms[i].rotation = allFootTransforms[i].rotation;
            }
        }
    }

    private void RotateCharacterBody()
    {
        float maxRotationStep = 1f;
        float aveHitNormalX = 0f;
        float aveHitNormalY = 0f;
        float aveHitNormalZ = 0f;
        for (int i = 0; i < 4; i++)
        {
            aveHitNormalX += allHitNormals[i].x;
            aveHitNormalY += allHitNormals[i].y;
            aveHitNormalZ += allHitNormals[i].z;
        }
        aveHitNormal = new Vector3(aveHitNormalX/4, aveHitNormalY/4, aveHitNormalZ/4).normalized;

        ProjectedAxisAngles(out angleAboutX, out angleAboutZ, transform, aveHitNormal);

        float maxRotationX = 50;
        float maxRotationZ = 20;

        float characterXRotation = transform.eulerAngles.x;
        float characterZRotation = transform.eulerAngles.z;

        if (characterXRotation >180)
        {
            characterXRotation -= 360;
        }
        if (characterZRotation > 180)
        {
            characterZRotation -= 360;
        }

        if (characterXRotation + angleAboutX < -maxRotationX)
        {
            angleAboutX = maxRotationX + characterXRotation;
        }
        else if (characterXRotation + angleAboutX > maxRotationX)
        {
            angleAboutX = maxRotationX = characterXRotation;
        }

        if (characterZRotation + angleAboutZ < -maxRotationZ)
        {
            angleAboutZ = maxRotationZ + characterZRotation;
        }
        else if (characterZRotation + angleAboutZ > maxRotationZ) 
        {
            angleAboutZ = maxRotationZ - characterZRotation;
        }

        float bodyEulerX = Mathf.MoveTowardsAngle(0, angleAboutX, maxRotationStep);
        float bodyEulerZ = Mathf.MoveTowardsAngle(0, angleAboutZ, maxRotationStep);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x + bodyEulerX, transform.eulerAngles.y, transform.eulerAngles.z + angleAboutZ);
    }

    private void CharacterHeightAdjustements()
    {
        for(int i = 0; i<4; i++)
        {
            if(allFootTargetTransforms[i].localPosition.y < upperFootYLimit && allFootTargetTransforms[i].localPosition.y > lowerFootYLimit)
            {
                checkLocalTargetY[i] = 0;
            }
            else if (allFootTargetTransforms[i].localPosition.y > upperFootYLimit)
            {   
                //squishy leg
                checkLocalTargetY[i] = 1;
            }
            else
            {
                // stretchy leg
                checkLocalTargetY[i] = -1;
            }
        }
        if (checkLocalTargetY[0] == 1 && checkLocalTargetY[2] == 1 || checkLocalTargetY[1] == 1 && checkLocalTargetY[3] == 1)
        {   
            if (capsuleCollider.center.y > -1.4){
                capsuleCollider.center -= new Vector3(0f, 0.05f, 0f);
            }
            else
            {
                capsuleCollider.center = new Vector3(0f, 3.4f, 0f);
            }
        }
        else if(checkLocalTargetY[0] == -1 && checkLocalTargetY[2] == -1 || checkLocalTargetY[1] == 1 && checkLocalTargetY[3] == -1)
        {
            if( capsuleCollider.center.y < 1.5)
            {
                capsuleCollider.center += new Vector3(0f, 0.05f, 0f);
            }
        }
    }
}
