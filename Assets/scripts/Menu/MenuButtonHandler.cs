using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonHandler : MonoBehaviour, IPointerEnterHandler
{
    public int index = 0;

    public bool unpressable = false;
    public UnityEvent OnPress;
    public UnityEvent OnHover;

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
            
            if (controller.pressed && !unpressable)
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

    public void Press()
    {
        OnPress.Invoke();
    }

    public void Hover()
    {
        OnHover.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.currentIndex = index;
    }
}
