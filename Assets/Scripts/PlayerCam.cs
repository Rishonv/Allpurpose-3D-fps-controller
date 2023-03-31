using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
 public float sensX;
 public float sensY;

 public Transform orientation;

 float xRotation;
 float yRotation;
 float mouseX;
 float mouseY;

// Hides and locks the cursor to the center of the screen
 private void Start(){
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
 }

// Rotates the camera and/or the player to stay with the camera.
 private void Update(){
    yRotation += mouseX;
    xRotation -= mouseY;
    // Makes sure you can only look up or down 90 degrees
    xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    orientation.rotation = Quaternion.Euler(0, yRotation, 0);
 } 
// Gets the input from the mouse
 private void LateUpdate(){
    mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensX;
    mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensY;
 }
}
