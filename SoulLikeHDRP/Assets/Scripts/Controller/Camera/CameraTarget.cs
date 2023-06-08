using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Transform playerTransform;

    void Update()
    {
        Quaternion yRotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);

        transform.position = playerTransform.position;
        transform.rotation = yRotation;
    } 
}
