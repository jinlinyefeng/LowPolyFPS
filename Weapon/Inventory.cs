using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// ��Ч����ǹ ��ǹ��
/// </summary>
[System.Serializable]
public class SoundClips_start
{
    public AudioClip holsterWeaponSound;//��ǹ��Ч
    public AudioClip takeOutWeaponSound;//��ǹ��Ч
}

    /// <summary>
    /// ������
    /// ����������л��� ��ӣ�ȥ������
    /// </summary>
public class Inventory : MonoBehaviour
{
    private Player_Control playerController;

    [Header("��Դ")]
    private AudioSource mainAudioSource;

    public SoundClips_start soundClip;

    public List<GameObject> weapons = new List<GameObject>();

    public int weaponCount = 0;//��������������
    public int currentWeaponID;


    private void Awake()
    {
        mainAudioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<Player_Control>();
    }
    void Start()
    {
        currentWeaponID = -1;
    }


    void Update()
    {
        ChangeCurrentWeaponID();
    }

    /// <summary>
    /// �����������
    /// </summary>
    public void ChangeCurrentWeaponID()
    {
        //ͨ�������л�����
        //��0.1 ����0 ��-0.1
        if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //��һ������
            ChangeWeapon(currentWeaponID + 1);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //��һ������
            ChangeWeapon(currentWeaponID - 1);
        }

        for (int i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))//KeyCode.Alpha0 + i��ʾ���� i��1��2��3...8��9��
            {
                int num = 0;
                if (i == 10)
                {
                    num = 10;
                }
                else
                {
                    num = i - 1;
                }
                //���ж�num�Ƿ�С�������б�С�ھ͵��û�������������Ч
                if (num < weapons.Count)
                {
                    ChangeWeapon(num);
                }
            }
        }
    }


    /// <summary>
    /// �����л�
    /// </summary>
    /// <param name="weaponID">�����±�ֵ</param>
    public void ChangeWeapon(int weaponID)
    {
        if(weapons.Count == 0) return;

        //IndexOf:��ȡ�б���Ԫ���״γ��ֵ�����
        //Max list ���ȡ���Ԫ��
        if (weaponID > weapons.Max(weapons.IndexOf))
        {
            weaponID = weapons.Min(weapons.IndexOf);
        }
        else if (weaponID < weapons.Min(weapons.IndexOf))
        {
            weaponID = weapons.Max(weapons.IndexOf);
        }

        if (weaponID == currentWeaponID)
        {
            //ֻ��һ��������ʱ�򲻽����л�
            return;
        }

        currentWeaponID = weaponID;//������������

        //���������ı����ʾ��Ӧ������
        for (int i = 0; i < weapons.Count; i++)
        {
            if(weaponID == i)
            {
                weapons[i].gameObject.SetActive(true);
                mainAudioSource.clip = soundClip.takeOutWeaponSound;
                mainAudioSource.Play();
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
                mainAudioSource.clip = soundClip.holsterWeaponSound;
                mainAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void AddWeapon(GameObject weapon)
    {
        if (weapons.Contains(weapon))
        {
            playerController.PromptUI("�������Ѵ��ڴ�ǹе");
            print("�������Ѵ��ڴ�ǹе");
            return;
        }
        else
        {
            if (weapons.Count < 3)
            {
                weaponCount++;
                weapons.Add(weapon);
                ChangeWeapon(currentWeaponID + 1);//��ʾ����
                weapon.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    public void ThrowWeapon(GameObject weapon)
    {
            weaponCount--;
            weapons.Remove(weapon);
            ChangeWeapon(currentWeaponID - 1);
            weapon.gameObject.SetActive(false);
    }
}
