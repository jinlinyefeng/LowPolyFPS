using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人进入巡逻状态
/// </summary>
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.animState = 0;
        //加载路线

        enemy.LoadPath(enemy.wayPointObj[WayPointManager.Instance.usingIndex[enemy.index]]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        //判断如果当前idle动画已经播放完以后才能执行移动
        if (!enemy.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            enemy.animState = 1;
            enemy.MoveToTarget();
        }
        
        //计算敌人和导航点的距离
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        //距离很小的时候表示已经走到了导航点
        if (distance <= 0.1f)
        {
            enemy.animState = 0;
            enemy.animator.Play("Idle");
            enemy.index++;//设置下一个导航点
            enemy.index = Mathf.Clamp(enemy.index , 0 ,enemy.wayPoints.Count - 1);
            //下面将再次判断敌人和巡逻路线最后后一个导航点的距离，如果距离小于某一定值，
            //则判断当前路线已经走完，就重置导航点下标，使其重新走一遍
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
