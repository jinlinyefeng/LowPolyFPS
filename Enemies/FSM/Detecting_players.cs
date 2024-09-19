using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Detecting_players : MonoBehaviour
{
    Vector3 targetPosition;
    //敌人的攻击目标，场景中有玩家将会用列表存储
    public List<Transform> attackList = new List<Transform>();
    [Tooltip("攻击间隔，时间越长攻击频率越慢")] public float attackRate;
    private float nextAttack = 0;//下次攻击时间
    [Tooltip("攻击距离")] public float attackRange;
    public Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }


    private void OnTriggerEnter(Collider other)
    {
        //排除子弹物体，避免子弹也被当成可攻击对象
        if (!attackList.Contains(other.transform) && gameObject.name != "Missing" && !enemy.isDead )
        {
            attackList.Add(other.transform);
            Debug.Log(attackList);
            enemy.UpdateAttackList(attackList); // 通知 Enemy 更新攻击列表
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemy != null && !enemy.isDead)
        {
            attackList.Remove(other.transform);
            enemy.UpdateAttackList(attackList); // 通知 Enemy 更新攻击列表
        }
    }
}
