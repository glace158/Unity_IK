using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        moving();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && !animator.GetBool("EquipGun"))
        {
            change_run_mode();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            equip_gun(!animator.GetBool("EquipGun"));
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            crouching(!animator.GetBool("Crouch"));
        }
    }

    void moving()
    {
        float speed_x = Input.GetAxis("Horizontal");//'a', 'd' °ª °¡Àú¿È
        float speed_y = Input.GetAxis("Vertical");//'w', 's' °ª °¡Àú¿È


        animator.SetFloat("Turn", speed_x);
        animator.SetFloat("Forward", speed_y);
    }

    void change_run_mode()
    {
        animator.SetTrigger("ChangeRunMode");
    }

    void equip_gun(bool state)
    {
        animator.SetBool("EquipGun", state);
    }

    void crouching(bool state)
    {
        animator.SetBool("Crouch", state);
    }
}
