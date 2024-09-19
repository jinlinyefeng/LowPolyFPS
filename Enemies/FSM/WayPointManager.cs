using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 工具类
/// 给不同的敌人分配路线
/// </summary>
public class WayPointManager : MonoBehaviour
{
    private static WayPointManager _instance;
    //属性封装
    public static WayPointManager Instance
    {
        get { return _instance; }
    }

    //用两个list随机生成不同路线赋值给多个敌人，防止敌人出现走同一条路线
    public List<int> usingIndex = new List<int>();//每个敌人分配用到的路线id
    public List<int> rawIndex = new List<int>();//辅助的list 将多条路线打乱，重新分配


    private void Awake()
    {
        _instance = this;
        //分配路线id
        int tempCount = rawIndex.Count;
        for (int i = 0; i < tempCount; i++)
        {
            int tempIndex = Random.Range(0, rawIndex.Count);
            usingIndex.Add(rawIndex[tempIndex]);//游戏一开始运行时会让rawIndex列表中的元素随机打乱再添加到usingIndex里
            rawIndex.RemoveAt(tempIndex);
        }
    }
}
