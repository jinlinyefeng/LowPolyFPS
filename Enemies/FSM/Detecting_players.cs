using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Detecting_players : MonoBehaviour
{
    Vector3 targetPosition;
    //���˵Ĺ���Ŀ�꣬����������ҽ������б�洢
    public List<Transform> attackList = new List<Transform>();
    [Tooltip("���������ʱ��Խ������Ƶ��Խ��")] public float attackRate;
    private float nextAttack = 0;//�´ι���ʱ��
    [Tooltip("��������")] public float attackRange;
    public Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }


    private void OnTriggerEnter(Collider other)
    {
        //�ų��ӵ����壬�����ӵ�Ҳ�����ɿɹ�������
        if (!attackList.Contains(other.transform) && gameObject.name != "Missing" && !enemy.isDead )
        {
            attackList.Add(other.transform);
            Debug.Log(attackList);
            enemy.UpdateAttackList(attackList); // ֪ͨ Enemy ���¹����б�
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemy != null && !enemy.isDead)
        {
            attackList.Remove(other.transform);
            enemy.UpdateAttackList(attackList); // ֪ͨ Enemy ���¹����б�
        }
    }
}
