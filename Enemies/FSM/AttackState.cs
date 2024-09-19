using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人进入攻击状态
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
        //当前敌人没有目标， 此时敌人切换回巡逻状态
        if(enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
        }

        //当前敌人有目标，可能会存在多个目标的情况，要找距离最近的攻击目标
        if (enemy.attackList.Count > 1)
        {

            for (int i = 1; i < enemy.attackList.Count; i++)
            {
                //计算敌人和攻击列表里多个目标距离的差值 比较 攻击范围的目标和当前攻击目标的最小距离差
                //如果有其他攻击范围里的目标 与 敌人距离差最小，那么当前攻击目标就转换为第i个对象
                if (
                    Mathf.Abs(enemy.transform.position.x - enemy.attackList[i].position.x) <
                    Mathf.Abs(enemy.transform.position.x - enemy.targetPoint.position.x)
                    )
                {
                    enemy.targetPoint = enemy.attackList[i];
                }

            }

        }

        //当敌人只有一个攻击目标时，就只找list里的第一个
        if (enemy.attackList.Count == 1)
        {

            enemy.targetPoint = enemy.attackList[0];
        }

        //敌人攻击对象
        if (enemy.targetPoint != null && enemy.targetPoint.tag == "Player")
        {
            enemy.AttackAction();
        }

        //判断如果当前idle动画已经播放完以后才能执行移动
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.MoveToTarget();
        }
    }
}
