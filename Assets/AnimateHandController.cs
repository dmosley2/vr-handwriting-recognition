using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class AnimateHandController : MonoBehaviour
{


    public InputActionReference gripInputActionReference;
    public InputActionReference triggerInputActionReference;
    private Animator handAnimator;
    private float gripValue;
    private float triggerValue;
    private AnimateHandController aController = null;
    // Start is called before the first frame update
    void Start()
    {
        handAnimator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update()
    {
        GripAnimate();
        TriggerAnimate();
    }

    private void TriggerAnimate()
    {
        triggerValue = triggerInputActionReference.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);
    }

    private void GripAnimate()
    {
        gripValue = gripInputActionReference.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
    }
    // Complete the TriggerAnimate function
    public float GetGripValue()
    {
        return gripValue;
    }
    public float GetTriggerValue()
    {
        return triggerValue;
    }
    
}