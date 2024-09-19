using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Player_Control;


/// <summary>
/// ������Ч�ڲ���
/// </summary>
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;//������Ч
    public AudioClip silencerShootSound;//������Ч��������
    public AudioClip reloadSoundAmmotLeft;//���ӵ���Ч
    public AudioClip reloadSoundOutOfAmmo;//���ӵ���ǹ˨����һ����ϻ���꣩
    public AudioClip aimSound;//��׼��Ч

}
public class Weapon_AutomaticGun : Weapon
{
    public SoundClips soundClips;
    public Animator animator;
    private Player_Control playerController;
    private Camera mainCamera;
    public Camera gunCamera;

    public bool IS_AUTORIFLE;//�Ƿ���ȫ�Զ�ģʽ
    public bool IS_SEMIGUN;//�Ƿ��ǰ��Զ�ģʽ

    [Header("��������λ��")]
    [Tooltip("�����λ��")] public Transform ShootPoint;//���ߴ����λ��
    [Tooltip("�ӵ���Ч�����λ��")] public Transform BulletShootPoint;//�ӵ���Ч�����λ��
    [Tooltip("�����׳���λ��")] public Transform CasingBulletSpawnPoint;

    [Header("�ӵ�Ԥ����")]
    public Transform bulletPrefab;//�ӵ�
    public Transform casingPrefab;//�ӵ��׿�

    [Header("ǹе����")]
    [Tooltip("�������")] public float range;
    [Tooltip("��帳ֵ��������")] public float gunRate;
    [Tooltip("��������")] private float fireRate;
    [Tooltip("ԭʼ����")] public float originRate;
    [Tooltip("���ƫ��")] public float SpreadFactor;
    [Tooltip("��ʱ��")] private float fireTimer;
    [Tooltip("�ӵ��������")] private float bulletForce;

    [Tooltip("��ǰ����ÿ����ϻ�ӵ���")] public int bulletMag;
    [Tooltip("��ǰ�ӵ���")] private int currentBullet;
    [Tooltip("��������")] public int bulletLeft;
    private int shotgunFragment = 8;//һ�δ�����ӵ���������ǹ��
    public float minDamage;
    public float maxDamage;

    [Header("�������")]
    [Tooltip("������")] public bool isSilencer;//�Ƿ�װ������

    [Header("��Ч")]
    public Light muzzleflashLight;//����ƹ�
    private float lightDuration;//�ƹ����ʱ��
    public ParticleSystem muzzlePatic;//�ƹ�Ļ���������Ч1
    public ParticleSystem sparkPatic;// �ƹ�Ļ���������Ч2
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("��Դ")]
    private AudioSource mainAudioSource;
    private AudioSource shootAudioSource;

    [Header("UI")]
    public Image[] crossQuarterImgs;//׼��
    public float currentExpanedDegree;//��ǰ׼�ǿ��϶�
    private float crossExpanedDegree;//ÿ֡׼�ǿ��϶�
    private float maxCrossDegree;//��󿪺϶�
    public Text ammoTextUi;
    public Text shootModeTextUI;

    public Player_Control.MovementState state;
    private bool isReloading;//�ж��Ƿ���װ��
    private bool isAiming;//�Ƿ�����׼״̬
    private bool isFire;//�Ƿ��ڿ���״̬

    private Vector3 sniperingFiflePosition;//ǹеĬ�ϵĳ�ʼλ��
    public Vector3 sniperingFifleOnPosition;//��ʼ��׼��ģ��λ��


    [Header("��λ����")]
    [SerializeField][Tooltip("װ���ӵ�����")] private KeyCode reloadInputName = KeyCode.R;
    [SerializeField][Tooltip("�Զ�����Զ��л�����")] private KeyCode GunShootModelInputName = KeyCode.X;
    [SerializeField][Tooltip("������������")] private KeyCode inspectInputName = KeyCode.I;
    
    /*ʹ��ö������ȫ�Զ��Ͱ��Զ�ģʽ*/
    public ShootMode shootMode;
    private bool GunShootInput;//����ȫ�Զ��Ͱ��Զ� ����ļ�λ���뷢���ı�
    private int modeNum;//ģʽ�л���һ���м������1.ȫ�Զ�ģʽ��2.���Զ�ģʽ��
    private string shootModeName;

    [Header("�ѻ�������")]
    [Tooltip("�ѻ�������")]public Material scopeRenderMaterial;
    [Tooltip("��û�н�����׼ʱ��ѻ�������ɫ")] public Color fadeColor;
    [Tooltip("��������׼ʱ�ľѻ�������ɫ")] public Color defaultColor;
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

        /*���ݲ�ͬǹе����Ϸ�տ�ʼ�ǽ��в�ͬ������ģʽ*/
        if (IS_AUTORIFLE)
        {
            modeNum = 1;
            shootModeName = "ȫ�Զ�";
            shootMode = ShootMode.AutoRifle;
            UpdateAmmoUI();
        }
        if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "���Զ�";
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
        //�Զ�ǹе������뷽ʽ ������GetMouseButton ��GetMouseButtonDown ���л�
        if(IS_AUTORIFLE || IS_SEMIGUN)
        {
            if(Input.GetKeyDown(GunShootModelInputName) && modeNum != 1)
            {
                modeNum = 1;
                shootModeName = "ȫ�Զ�";
                shootMode = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModelInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "���Զ�";
                shootMode = ShootMode.SemiGun;
                UpdateAmmoUI();
            }
            /*�������ģʽ��ת�� �����Ҫ�ô���ȥ��̬������*/
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
            //���Զ�ǹе������뷽ʽ��ΪGetMouseButtonDown
            GunShootInput = Input.GetMouseButtonDown(0);
        }

        state = playerController.state;//����ʵʱ��ȡ������ƶ�״̬�����ߣ����ܣ��¶ף�
        if (state == MovementState.walking && Vector3.SqrMagnitude(playerController.moveDirection) > 0 && state != MovementState.running && state != MovementState.crouching)
        {
            //�ƶ�ʱ׼�ǵĿ��϶�
            ExpandingCrossUpdate(crossExpanedDegree);
        }
        else if (state != MovementState.walking && state == MovementState.running && state != MovementState.crouching)
        {
            //����ʱ׼�ǿ��϶�
            ExpandingCrossUpdate(crossExpanedDegree * 2);
        }
        else
        {
            //վ�������¶ײ�����׼�ǿ��϶�
            ExpandingCrossUpdate(0);
        }

        //�������ߡ��ܲ�����
        //if(playerController.isWalk && playerController.isRun && !playerController.isCrouching)  
        animator.SetBool("Run", playerController.isRun);
        
        animator.SetBool("Walk", playerController.isWalk);

        //�ж��ǲ���װ����
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
            //��ǰ����ǹ�ӵ�װ����ϣ���ʱ������������
            animator.Play("reload_close");
            isReloading = false;
        }


            if (Input.GetKeyDown(reloadInputName) && currentBullet < bulletMag && bulletLeft > 0 && !isReloading)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("aim_in")) return;
            DoReloadAnimation();

        }
        
        //��ʱ��
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (GunShootInput && currentBullet > 0)
        {
            //����ǹ���һ��ͬʱ���8��������ǹе����һ������
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

        ////�ж��Ƿ�������У����ƿ���ʱ���ܽ���������������׼
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

        //��������˳���׼
        if (Input.GetMouseButton(1) && !isReloading && !playerController.isRun )
        {
            isAiming = true;
            animator.SetBool("Aim", isAiming);
            //��׼��ʱ����Ҫ΢��һЩǹеģ��λ��
            transform.localPosition = sniperingFifleOnPosition;
        }
        else 
        {
            isAiming = false;
            animator.SetBool("Aim", isAiming);
            transform.localPosition = sniperingFiflePosition;
        }

        //�������׼״̬ʱ��׼�ȵĲ�ͬ
        SpreadFactor = (isAiming) ? 0.001f : 0.1f; 


        //����ǹе
        if (Input.GetKeyDown(inspectInputName))
        {
            animator.SetTrigger("Inspect");
        }
        
    }

    /// <summary>
    /// ���
    /// </summary>
    public override void GunFire()
    {
        /*
         * 1����������
         * 2����ǰ��ϻΪ��
         * �����Է����ӵ�
         */
        if (fireTimer < fireRate || currentBullet <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("take_out_weapon") || isReloading || animator.GetCurrentAnimatorStateInfo(0).IsName("inspect_weapon") ) return;

        StartCoroutine(MuzzleFlashLight());//����ƹ�
        muzzlePatic.Emit(1);//�ýű����Ʒ���1��ǹ�ڻ�������
        sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission ));//����ǹ�ڻ�������
        StartCoroutine(Shoot_Crss());
        

        if(!isAiming)
        {
            //������ͨ���𶯻���ʹ�ö����ĵ��뵭��Ч����
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }
        else
        {
            //��׼״̬�£�������׼���𶯻�
            animator.Play("aim_fire", 0, 0);
        }

        for (int i = 0; i < shotgunFragment; i++)
        {
            RaycastHit hit;
            Vector3 shootDirection = ShootPoint.forward;//��ǰ�����
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
                    //����ǹ���⴦���� ���ӵ�����λ���趨�� hit.point
                    bullet = Instantiate(bulletPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
                
                //���ӵ���βһ����ǰ���ٶ������������ߴ��ȥ��ƫ��ֵ��
                bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;
                //���е���ʱ�ж�
                if (hit.transform.gameObject.transform.tag == "Enemy")
                {
                    hit.transform.gameObject.GetComponent<Enemy>().Health(Random.Range(minDamage, maxDamage));
                }
                Debug.Log(hit.transform.gameObject.name + ("����"));

            }
        }
        
        //ʵ���׿�
        Instantiate(casingPrefab, CasingBulletSpawnPoint.transform.position, CasingBulletSpawnPoint.transform.rotation);
        //�����Ƿ�װ�����������л���ͬ��Ч
        shootAudioSource.clip = isSilencer ? soundClips.silencerShootSound : soundClips.shootSound;
        shootAudioSource.Play();

        fireTimer = 0f;//���ü�ʱ��
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
    /// ������׼������׼�ǣ��������Ұ���
    /// </summary>
    public override void AimIn()
    {
        
        float currentVelocity = 0f;
        for (int i = 0; i < crossQuarterImgs.Length; i++)
        {
            crossQuarterImgs[i].gameObject.SetActive(false);
        }

        //�ѻ�ǹ��׼��ʱ�򣬸ı� gunCamera ����Ұ����׼������ɫ
        if(IS_SEMIGUN && (gameObject.name =="4"))
        {
            scopeRenderMaterial.color = defaultColor;
            gunCamera.fieldOfView = 15;
        }

        //��׼��ʱ���������Ұ���
        mainCamera.fieldOfView = Mathf.SmoothDamp(45, 60, ref currentVelocity, 0.5f);
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();

    }

    /// <summary>
    /// animator event
    /// �˳���׼����ʾ׼�ǣ��������Ұ�ָ�
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

        //��׼��ʱ���������Ұ���
        mainCamera.fieldOfView = Mathf.SmoothDamp(60, 45, ref currentVelocity, 0.5f);
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();
    }

    /// <summary>
    /// ���Ų�ͬ��װ������
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
                isReloading = true;//������ֳ��������ʱ�򻻵�����û�л���������������ǰ��ֵΪtrue����update�м���Ƿ��ڻ����ٸ�ֵΪfalse
            }
        }
        else
        {
            if(currentBullet ==bulletMag) return;
            //����ǹ���ӵ���������
            animator.SetTrigger("shotgun_reload");
        }
        
        
    }

    /// <summary>
    /// װ�ҩ�߼����ڶ����������
    /// </summary>
    public override void Reload()
    {
        //if (bulletLeft <= 0) return;
        //������Ҫ�����ӵ�
        int bulletToLoad = bulletMag - currentBullet;
        //���㱸���۳����ӵ���
        int bulletToReduce = bulletLeft >= bulletToLoad ? bulletToLoad : bulletLeft;
        bulletLeft -= bulletToReduce;//��������
        currentBullet += bulletToReduce;//��ϻ�ӵ�����
        UpdateAmmoUI();
    }

    /// <summary>
    /// ����ǹ�����߼�
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
    /// ����ָ����С�������ӻ��С׼�ǿ��϶�
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
    /// �ı�׼�ǵĿ��϶ȣ����Ҽ�¼��ǰ׼�ǿ��϶�
    /// </summary>
    public void ExpandCross(float add)
    {

        crossQuarterImgs[0].transform.localPosition += new Vector3(-add, 0, 0);//��׼��
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);//��׼��
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0);//��׼��
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);//��׼��
        crossQuarterImgs[4].transform.localPosition += new Vector3(0, 0, 0);//�м�׼��
        currentExpanedDegree += add;//���浱ǰ׼�ǿ����
        currentExpanedDegree = Mathf.Clamp(currentExpanedDegree, 0, maxCrossDegree); //����׼�ǿ��϶ȴ�С

    }

    /// <summary>
    /// б�ˣ�����׼�ǿ��϶ȣ�1ִ֡����5��
    /// ֻ�������ʱ��˲������׼��
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
