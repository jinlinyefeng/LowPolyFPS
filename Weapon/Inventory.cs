using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// 音效（拔枪 收枪）
/// </summary>
[System.Serializable]
public class SoundClips_start
{
    public AudioClip holsterWeaponSound;//收枪音效
    public AudioClip takeOutWeaponSound;//拔枪音效
}

    /// <summary>
    /// 武器库
    /// 人物的武器切换， 添加，去除功能
    /// </summary>
public class Inventory : MonoBehaviour
{
    private Player_Control playerController;

    [Header("音源")]
    private AudioSource mainAudioSource;

    public SoundClips_start soundClip;

    public List<GameObject> weapons = new List<GameObject>();

    public int weaponCount = 0;//武器数量计数器
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
    /// 更新武器编号
    /// </summary>
    public void ChangeCurrentWeaponID()
    {
        //通过滚轮切换武器
        //上0.1 不动0 下-0.1
        if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //下一把武器
            ChangeWeapon(currentWeaponID + 1);
        }
        else if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //上一把武器
            ChangeWeapon(currentWeaponID - 1);
        }

        for (int i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))//KeyCode.Alpha0 + i表示按下 i（1，2，3...8，9）
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
                //再判断num是否小于武器列表，小于就调用换武器，负责无效
                if (num < weapons.Count)
                {
                    ChangeWeapon(num);
                }
            }
        }
    }


    /// <summary>
    /// 武器切换
    /// </summary>
    /// <param name="weaponID">武器下标值</param>
    public void ChangeWeapon(int weaponID)
    {
        if(weapons.Count == 0) return;

        //IndexOf:获取列表中元素首次出现的索引
        //Max list 里获取最大元素
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
            //只有一种武器的时候不进行切换
            return;
        }

        currentWeaponID = weaponID;//更新武器索引

        //根据武器的编号显示对应的武器
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
    /// 捡起武器
    /// </summary>
    public void AddWeapon(GameObject weapon)
    {
        if (weapons.Contains(weapon))
        {
            playerController.PromptUI("背包中已存在此枪械");
            print("背包中已存在此枪械");
            return;
        }
        else
        {
            if (weapons.Count < 3)
            {
                weaponCount++;
                weapons.Add(weapon);
                ChangeWeapon(currentWeaponID + 1);//显示武器
                weapon.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 丢武器
    /// </summary>
    public void ThrowWeapon(GameObject weapon)
    {
            weaponCount--;
            weapons.Remove(weapon);
            ChangeWeapon(currentWeaponID - 1);
            weapon.gameObject.SetActive(false);
    }
}
