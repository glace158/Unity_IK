using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootRrocedural : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default; 
    [SerializeField] IKFootRrocedural otherFoot = default;
    [SerializeField] float stepDistance = 1;
    [SerializeField] float stepHeight = 1;
    [SerializeField] float stepLength = 1;
    [SerializeField] float speed = 4;

    [SerializeField] Vector3 footOffset = default;

    float footSpacingX;
    float footSpacingZ;

    Vector3 oldPosition, currentPosition, newPosition;
    Vector3 oldNormal, currentNormal, newNormal;
    float lerp;

    // Start is called before the first frame update
    void Start()
    {
        footSpacingX = transform.localPosition.x;
        footSpacingZ = transform.localPosition.z;
        currentPosition = newPosition = oldPosition = transform.position;
        currentNormal = newNormal = oldNormal = transform.up;
        lerp = 1;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentPosition;
        transform.up = currentNormal;

        Ray ray = new Ray(body.position + (body.right * footSpacingX) + (body.forward * footSpacingZ), Vector3.down);
        Debug.DrawRay(body.position + (body.right * footSpacingX) + (body.forward * footSpacingZ), Vector3.down * 10, Color.red);
        
        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                int direction = body.InverseTransformPoint(info.point).z > body.InverseTransformPoint(newPosition).z ? 1 : -1;
                newPosition = info.point + (body.forward * stepLength * direction) + footOffset;
                newNormal = info.normal;
            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = tempPosition;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
            oldNormal = newNormal;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.2f);
    }
    public bool IsMoving()
    {
        return lerp < 1;
    }
}
