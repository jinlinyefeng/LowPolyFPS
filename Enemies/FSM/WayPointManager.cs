using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ������
/// ����ͬ�ĵ��˷���·��
/// </summary>
public class WayPointManager : MonoBehaviour
{
    private static WayPointManager _instance;
    //���Է�װ
    public static WayPointManager Instance
    {
        get { return _instance; }
    }

    //������list������ɲ�ͬ·�߸�ֵ��������ˣ���ֹ���˳�����ͬһ��·��
    public List<int> usingIndex = new List<int>();//ÿ�����˷����õ���·��id
    public List<int> rawIndex = new List<int>();//������list ������·�ߴ��ң����·���


    private void Awake()
    {
        _instance = this;
        //����·��id
        int tempCount = rawIndex.Count;
        for (int i = 0; i < tempCount; i++)
        {
            int tempIndex = Random.Range(0, rawIndex.Count);
            usingIndex.Add(rawIndex[tempIndex]);//��Ϸһ��ʼ����ʱ����rawIndex�б��е�Ԫ�������������ӵ�usingIndex��
            rawIndex.RemoveAt(tempIndex);
        }
    }
}
