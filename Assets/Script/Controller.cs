using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float Speed = 6f;
    Vector3 movement;

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        move(h, v);
    }

    void move(float h, float v)
    {
        if (v == 0 && h != 0)
        {
            transform.localRotation *= Quaternion.Euler(Vector3.up * Speed * 10 *  Time.deltaTime * h);
        }
        else
        {
            movement.Set(h, 0f, v);
            movement = movement.normalized * Speed * Time.deltaTime;
            transform.Translate( movement );
        }
    }
}
