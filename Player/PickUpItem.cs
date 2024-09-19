using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器拾取
/// </summary>
public class PickUpItem : MonoBehaviour
{
    [Tooltip("武器旋转速度")] public float rotateSpeed;
    [Tooltip("武器编号")] public int itemID;
    private GameObject weaponModel;
    private Inventory inventory;
    public int weaponCount = 0;//武器数量计数器

    // Start is called before the first frame update
    void Start()
    { 
        rotateSpeed = 100f;
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            Player_Control player = other.GetComponent<Player_Control>();//先获取不同物体（player）的组件
            inventory = player.GetComponentInChildren<Inventory>();//在从player上获取到脚本Inventory,实现跨脚本访问
            weaponCount = inventory.weapons.Count;
            if (weaponCount == 3)
            player.PromptUI("背包已满");
            Debug.Log("背包已满");
            weaponModel = GameObject.Find("Player/Assault_Rifle_Arm/Inventory/").gameObject.transform.GetChild(itemID).gameObject;
            if (weaponCount < 3 || inventory.weapons.Contains(weaponModel))//小于三把武器才会添加武器，并且删除模型
            {
                player.PickUpWeapon(itemID, weaponModel);
                Destroy(gameObject);
            }

        }

    }
}
