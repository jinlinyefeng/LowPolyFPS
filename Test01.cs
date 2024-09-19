using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using UnityEngine;


public class Test01 : MonoBehaviour
{
    public float MoveSpeed = 5; // �ƶ��ٶ�
    private Rigidbody rb;
    private const float JumpForce = 10f; // ��Ծ��
    private const float GroundRaycastDistance = 0.5f; // ���������
    private bool hasJumped;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        JumpAction();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        
    }

    private void JumpAction()
    {
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, GroundRaycastDistance);

        if (isGrounded)
        {
            hasJumped = false;
        }
        if (!hasJumped && Input.GetKeyDown(KeyCode.Space))
        {
            // ʩ��һ�����ϵ���
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            hasJumped = true;
        }
    }
    private void MoveCharacter()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        int speedMultiplier = 1;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedMultiplier = 2;
        }

        // ʹ�� Translate ����ƽ��
        transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * speedMultiplier * MoveSpeed);
    }
}
