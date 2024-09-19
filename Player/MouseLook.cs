using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制摄像机上下旋转
/// 控制玩家左右旋转
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Tooltip("视野灵敏度")]public float mouseSensitivity = 300f;//视线灵敏度
    private Transform playerBody;
    private float yRotation = 0f;

    private CharacterController characterController;
    [Tooltip("当前摄像机初始高度")]public float height = 1.8f;
    private float interpolationSpeed = 12f; //高度变化平滑值
    
    void Start()
    {
        //隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        playerBody = transform.GetComponentInParent<Player_Control>().transform;
        characterController = GetComponentInParent<CharacterController>();

    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 80f);//限制旋转角度

        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);//玩家横向旋转

        //当人物下蹲或者站立时，随着高度变化，摄像机的高度也要发生变化
        float heightTarget = characterController.height * 0.9f;
        height = Mathf.Lerp(height, heightTarget, interpolationSpeed * Time.deltaTime);
        transform.localPosition = Vector3.up * height ;
    }
}
