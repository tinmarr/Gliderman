using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public int index = 0;

    private MenuController controller;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<MenuController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.currentIndex == index)
        {
            
            if (controller.pressed)
            {
                animator.SetInteger("state", 2);
                controller.pressed = false;
            } else
            {
                animator.SetInteger("state", 1);
            }
        } else
        {
            animator.SetInteger("state", 0);
        }
    }
}
