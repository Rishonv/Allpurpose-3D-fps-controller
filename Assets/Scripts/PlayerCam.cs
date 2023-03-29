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

 private void Start(){
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
 }

 private void Update(){
    yRotation += mouseX;
    xRotation -= mouseY;
    xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    orientation.rotation = Quaternion.Euler(0, yRotation, 0);
 } 

 private void LateUpdate(){
    mouseX = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * sensX;
    mouseY = Input.GetAxisRaw("Mouse Y") * Time.fixedDeltaTime * sensY;
 }
}
