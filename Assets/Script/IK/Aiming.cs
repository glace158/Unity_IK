using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}

public class Aiming : MonoBehaviour
{
    public Transform target_transform;
    public Transform aim_transform;
    public Transform bone;

    public int iterations = 10;

    [Range(0, 1)]
    public float weight = 1.0f;

    public HumanBone[] humanBones;
    Transform[] bone_transform;

    public float angle_limit = 90.0f;
    public float distance_limit = 1.5f;

    public float blend_out_amount = 50.0f;

    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        bone_transform = new Transform[humanBones.Length];
        for(int i = 0; i < bone_transform.Length; i++)
        {
            bone_transform[i] = anim.GetBoneTransform(humanBones[i].bone);
        }
    }

    private void LateUpdate()
    {
        Vector3 target_position = get_target_position();
        for(int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < bone_transform.Length; j++)
            {
                Transform bone = bone_transform[j];
                float bone_weight = humanBones[j].weight * weight;
                aim_at_target(bone, target_position, bone_weight);
            }
        } 
    }

    Vector3 get_target_position()
    {
        Vector3 target_diretion = target_transform.position - aim_transform.position;
        Vector3 aim_diretion = aim_transform.forward;
        float blend_out = 0.0f;
        float target_angle = Vector3.Angle(target_diretion, aim_diretion);

        //Debug.Log("target_angle: " + target_angle);

        if(target_angle > angle_limit)
        {
            //Debug.Log("target_back");
        }

        float target_distance = target_diretion.magnitude;
        if(target_distance < distance_limit)
        {
            blend_out += distance_limit - target_distance;
        }

        Vector3 direction = Vector3.Slerp(target_diretion, aim_diretion, blend_out);
        return aim_transform.position + direction;
    }


    void aim_at_target(Transform bone, Vector3 target_position, float weight)
    {
        Vector3 aim_direction = aim_transform.forward;
        Vector3 target_diretion = target_position - aim_transform.position;
        Quaternion aim_towards = Quaternion.FromToRotation(aim_direction, target_diretion);

        Quaternion blended_rotation = Quaternion.Slerp(Quaternion.identity, aim_towards, weight);

        bone.rotation = blended_rotation * bone.rotation;
    }
}
