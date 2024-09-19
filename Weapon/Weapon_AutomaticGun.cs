using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Player_Control;


/// <summary>
/// 武器音效内部类
/// </summary>
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;//开火音效
    public AudioClip silencerShootSound;//开火音效带消音器
    public AudioClip reloadSoundAmmotLeft;//换子弹音效
    public AudioClip reloadSoundOutOfAmmo;//换子弹拉枪栓声（一个弹匣打完）
    public AudioClip aimSound;//瞄准音效

}
public class Weapon_AutomaticGun : Weapon
{
    public SoundClips soundClips;
    public Animator animator;
    private Player_Control playerController;
    private Camera mainCamera;
    public Camera gunCamera;

    public bool IS_AUTORIFLE;//是否是全自动模式
    public bool IS_SEMIGUN;//是否是半自动模式

    [Header("武器部件位置")]
    [Tooltip("射击的位置")] public Transform ShootPoint;//射线打出的位置
    [Tooltip("子弹特效打出的位置")] public Transform BulletShootPoint;//子弹特效打出的位置
    [Tooltip("弹壳抛出的位置")] public Transform CasingBulletSpawnPoint;

    [Header("子弹预制体")]
    public Transform bulletPrefab;//子弹
    public Transform casingPrefab;//子弹抛壳

    [Header("枪械属性")]
    [Tooltip("武器射程")] public float range;
    [Tooltip("面板赋值武器射速")] public float gunRate;
    [Tooltip("武器射速")] private float fireRate;
    [Tooltip("原始射速")] public float originRate;
    [Tooltip("射击偏倚")] public float SpreadFactor;
    [Tooltip("计时器")] private float fireTimer;
    [Tooltip("子弹发射的力")] private float bulletForce;

    [Tooltip("当前武器每个弹匣子弹数")] public int bulletMag;
    [Tooltip("当前子弹数")] private int currentBullet;
    [Tooltip("武器备弹")] public int bulletLeft;
    private int shotgunFragment = 8;//一次打出的子弹数（霰弹枪）
    public float minDamage;
    public float maxDamage;

    [Header("武器配件")]
    [Tooltip("消音器")] public bool isSilencer;//是否装消音器

    [Header("特效")]
    public Light muzzleflashLight;//开火灯光
    private float lightDuration;//灯光持续时间
    public ParticleSystem muzzlePatic;//灯光的火焰粒子特效1
    public ParticleSystem sparkPatic;// 灯光的火焰粒子特效2
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("音源")]
    private AudioSource mainAudioSource;
    private AudioSource shootAudioSource;

    [Header("UI")]
    public Image[] crossQuarterImgs;//准星
    public float currentExpanedDegree;//当前准星开合度
    private float crossExpanedDegree;//每帧准星开合度
    private float maxCrossDegree;//最大开合度
    public Text ammoTextUi;
    public Text shootModeTextUI;

    public Player_Control.MovementState state;
    private bool isReloading;//判断是否在装弹
    private bool isAiming;//是否在瞄准状态
    private bool isFire;//是否在开火状态

    private Vector3 sniperingFiflePosition;//枪械默认的初始位置
    public Vector3 sniperingFifleOnPosition;//开始瞄准的模型位置


    [Header("键位设置")]
    [SerializeField][Tooltip("装填子弹按键")] private KeyCode reloadInputName = KeyCode.R;
    [SerializeField][Tooltip("自动与半自动切换按键")] private KeyCode GunShootModelInputName = KeyCode.X;
    [SerializeField][Tooltip("检视武器按键")] private KeyCode inspectInputName = KeyCode.I;
    
    /*使用枚举区分全自动和半自动模式*/
    public ShootMode shootMode;
    private bool GunShootInput;//根据全自动和半自动 射击的键位输入发生改变
    private int modeNum;//模式切换的一个中间参数（1.全自动模式，2.半自动模式）
    private string shootModeName;

    [Header("狙击镜设置")]
    [Tooltip("狙击镜材质")]public Material scopeRenderMaterial;
    [Tooltip("当没有进行瞄准时候狙击镜的颜色")] public Color fadeColor;
    [Tooltip("当进行瞄准时的狙击镜的颜色")] public Color defaultColor;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<Player_Control>();
        shootAudioSource = GetComponent<AudioSource>();
        mainAudioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
    }
    private void Start()
    {
        sniperingFiflePosition = transform.localPosition;
        muzzleflashLight.enabled = false;
        SpreadFactor = 0.1f;
        maxCrossDegree = 300f;
        crossExpanedDegree = 50f;
        lightDuration = 0.02f;
        range = 300f;
        bulletForce = 300f;
        bulletLeft = bulletMag * 5;
        currentBullet = bulletMag;
        UpdateAmmoUI();

        /*根据不同枪械，游戏刚开始是进行不同射击射击模式*/
        if (IS_AUTORIFLE)
        {
            modeNum = 1;
            shootModeName = "全自动";
            shootMode = ShootMode.AutoRifle;
            UpdateAmmoUI();
        }
        if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "半自动";
            shootMode = ShootMode.SemiGun;
            UpdateAmmoUI();
        }
    }

    private void Update()
    {
        if (playerController.playerDeath)
        {
            mainAudioSource.Pause();
            shootAudioSource.Pause();
        }
        //自动枪械鼠标输入方式 可以在GetMouseButton 和GetMouseButtonDown 里切换
        if(IS_AUTORIFLE || IS_SEMIGUN)
        {
            if(Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
            {
                modeNum = 1;
                shootModeName = "全自动";
                shootMode = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "半自动";
                shootMode = ShootMode.SemiGun;
                UpdateAmmoUI();
            }
            /*控制射击模式的转换 后面就要用代码去动态控制了*/
            switch(shootMode)
            {
                case ShootMode.AutoRifle:
                    GunShootInput = Input.GetMouseButton(0);
                    fireRate = originRate;
                    break;
                case ShootMode.SemiGun:
                    GunShootInput = Input.GetMouseButtonDown(0);
                    fireRate = gunRate;
                    break;
            }
        }
        else
        {
            //半自动枪械鼠标输入方式改为GetMouseButtonDown
            GunShootInput = Input.GetMouseButtonDown(0);
        }

        state = playerController.state;//这里实时获取人物的移动状态（行走，奔跑，下蹲）
        if (state == MovementState.walking && Vector3.SqrMagnitude(playerController.moveDirection) > 0 && state != MovementState.running && state != MovementState.crouching)
        {
            //移动时准星的开合度
            ExpandingCrossUpdate(crossExpanedDegree);
        }
        else if (state != MovementState.walking && state == MovementState.running && state != MovementState.crouching)
        {
            //奔跑时准星开合度
            ExpandingCrossUpdate(crossExpanedDegree * 2);
        }
        else
        {
            //站立或者下蹲不调整准星开合度
            ExpandingCrossUpdate(0);
        }

        //播放行走、跑步动画
        //if(playerController.isWalk && playerController.isRun && !playerController.isCrouching)  
        animator.SetBool("Run", playerController.isRun);
        
        animator.SetBool("Walk", playerController.isWalk);

        //判断是不是装弹中
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (
            info.IsName("reload_ammo_left") ||
            info.IsName("reload_out_of_ammo") ||
            info.IsName("reload_open") ||
            info.IsName("reload_close") ||
            info.IsName("reload_insert 1") ||
            info.IsName("reload_insert 2") ||
            info.IsName("reload_insert 3") ||
            info.IsName("reload_insert 4") ||
            info.IsName("reload_insert 5") ||
            info.IsName("reload_insert 6") 
            )
        {
            isReloading = true;
        }
        else
        {
            isReloading = false;
        }

        if (
            (info.IsName("reload_insert 1") ||
            info.IsName("reload_insert 2") ||
            info.IsName("reload_insert 3") ||
            info.IsName("reload_insert 4") ||
            info.IsName("reload_insert 5") ||
            info.IsName("reload_insert 6")) &&
            currentBullet == bulletMag
            )
        {
            //当前霰弹枪子弹装填完毕，随时结束换弹播放
            animator.Play("reload_close");
            isReloading = false;
        }


            if (Input.GetKeyDown(reloadInputName) && currentBullet < bulletMag && bulletLeft > 0 && !isReloading)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("aim_in")) return;
            DoReloadAnimation();

        }
        
        //计时器
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (GunShootInput && currentBullet > 0)
        {
            //霰弹枪射击一次同时打出8发，其他枪械正常一次射线
            if (IS_SEMIGUN && gameObject.name == "3")
            {
                shotgunFragment = 8;
            }
            else
            {
                shotgunFragment = 1;
            }
            GunFire();
            
        }

        ////判断是否在射击中，限制开火时不能进行其他动作如瞄准
        //if (Input.GetMouseButton(0))
        //{
        //    isFire = true;
        //    animator.SetBool("isFire", isFire);
        //}
        //else
        //{
        //    isFire = false;
        //    animator.SetBool("isFire", isFire);
        //}

        //鼠标进入和退出瞄准
        if (Input.GetMouseButton(1) && !isReloading && !playerController.isRun )
        {
            isAiming = true;
            animator.SetBool("Aim", isAiming);
            //瞄准的时候需要微调一些枪械模型位置
            transform.localPosition = sniperingFifleOnPosition;
        }
        else 
        {
            isAiming = false;
            animator.SetBool("Aim", isAiming);
            transform.localPosition = sniperingFiflePosition;
        }

        //腰射和瞄准状态时精准度的不同
        SpreadFactor = (isAiming) ? 0.001f : 0.1f; 


        //检视枪械
        if (Input.GetKeyDown(inspectInputName))
        {
            animator.SetTrigger("Inspect");
        }
        
    }

    /// <summary>
    /// 射击
    /// </summary>
    public override void GunFire()
    {
        /*
         * 1、控制射速
         * 2、当前弹匣为空
         * 不可以发射子弹
         */
        if (fireTimer < fireRate || currentBullet <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("take_out_weapon") || isReloading || animator.GetCurrentAnimatorStateInfo(0).IsName("inspect_weapon") ) return;

        StartCoroutine(MuzzleFlashLight());//开火灯光
        muzzlePatic.Emit(1);//用脚本控制发射1个枪口火焰粒子
        sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission ));//发射枪口火星粒子
        StartCoroutine(Shoot_Crss());
        

        if(!isAiming)
        {
            //播放普通开火动画（使用动画的淡入淡出效果）
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {
            //瞄准状态下，播放瞄准开火动画
            animator.Play("aim_fire", 0, 0);
        }

        for (int i = 0; i < shotgunFragment; i++)
        {
            RaycastHit hit;
            Vector3 shootDirection = ShootPoint.forward;//向前方射击
            if (playerController.isRun)
            {
                SpreadFactor = SpreadFactor * 2;
            }

            shootDirection = shootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
            
            if (Physics.Raycast(ShootPoint.position, shootDirection, out hit, range))
            {
                Transform bullet;
                if (IS_AUTORIFLE || (IS_SEMIGUN && gameObject.name == "2"))
                {
                    bullet = (Transform)Instantiate(bulletPrefab, BulletShootPoint.transform.position, BulletShootPoint.transform.rotation);
                }
                else
                {
                    //霰弹枪特殊处理下 将子弹限制位置设定到 hit.point
                    bullet = Instantiate(bulletPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                
                //给子弹拖尾一个向前的速度力（加上射线打出去的偏移值）
                bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;
                //击中敌人时判断
                if (hit.transform.gameObject.transform.tag == "Enemy")
                {
                    hit.transform.gameObject.GetComponent<Enemy>().Health(Random.Range(minDamage, maxDamage));
                }
                Debug.Log(hit.transform.gameObject.name + ("打到了"));

            }
        }
        
        //实例抛壳
        Instantiate(casingPrefab, CasingBulletSpawnPoint.transform.position, CasingBulletSpawnPoint.transform.rotation);
        //根据是否装备消音器，切换不同音效
        shootAudioSource.clip = isSilencer ? soundClips.silencerShootSound : soundClips.shootSound;
        shootAudioSource.Play();

        fireTimer = 0f;//重置计时器
        currentBullet--;
        UpdateAmmoUI();

    }

    public IEnumerator MuzzleFlashLight()
    {
        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleflashLight.enabled = false;
    }

    /// <summary>
    /// animator event
    /// 进入瞄准，隐藏准星，摄像机视野变近
    /// </summary>
    public override void AimIn()
    {
        
        float currentVelocity = 0f;
        for (int i = 0; i < crossQuarterImgs.Length; i++)
        {
            crossQuarterImgs[i].gameObject.SetActive(false);
        }

        //狙击枪瞄准的时候，改变 gunCamera 的视野和瞄准镜的颜色
        if(IS_SEMIGUN && (gameObject.name =="4"))
        {
            scopeRenderMaterial.color = defaultColor;
            gunCamera.fieldOfView = 15;
        }

        //瞄准的时候摄像机视野变近
        mainCamera.fieldOfView = Mathf.SmoothDamp(45, 60, ref currentVelocity, 0.5f);
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();

    }

    /// <summary>
    /// animator event
    /// 退出瞄准，显示准星，摄像机视野恢复
    /// </summary>
    public override void AimOut()
    {
        float currentVelocity = 0f;
        for (int i = 0; i < crossQuarterImgs.Length; i++)
        {
            crossQuarterImgs[i].gameObject.SetActive(true);
        }

        if (IS_SEMIGUN && (gameObject.name == "4"))
        {
            scopeRenderMaterial.color = fadeColor;
            gunCamera.fieldOfView = 35;
        }

        //瞄准的时候摄像机视野变近
        mainCamera.fieldOfView = Mathf.SmoothDamp(60, 45, ref currentVelocity, 0.5f);
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();
    }

    /// <summary>
    /// 播放不同的装弹动画
    /// </summary>
    public override void DoReloadAnimation()
    {
        if (isAiming) return;
        if (!(IS_SEMIGUN && (gameObject.name == "3" || gameObject.name == "4"))) 
        {
            
            if (currentBullet > 0 && bulletLeft > 0)
            {
                animator.Play("reload_ammo_left", 0, 0);

                mainAudioSource.clip = soundClips.reloadSoundAmmotLeft;
                mainAudioSource.Play();
                Reload();
                isReloading = true;

            }
            if (currentBullet == 0 && bulletLeft > 0)
            {
                animator.Play("reload_out_of_ammo", 0, 0);

                mainAudioSource.clip = soundClips.reloadSoundOutOfAmmo;
                mainAudioSource.Play();
                Reload();
                isReloading = true;//避免出现持续开火的时候换弹导致没有换弹动画，所以提前赋值为true，在update中检测是否在换弹再赋值为false
            }
        }
        else
        {
            if(currentBullet ==bulletMag) return;
            //霰弹枪换子弹动画触发
            animator.SetTrigger("shotgun_reload");
        }
        
        
    }

    /// <summary>
    /// 装填弹药逻辑，在动画里面调用
    /// </summary>
    public override void Reload()
    {
        //if (bulletLeft <= 0) return;
        //计算需要填充的子弹
        int bulletToLoad = bulletMag - currentBullet;
        //计算备弹扣除的子弹数
        int bulletToReduce = bulletLeft >= bulletToLoad ? bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce;//备弹减少
        currentBullet += bulletToReduce;//弹匣子弹增加
        UpdateAmmoUI();
    }

    /// <summary>
    /// 霰弹枪换弹逻辑
    /// ReloadAmmoState 
    /// </summary>
    public void ShotGunReload()
    {
        if (currentBullet < bulletMag)
        {
            currentBullet++;
            bulletLeft--;
            UpdateAmmoUI() ;
        }
        else
        {
            animator.Play("reload_close");
            return;
        }
        if (bulletLeft < 0) return;
    }

    /// <summary>
    /// 根据指定大小，来增加或减小准星开合度
    /// </summary>
    /// <param name="expanDegree"></param>
    public override void ExpandingCrossUpdate(float expanDegree)
    {
        if (currentExpanedDegree < expanDegree - 5)
        {
            ExpandCross(150 * Time.deltaTime);
        }
        else if(currentExpanedDegree > expanDegree + 5)
        {
            ExpandCross(-300 * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 改变准星的开合度，并且记录当前准星开合度
    /// </summary>
    public void ExpandCross(float add)
    {

        crossQuarterImgs[0].transform.localPosition += new Vector3(-add, 0, 0);//左准星
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);//右准星
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0);//上准星
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);//下准星
        crossQuarterImgs[4].transform.localPosition += new Vector3(0, 0, 0);//中间准星
        currentExpanedDegree += add;//保存当前准星开火度
        currentExpanedDegree = Mathf.Clamp(currentExpanedDegree, 0, maxCrossDegree); //限制准星开合度大小

    }

    /// <summary>
    /// 斜乘，调用准星开合度，1帧执行了5次
    /// 只负责射击时的瞬间增大准星
    /// </summary>
    /// <returns></returns>
    public IEnumerator Shoot_Crss()
    {
        yield return null;
        for (int i = 0; i < 5; i++)
        {
            ExpandCross(Time.deltaTime * 500);
        }
    }

    public void UpdateAmmoUI()
    {
        ammoTextUi.text = currentBullet + "/" + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    public enum ShootMode
    {
        AutoRifle,
        SemiGun
    };
}
