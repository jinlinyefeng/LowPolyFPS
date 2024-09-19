using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


/// <summary>
/// ������
/// ʵ��״̬�л������ص���Ѳ��·��
/// </summary>
public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
    private AudioSource audioSource;
    public Detecting_players Detplayer;

    [Tooltip("����Ѫ��")] public float enemyHealth;
    [Tooltip("����Ѫ��")] public Slider slider;
    [Tooltip("�����ܵ��˺�����UI")] public Text getDamageText;
    [Tooltip("����������Ч")] public GameObject deadEffect;

    public GameObject[] wayPointObj;//��ŵ��˲�ͬ·��
    public List<Vector3> wayPoints = new List<Vector3>();//���Ѳ��·�ߵ�ÿ��Ѳ�ߵ�
    public int index;//�±�ֵ
    [Tooltip("�����±�(�����������·��)")] public int nameIndex;
    public int animState;//����״̬��ʾ 0:idle 1:walk 2:attack
    public Transform targetPoint;

    public EnemyBaseState currentState;//���˵�ǰ��״̬
    public PatrolState patrolState;//�������Ѳ��״̬����������
    public AttackState attackState; //�������Ѳ��״̬����������

    Vector3 targetPosition;
    //���˵Ĺ���Ŀ�꣬����������ҽ������б�洢
    public List<Transform> attackList = new List<Transform>();
    [Tooltip("���������ʱ��Խ������Ƶ��Խ��")]public float attackRate;
    private float nextAttack = 0;//�´ι���ʱ��
    [Tooltip("��������")] public float attackRange;

    public bool isDead;//�ж��Ƿ�����

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
        TransitionToState(patrolState);//��Ϸһ��ʼ��ʱ����˽���Ѳ��״̬

    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            slider.gameObject.SetActive(false);
            return;
        }
        //�����Ǳ�ʾ��ǰ״̬����ִ��
        //�����ƶ�����һֱִ��
        currentState.OnUpdate(this);
        animator.SetInteger("state", animState);

        // ���¹���Ŀ��
        if (attackList.Count > 0)
        {
            targetPoint = attackList[0];
        }

    }

    /// <summary>
    /// �������ŵ������ƶ�
    /// </summary>
    public void MoveToTarget()
    {
        if (attackList.Count == 0)
        {
            agent.speed = 1.5f;
            // ����û�й���Ŀ�꣬����Ѳ��
            targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed);
        }
        else
        {
            
            // ����ɨ�赽���������򹥻�������ȥ
            targetPosition = attackList[0].position;

            // ��������Ŀ��ľ���
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance < 2.5f) // ����2.5fΪ��ȫ����
            {
                agent.speed = 0; // ֹͣ�ƶ�
                if (Time.time > nextAttack)
                {
                    AttackAction(); // ִ�й�������
                    nextAttack = Time.time + attackRate;
                }
            }

        }

        // NavMesh Surface��Ҫ���ھ�̬�����ϣ�
        agent.destination = targetPosition;
    }


    /// <summary>
    /// ����·��
    /// </summary>
    /// <param name="go"></param>
    public void LoadPath(GameObject go)
    {
        //����·��֮ǰ���list
        wayPoints.Clear();
        //����·��Ԥ���������е�����λ����Ϣ�������ص�list��
        foreach (Transform T in go.transform)
        {
            wayPoints.Add(T.position);
        }
    }

    /// <summary>
    /// �л�����״̬
    /// </summary>
    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnemyState(this);
    }

    /// <summary>
    /// �����ܵ��˺��۳�Ѫ��
    /// </summary>
    /// <param name="damage">�˺�ֵ</param>
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
            Destroy(Instantiate(deadEffect, transform.position, Quaternion.identity),3f);//����������ը��Ч��������
            
        }
    }

    /// <summary>
    /// ���˹������
    /// ��ͨ����
    /// </summary>
    public void AttackAction()
    {
        //�����˺͹���Ŀ�����С�ڹ������룬������������
        if (Vector3.Distance(transform.position, targetPoint.position) < attackRange)
        {
            if (Time.time > nextAttack)
            {
                //��������
                animator.SetTrigger("attack");
                agent.speed = 0;
                //�����´ι���ʱ��
                nextAttack = Time.time + attackRate;
            }
        }
    }

    /// <summary>
    /// ��������Detecting_player�е�attackList�б�
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
