using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player_Control : MonoBehaviour
{
    private CharacterController characterController;

    public Vector3 moveDirection; // �����ƶ�����

    private AudioSource audioSource;

    [Header("�����ֵ")]
    [Tooltip("�����ٶ�")] public float walkSpeed; // �ƶ��ٶ�
    [Tooltip("�����ٶ�")] public float runSpeed;
    [Tooltip("�¶������ٶ�")] public float crouchSpeed;
    [Tooltip("��ǰ�ƶ��ٶ�")] public float speed; // ��ǰ�ƶ��ٶ�
    [Tooltip("�������ֵ")] public float playerHealth;//�������ֵ

    [Tooltip("��Ծ��")] public float JumpForce = 2f; // ��Ծ����
    [Tooltip("����")] public float gravity = 30f;

    [Tooltip("����¶�ʱ����Ҹ߶�")] public float crouchHeight;
    [Tooltip("���վ��ʱ����Ҹ߶�")] public float standHeight;

    [Header("��λ����")]
    [Tooltip("���ܰ���")] private KeyCode runInputName = KeyCode.LeftShift; // ���ܰ���
    [Tooltip("��Ծ����")] public KeyCode jumpInputName = KeyCode.Space; // ��Ծ����
    [Tooltip("�¶װ���")] private KeyCode crouchInputName = KeyCode.LeftControl; //�¶װ���
    [Tooltip("��������")] private KeyCode discarweapondName = KeyCode.G; //��������

    [Header("��������ж�")]
    public MovementState state;
    private CollisionFlags collisionFlags;
    public bool isWalk; //�ж��Ƿ�������
    public bool isRun; // �ж��Ƿ����ڱ���
    public bool hasJumped; // �ж��Ƿ�������Ծ
    public bool isGround; //�ж��Ƿ��ڵ���
    public bool isCanCrouch; //�ж�����Ƿ�����¶�
    public bool isCrouching; //�ж�����Ƿ����¶�
    public bool playerDeath;//�ж�����Ƿ�����
    private bool isDamage;//�ж�����Ƿ��ܵ��˺�

    public LayerMask crouchLayerMask;

    public Text playerHealthUI;
    public Text ProptboxUI;

    public Image hurtImage; //���Ѫ�� 
    private Color flashColor = Color.red;
    private Color clearColor = Color.clear;

    [Header("��Ч")]
    [Tooltip("������Ч")] public AudioClip walkSound;
    [Tooltip("������Ч")] public AudioClip runSound;

    private Inventory inventory;
    private void Start()
    {
        // ��ȡ player ���ϵ� CharacterController ���
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        inventory = GetComponentInChildren<Inventory>();
        hurtImage.gameObject.SetActive(true);
        playerHealth = 100f;
        walkSpeed = 5f;
        runSpeed = 10f;
        crouchSpeed = 2f;
        JumpForce = 0f;
        gravity = 30f;
        crouchHeight = 1f;
        standHeight = characterController.height;
        playerHealthUI.text = "����ֵ��" + playerHealth;
        //groundCheck = GameObject.Find("Player/CheckGround").GetComponent<Transform>();
    }

    private void Update()
    {
        MoveCharacter();
        if (Input.GetKeyDown(discarweapondName))
        {
            if (inventory.weapons != null)
            {
                DiscardWeapon(inventory.weapons[inventory.currentWeaponID].gameObject);
            }
            else
            {
                PromptUI("����û������");
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDamage)
        {
            hurtImage.color = flashColor;
            isDamage = false;
        }
        else
        {
            hurtImage.color = Color.Lerp(hurtImage.color, clearColor, Time.deltaTime * 5);
        }
        CanCrouch();
        if (Input.GetKey(crouchInputName))
        {
            Crouch(true);
        } else
        {
            Crouch(false);
        }
        //CheckGround();
        JumpAction();
        PlayerFootSoundSet();
    }

    /// <summary>
    /// �ƶ�
    /// </summary>
    public void MoveCharacter()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        isRun = Input.GetKey(runInputName);
        isWalk = (Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0) ? true : false;
        if (isRun && isGround && !isCrouching && isWalk)
        {
            state = MovementState.running;
            speed = runSpeed;
        }
        else if (isGround)//������������ߣ�����walkSpeed
        {
            state = MovementState.walking;
            speed = walkSpeed;
            if (isCrouching)
            {
                state = MovementState.crouching;
                speed = crouchSpeed;
            }
        }


        moveDirection = (transform.right * h + transform.forward * v).normalized; // ��������ƶ�����(normalizedʹб���ٶ�Ϊ1��
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// ��Ծ
    /// </summary>
    private void JumpAction()
    {
        if (!isCanCrouch) return;
        hasJumped = Input.GetKey(jumpInputName); //hasJumped(�Ƿ�����һ��)
        if (hasJumped && isGround)
        {
            isGround = false;
            JumpForce = 10f;
        }
        else if (!hasJumped && isGround)
        {
            isGround = false;
        }

        if (!isGround)
        {
            JumpForce = JumpForce - gravity * Time.deltaTime;
            Vector3 jump = new Vector3(0, JumpForce * Time.deltaTime,0);//����Ծ��ת��ΪV3����
            collisionFlags = characterController.Move(jump);//���ý�ɫ�������ƶ����������Ϸ���ģ����Ծ
            //Debug.Log("collisionFlags:" + collisionFlags);
            //Debug.Log("characterController:" + characterController.isGrounded);
            /*�ж�����Ƿ��ڵ�����
             CollisionFlags:characterController ���õ���ײλ�ñ�ʶ��
             CollisionFlags.Below --> �ڵ���*/
            if (collisionFlags == CollisionFlags.Below)
            {
                isGround = true;
                JumpForce = -2f;
            }
            
        }
    }

    /// <summary>
    /// �ж������Ƿ�����¶�
    /// </summary>
    public void CanCrouch()
    {
        //��ȡ����ͷ���ĸ߶�V3λ��
        Vector3 sphereLocation = transform.position + new Vector3(0, 0.3f, 0) + Vector3.up * standHeight;
        //����ͷ�����Ƿ������壬���ж��Ƿ�����¶�
        isCanCrouch = (Physics.OverlapSphere(sphereLocation, characterController.radius, crouchLayerMask ).Length )== 0;
        Collider[] colis = Physics.OverlapSphere(sphereLocation, characterController.radius, crouchLayerMask);
        //for (int i = 0; i < colis.Length; i++)
        //{
        //    Debug.Log("colis:" + colis[i].name);

        //}
        //Debug.Log("sphereLocation:" + sphereLocation);
        //Debug.Log("isCanCrouch:" + isCanCrouch);

    }
    /// <summary>
    /// ������
    /// </summary>
    //public void CheckGround()
    //{
    //    hasJumped = !Physics.CheckSphere(groundCheck.position, groundDistance, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    //}

    /// <summary>
    /// ����Ƿ��ڵ�����
    /// </summary>
    /// <returns></returns>
    //private bool IsGrounded()
    //{
    //    return Physics.CheckSphere(groundCheck.position, groundDistance, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    //}

    /// <summary>
    /// �¶�
     /// <summary>
    public void Crouch(bool newCrouching)
    {
        if (!isCanCrouch) return; //����¶��˾Ͳ������¶���
        isCrouching = newCrouching;
        characterController.height = isCrouching? crouchHeight : standHeight;
        characterController.center = characterController.height / 2.0f * Vector3.up;
    }

    /// <summary>
    /// �¶�
    /// <summary>
    public void PlayerFootSoundSet()
    {
        if (isGround && moveDirection.sqrMagnitude > 0)//˵��λ������ת��Ϊ����ֵ��ȷ�����ƶ�
        {
            audioSource.clip = isRun? runSound : walkSound;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

        }else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        if (isCrouching)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    /// <summary>
    /// ʰȡ����
    /// </summary>
    public void PickUpWeapon(int itemID, GameObject weapon)
    {
        //��������������������ӣ����򲹳䱸��
        if (inventory.weapons.Contains(weapon))
        {
            weapon.GetComponent<Weapon_AutomaticGun>().bulletLeft = weapon.GetComponent<Weapon_AutomaticGun>().bulletMag * 5;
            weapon.GetComponent<Weapon_AutomaticGun>().UpdateAmmoUI();
            PromptUI("�������Ѿ�����������ˣ������˱���");
            Debug.Log("�������Ѿ�����������ˣ������˱���");
            return;
        }
        else
        {
            inventory.AddWeapon(weapon);
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void DiscardWeapon(GameObject weapon)
    {
        inventory.ThrowWeapon(weapon);
    }

    /// <summary>
    /// �������ֵ
    /// </summary>
    /// <param name="damage">���ܵ��˺�</param>
    public void PlayerHealth(float damage)//�ӵ��˻�ȡdamage
    {
        playerHealth -= damage;
        isDamage = true;
        playerHealthUI.text = "����ֵ��" + playerHealth;
        if (playerHealth <= 0)
        {
            playerDeath = true;
            playerHealthUI.text = "�������";
            Time.timeScale = 0;//��Ϸ��ͣ
        }
    }

    /// <summary>
    /// ��Ϣ��ʾ
    /// </summary>
    public void PromptUI(string text)
    {
        ProptboxUI.text = text;
        ProptboxUI.gameObject.SetActive(true);
        //����HideTextAfterDelay���������ı���ʾ3�����ʧ
        StartCoroutine(HideTextAfterDelay(3f));
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ProptboxUI.gameObject.SetActive(false);
    }

    public enum MovementState
    {
        walking,
        running, 
        crouching,
        idle
    }
}