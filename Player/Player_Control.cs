using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Player_Control : MonoBehaviour
{
    private CharacterController characterController;

    public Vector3 moveDirection; // 设置移动方向

    private AudioSource audioSource;

    [Header("玩家数值")]
    [Tooltip("行走速度")] public float walkSpeed; // 移动速度
    [Tooltip("奔跑速度")] public float runSpeed;
    [Tooltip("下蹲行走速度")] public float crouchSpeed;
    [Tooltip("当前移动速度")] public float speed; // 当前移动速度
    [Tooltip("玩家生命值")] public float playerHealth;//玩家生命值

    [Tooltip("跳跃力")] public float JumpForce = 2f; // 跳跃力度
    [Tooltip("重力")] public float gravity = 30f;

    [Tooltip("玩家下蹲时的玩家高度")] public float crouchHeight;
    [Tooltip("玩家站立时的玩家高度")] public float standHeight;

    [Header("键位设置")]
    [Tooltip("奔跑按键")] private KeyCode runInputName = KeyCode.LeftShift; // 奔跑按键
    [Tooltip("跳跃按键")] public KeyCode jumpInputName = KeyCode.Space; // 跳跃按键
    [Tooltip("下蹲按键")] private KeyCode crouchInputName = KeyCode.LeftControl; //下蹲按键
    [Tooltip("丢弃按键")] private KeyCode discarweapondName = KeyCode.G; //丢弃按键

    [Header("玩家属性判断")]
    public MovementState state;
    private CollisionFlags collisionFlags;
    public bool isWalk; //判断是否在行走
    public bool isRun; // 判断是否是在奔跑
    public bool hasJumped; // 判断是否是在跳跃
    public bool isGround; //判断是否在地面
    public bool isCanCrouch; //判断玩家是否可以下蹲
    public bool isCrouching; //判断玩家是否在下蹲
    public bool playerDeath;//判断玩家是否死亡
    private bool isDamage;//判断玩家是否受到伤害

    public LayerMask crouchLayerMask;

    public Text playerHealthUI;
    public Text ProptboxUI;

    public Image hurtImage; //玩家血雾 
    private Color flashColor = Color.red;
    private Color clearColor = Color.clear;

    [Header("音效")]
    [Tooltip("行走音效")] public AudioClip walkSound;
    [Tooltip("奔跑音效")] public AudioClip runSound;

    private Inventory inventory;
    private void Start()
    {
        // 获取 player 身上的 CharacterController 组件
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
        playerHealthUI.text = "生命值：" + playerHealth;
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
                PromptUI("背包没有武器");
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
    /// 移动
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
        else if (isGround)//如果是正常行走，才是walkSpeed
        {
            state = MovementState.walking;
            speed = walkSpeed;
            if (isCrouching)
            {
                state = MovementState.crouching;
                speed = crouchSpeed;
            }
        }


        moveDirection = (transform.right * h + transform.forward * v).normalized; // 设置玩家移动方向(normalized使斜向速度为1）
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    private void JumpAction()
    {
        if (!isCanCrouch) return;
        hasJumped = Input.GetKey(jumpInputName); //hasJumped(是否跳了一下)
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
            Vector3 jump = new Vector3(0, JumpForce * Time.deltaTime,0);//将跳跃力转换为V3坐标
            collisionFlags = characterController.Move(jump);//调用角色控制器移动方法，向上方法模拟跳跃
            //Debug.Log("collisionFlags:" + collisionFlags);
            //Debug.Log("characterController:" + characterController.isGrounded);
            /*判断玩家是否在地面上
             CollisionFlags:characterController 内置的碰撞位置标识号
             CollisionFlags.Below --> 在地面*/
            if (collisionFlags == CollisionFlags.Below)
            {
                isGround = true;
                JumpForce = -2f;
            }
            
        }
    }

    /// <summary>
    /// 判断人物是否可以下蹲
    /// </summary>
    public void CanCrouch()
    {
        //获取人物头顶的高度V3位置
        Vector3 sphereLocation = transform.position + new Vector3(0, 0.3f, 0) + Vector3.up * standHeight;
        //根据头顶上是否有物体，来判断是否可以下蹲
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
    /// 地面检测
    /// </summary>
    //public void CheckGround()
    //{
    //    hasJumped = !Physics.CheckSphere(groundCheck.position, groundDistance, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    //}

    /// <summary>
    /// 检查是否在地面上
    /// </summary>
    /// <returns></returns>
    //private bool IsGrounded()
    //{
    //    return Physics.CheckSphere(groundCheck.position, groundDistance, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    //}

    /// <summary>
    /// 下蹲
     /// <summary>
    public void Crouch(bool newCrouching)
    {
        if (!isCanCrouch) return; //如果下蹲了就不能再下蹲了
        isCrouching = newCrouching;
        characterController.height = isCrouching? crouchHeight : standHeight;
        characterController.center = characterController.height / 2.0f * Vector3.up;
    }

    /// <summary>
    /// 下蹲
    /// <summary>
    public void PlayerFootSoundSet()
    {
        if (isGround && moveDirection.sqrMagnitude > 0)//说明位移坐标转换为向量值，确保在移动
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
    /// 拾取武器
    /// </summary>
    public void PickUpWeapon(int itemID, GameObject weapon)
    {
        //捡到武器后在武器库里添加，否则补充备弹
        if (inventory.weapons.Contains(weapon))
        {
            weapon.GetComponent<Weapon_AutomaticGun>().bulletLeft = weapon.GetComponent<Weapon_AutomaticGun>().bulletMag * 5;
            weapon.GetComponent<Weapon_AutomaticGun>().UpdateAmmoUI();
            PromptUI("背包中已经有这把武器了，补充了备弹");
            Debug.Log("背包中已经有这把武器了，补充了备弹");
            return;
        }
        else
        {
            inventory.AddWeapon(weapon);
        }
    }

    /// <summary>
    /// 丢弃武器
    /// </summary>
    public void DiscardWeapon(GameObject weapon)
    {
        inventory.ThrowWeapon(weapon);
    }

    /// <summary>
    /// 玩家生命值
    /// </summary>
    /// <param name="damage">接受的伤害</param>
    public void PlayerHealth(float damage)//从敌人获取damage
    {
        playerHealth -= damage;
        isDamage = true;
        playerHealthUI.text = "生命值：" + playerHealth;
        if (playerHealth <= 0)
        {
            playerDeath = true;
            playerHealthUI.text = "玩家死亡";
            Time.timeScale = 0;//游戏暂停
        }
    }

    /// <summary>
    /// 消息提示
    /// </summary>
    public void PromptUI(string text)
    {
        ProptboxUI.text = text;
        ProptboxUI.gameObject.SetActive(true);
        //调用HideTextAfterDelay方法，让文本显示3秒后消失
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