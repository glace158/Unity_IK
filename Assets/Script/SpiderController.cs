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
   
    private float defaultup;
    Vector3 movement;
    private bool turn_mode = true;

    void Start()
    {
        defaultup = transform.localPosition.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        fixedup();
        if (Input.GetKey(KeyCode.E))
        {
            turn_mode = !turn_mode;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (turn_mode)
        {
            turn_move(h, v); 
        }
        else
        {
            move(h, v);
        }
    }

    void move(float h, float v)
    {
        movement.Set(h, 0f, v);
        movement = movement.normalized * _speed * Time.deltaTime;
        transform.Translate(movement);
    }

    void turn_move(float h, float v)
    {
        if (h != 0)
        {
            look_target.Rotate(look_target.up * _speed * _trun_gain * Time.deltaTime * h);
            transform.rotation = look_target.rotation;
        }

        if (v != 0)
        {
            movement.Set(0f, 0f, v);
            movement = movement.normalized * _speed * Time.deltaTime;
            transform.Translate(movement);
        }
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
