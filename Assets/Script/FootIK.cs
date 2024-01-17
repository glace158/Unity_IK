using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    private RaycastHit hit;
    private int layerMask;
    public Transform right_leg;
    Transform right_foot;
    public Transform left_leg;
    private RaycastHit hit_tf;
    Animator animator;
    float weight = 1.0f;
    public HumanBodyBones bone;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = 1 << 3;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        LegRaycast(right_leg);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        SetFootIK();
    }

    void LegRaycast(Transform leg)
    {
        if (Physics.Raycast(leg.position, leg.up, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Hit ground : " + hit.distance.ToString());
            Debug.DrawRay(leg.position, -transform.up * hit.distance, Color.red);
            Debug.DrawRay(hit.point, hit.normal, Color.blue);
            hit_tf = hit;
            
            //right_foot.position = hit.point;


        }
        else
        {
            Debug.DrawRay(leg.position, -transform.up * 3000f, Color.red);
        }
    }

    void SetFootIK()
    {
        Debug.Log(hit_tf.point);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, weight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, weight);
        
        animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(right_foot.position.x, hit_tf.point.y, right_foot.position.z) );

        Quaternion rot = Quaternion.LookRotation(hit_tf.point, hit_tf.normal);
        animator.SetIKRotation(AvatarIKGoal.RightFoot,rot);

    }


}
