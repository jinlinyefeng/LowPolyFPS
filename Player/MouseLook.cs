using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���������������ת
/// �������������ת
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Tooltip("��Ұ������")]public float mouseSensitivity = 300f;//����������
    private Transform playerBody;
    private float yRotation = 0f;

    private CharacterController characterController;
    [Tooltip("��ǰ�������ʼ�߶�")]public float height = 1.8f;
    private float interpolationSpeed = 12f; //�߶ȱ仯ƽ��ֵ
    
    void Start()
    {
        //���ع��
        Cursor.lockState = CursorLockMode.Locked;
        playerBody = transform.GetComponentInParent<Player_Control>().transform;
        characterController = GetComponentInParent<CharacterController>();

    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -90f, 80f);//������ת�Ƕ�

        transform.localRotation = Quaternion.Euler(yRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);//��Һ�����ת

        //�������¶׻���վ��ʱ�����Ÿ߶ȱ仯��������ĸ߶�ҲҪ�����仯
        float heightTarget = characterController.height * 0.9f;
        height = Mathf.Lerp(height, heightTarget, interpolationSpeed * Time.deltaTime);
        transform.localPosition = Vector3.up * height ;
    }
}
