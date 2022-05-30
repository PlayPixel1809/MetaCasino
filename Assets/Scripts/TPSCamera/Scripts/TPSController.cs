﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]

public class TPSController : MonoBehaviour
{
    public Text nameTxt;

    public Joystick joystick;
    
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    

    CharacterController characterController;

    private bool isIdle = true;
    private bool joystickActive;

    public Animation anims;
    public Transform lookAtHelper;

    public AudioSource audioSource;
    public AudioClip run;
    public Action onMove;
    public Action onCharacterIdle;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (joystick != null) { joystick.onJoystick += OnJoystick; }
        if (joystick != null) { joystick.onJoystickUp += OnJoystickUp; }
    }

    void OnJoystick(Vector3 dir, bool fullIntensity)
    {
        joystickActive = true;
        if (fullIntensity)  { Run(dir);  } else { Walk(dir); }
    }

    void OnJoystickUp()
    {
        joystickActive = false;
        Idle();
    }
   
    void Idle()
    {
        if (isIdle) { return; }
        isIdle = true;
        anims.CrossFade("Idle", .1f);
        audioSource.Stop();
        onCharacterIdle?.Invoke();
    }

    void Walk(Vector3 dir)
    {
        Move(dir * walkSpeed);
        anims.CrossFade("Walk", .1f);
        audioSource.Stop();
    }

    void Run(Vector3 dir)
    {
        Move(dir * runSpeed);
        anims.CrossFade("Run", .1f);
        audioSource.clip = run;
        if (!audioSource.isPlaying) { audioSource.Play(); }
    }

    void Move(Vector3 amount)
    {
        isIdle = false;
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 moveDirection = (forward * -amount.x) + (right * amount.z);

        characterController.Move(moveDirection * Time.deltaTime);

        lookAtHelper.localPosition = new Vector3(amount.z, 0, -amount.x);
        anims.transform.LookAt(lookAtHelper);
        onMove?.Invoke();
    }

    void Update()
    {
        if (joystickActive) { return; }
        
        
        
        
        float characterX = Input.GetAxis("Vertical");
        float characterZ = Input.GetAxis("Horizontal");

        Vector3 dir = new Vector3(-characterX, 0, characterZ);

        if (characterX != 0 || characterZ != 0) 
        {
            if (Input.GetKey(KeyCode.LeftShift))  { Run(dir);} else { Walk(dir); }
        }
        else { Idle(); }
        
    }
}