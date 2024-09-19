using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���˽���Ѳ��״̬
/// </summary>
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 0;
        //����·��

        enemy.LoadPath(enemy.wayPointObj[WayPointManager.Instance.usingIndex[enemy.index]]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        //�ж������ǰidle�����Ѿ��������Ժ����ִ���ƶ�
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.animState = 1;
            enemy.MoveToTarget();
        }
        
        //������˺͵�����ľ���
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        //�����С��ʱ���ʾ�Ѿ��ߵ��˵�����
        if (distance <= 0.1f)
        {
            enemy.animState = 0;
            enemy.animator.Play("Idle");
            enemy.index++;//������һ��������
            enemy.index = Mathf.Clamp(enemy.index , 0 ,enemy.wayPoints.Count - 1);
            //���潫�ٴ��жϵ��˺�Ѳ��·������һ��������ľ��룬�������С��ĳһ��ֵ��
            //���жϵ�ǰ·���Ѿ����꣬�����õ������±꣬ʹ��������һ��
            if (Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.wayPoints.Count - 1]) <= 0.1f )
            {
                enemy.index = 0;
            }
        }

        if (enemy.attackList.Count > 0)
        {
            enemy.TransitionToState(enemy.attackState);
        }

    }
}
