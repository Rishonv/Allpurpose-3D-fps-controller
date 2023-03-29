using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header ("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header ("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header ("Inputs")]
    public KeyCode slideKey = KeyCode.LeftAlt;
    private float horizontalInput;
    private float veritcalInput;

    //private bool isSliding;

    private void Start(){
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        startYScale = playerObj.localScale.y;
    }

    private void Update(){
        horizontalInput = Input.GetAxisRaw("Horizontal");
        veritcalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput !=0 || veritcalInput != 0)){
            StartSlide();
        }

        if (Input.GetKeyUp(slideKey) && pm.isSliding) StopSlide();
    }

    private void FixedUpdate(){
        if (pm.isSliding) SlidingMovement();
    }

    private void StartSlide(){
        pm.isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void StopSlide(){
        pm.isSliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    private void SlidingMovement(){
        Vector3 inputDirection = orientation.forward * veritcalInput + orientation.right * horizontalInput;
        //ground sliding
        if (!pm.OnSlope() || rb.velocity.y > -0.1f){
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        //slope sliding (no slide countdown)
        else {rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
        if (slideTimer <=0) StopSlide();
    }
}
