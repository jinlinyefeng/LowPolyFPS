using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线
/// </summary>
public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    private AudioSource audioSource;
    public Detecting_players Detplayer;

    [Tooltip("敌人血量")] public float enemyHealth;
    [Tooltip("敌人血条")] public Slider slider;
    [Tooltip("敌人受到伤害文字UI")] public Text getDamageText;
    [Tooltip("敌人死亡特效")] public GameObject deadEffect;

    public GameObject[] wayPointObj;//存放敌人不同路线
    public List<Vector3> wayPoints = new List<Vector3>();//存放巡逻路线的每个巡逻点
    public int index;//下标值
    [Tooltip("敌人下标(用来分配随机路线)")] public int nameIndex;
    public int animState;//动画状态表示 0:idle 1:walk 2:attack
    public Transform targetPoint;

    public EnemyBaseState currentState;//敌人当前的状态
    public PatrolState patrolState;//定义敌人巡逻状态，声明对象
    public AttackState attackState; //定义敌人巡逻状态，声明对象

    Vector3 targetPosition;
    //敌人的攻击目标，场景中有玩家将会用列表存储
    public List<Transform> attackList = new List<Transform>();
    [Tooltip("攻击间隔，时间越长攻击频率越慢")]public float attackRate;
    private float nextAttack = 0;//下次攻击时间
    [Tooltip("攻击距离")] public float attackRange;

    public bool isDead;//判断是否死亡

    public GameObject attackParticle01;
    public Transform attackParticle01Position;
    public AudioClip attackSound;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();
        Detplayer = transform.gameObject.AddComponent<Detecting_players>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
        isDead = false;
        slider.minValue = 0;
        slider.maxValue = enemyHealth;
        slider.value = enemyHealth;
        index = 0;
        TransitionToState(patrolState);//游戏一开始的时候敌人进入巡逻状态

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            slider.gameObject.SetActive(false);
            return;
        }
        //这里是表示当前状态持续执行
        //敌人移动方法一直执行
        currentState.OnUpdate(this);
        animator.SetInteger("state", animState);

        // 更新攻击目标
        if (attackList.Count > 0)
        {
            targetPoint = attackList[0];
        }

    }

    /// <summary>
    /// 敌人向着导航点移动
    /// </summary>
    public void MoveToTarget()
    {
        if (attackList.Count == 0)
        {
            agent.speed = 1.5f;
            // 敌人没有攻击目标，继续巡逻
            targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed);
        }
        else
        {
            
            // 敌人扫描到攻击对象，向攻击对象走去
            targetPosition = attackList[0].position;

            // 检查敌人与目标的距离
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance < 2.5f) // 假设2.5f为安全距离
            {
                agent.speed = 0; // 停止移动
                if (Time.time > nextAttack)
                {
                    AttackAction(); // 执行攻击动作
                    nextAttack = Time.time + attackRate;
                }
            }

        }

        // NavMesh Surface需要绑定在静态地形上！
        agent.destination = targetPosition;
    }


    /// <summary>
    /// 加载路线
    /// </summary>
    /// <param name="go"></param>
    public void LoadPath(GameObject go)
    {
        //加载路线之前清空list
        wayPoints.Clear();
        //遍历路线预制体里所有导航点位置信息，并加载到list里
        foreach (Transform T in go.transform)
        {
            wayPoints.Add(T.position);
        }
    }

    /// <summary>
    /// 切换敌人状态
    /// </summary>
    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

    /// <summary>
    /// 敌人受到伤害扣除血量
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void Health(float damage)
    {
        if (isDead) return;
        getDamageText.text = Mathf.Round(damage).ToString();
        enemyHealth -= damage;
        slider.value = enemyHealth;
        if (enemyHealth < 0)
        {
            isDead = true;
            animator.SetTrigger("dying");
            Destroy(Instantiate(deadEffect, transform.position, Quaternion.identity),3f);//敌人死亡爆炸特效持续三秒
            
        }
    }

    /// <summary>
    /// 敌人攻击玩家
    /// 普通攻击
    /// </summary>
    public void AttackAction()
    {
        //当敌人和攻击目标距离小于攻击距离，触发攻击动画
        if (Vector3.Distance(transform.position, targetPoint.position) < attackRange)
        {
            if (Time.time > nextAttack)
            {
                //触发攻击
                animator.SetTrigger("attack");
                agent.speed = 0;
                //更新下次攻击时间
                nextAttack = Time.time + attackRate;
            }
        }
    }

    /// <summary>
    /// 用来接受Detecting_player中的attackList列表
    /// </summary>
    /// <param name="newList"></param>
    public void UpdateAttackList(List<Transform> newList)
    {
        attackList = newList;
    }

    /// <summary>
    /// Animation Event
    /// </summary>
    public void PlayAttackSound()
    {
        audioSource.clip = attackSound;
        audioSource.Play();
    }

    public void Enemydispear()
    {
        Destroy(gameObject);
    }
    
    public void idleSpeed()
    {
        agent.speed = 0;
    }
    public void walkSpeed()
    {
        agent.speed = 3;
    }
}
