using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpiderController : MonoBehaviour
{
    public float _speed = 1f;
    public float _trun_gain = 60f;
    public float smoothness = 5f;
    public Transform look_target;
    
    private Rigidbody _rigidbody;
    private float defaultup;
    Vector3 movement;
    private float lastRot;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        defaultup = transform.localPosition.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        fixedup();
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        move(h, v);
    }

    void move(float h, float v)
    {
        if (v == 0 && h != 0)
        {
            look_target.localRotation *= Quaternion.Euler(transform.up * _speed * _trun_gain * Time.deltaTime * h);
        }
        else
        {
            movement.Set(h, 0f, v);
            movement = movement.normalized * _speed * Time.deltaTime;
            transform.Translate(movement);
        }
    }

    private void LookAtSlowlY(Transform target, float speed = 1f)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.z = 0f;

        var nextRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, nextRot, Time.deltaTime * speed);
    }

    private void RotateAround1(Transform target)
    {
        lastRot += _speed * Time.deltaTime;

        Vector3 diff = transform.position - target.position;
        diff.z = target.position.z;

        Vector3 offset = Quaternion.AngleAxis(lastRot, transform.up) * diff;
        target.position = transform.position + offset;
    }

    void fixedup() {
        Ray ray = new Ray(transform.localPosition, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit info, 100))
        {
            if (Mathf.Abs(defaultup - Vector3.Distance(transform.localPosition, info.point)) > 0.0025)
            {
                float dis = defaultup - Vector3.Distance(transform.position, info.point);
                Vector3 pos = Vector3.Lerp(transform.localPosition, transform.localPosition + (transform.up * dis), Time.deltaTime * 6.0f);
                transform.localPosition = pos;
            }
        }
    }
}
