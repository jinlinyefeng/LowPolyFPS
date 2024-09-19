using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���˽��빥��״̬
/// </summary>
public class AttackState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 2;
        enemy.targetPoint = enemy.attackList[0];
    }

    public override void OnUpdate(Enemy enemy)
    {
        //��ǰ����û��Ŀ�꣬ ��ʱ�����л���Ѳ��״̬
        if(enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
        }

        //��ǰ������Ŀ�꣬���ܻ���ڶ��Ŀ��������Ҫ�Ҿ�������Ĺ���Ŀ��
        if (enemy.attackList.Count > 1)
        {

            for (int i = 1; i < enemy.attackList.Count; i++)
            {
                //������˺͹����б�����Ŀ�����Ĳ�ֵ �Ƚ� ������Χ��Ŀ��͵�ǰ����Ŀ�����С�����
                //���������������Χ���Ŀ�� �� ���˾������С����ô��ǰ����Ŀ���ת��Ϊ��i������
                if (
                    Mathf.Abs(enemy.transform.position.x - enemy.attackList[i].position.x) <
                    Mathf.Abs(enemy.transform.position.x - enemy.targetPoint.position.x)
                    )
                {
                    enemy.targetPoint = enemy.attackList[i];
                }

            }

        }

        //������ֻ��һ������Ŀ��ʱ����ֻ��list��ĵ�һ��
        if (enemy.attackList.Count == 1)
        {

            enemy.targetPoint = enemy.attackList[0];
        }

        //���˹�������
        if (enemy.targetPoint != null && enemy.targetPoint.tag == "Player")
        {
            enemy.AttackAction();
        }

        //�ж������ǰidle�����Ѿ��������Ժ����ִ���ƶ�
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.MoveToTarget();
        }
    }
}
