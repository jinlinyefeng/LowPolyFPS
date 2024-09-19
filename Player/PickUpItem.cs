using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ʰȡ
/// </summary>
public class PickUpItem : MonoBehaviour
{
    [Tooltip("������ת�ٶ�")] public float rotateSpeed;
    [Tooltip("�������")] public int itemID;
    private GameObject weaponModel;
    private Inventory inventory;
    public int weaponCount = 0;//��������������

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

            Player_Control player = other.GetComponent<Player_Control>();//�Ȼ�ȡ��ͬ���壨player�������
            inventory = player.GetComponentInChildren<Inventory>();//�ڴ�player�ϻ�ȡ���ű�Inventory,ʵ�ֿ�ű�����
            weaponCount = inventory.weapons.Count;
            if (weaponCount == 3)
            player.PromptUI("��������");
            Debug.Log("��������");
            weaponModel = GameObject.Find("Player/Assault_Rifle_Arm/Inventory/").gameObject.transform.GetChild(itemID).gameObject;
            if (weaponCount < 3 || inventory.weapons.Contains(weaponModel))//С�����������Ż��������������ɾ��ģ��
            {
                player.PickUpWeapon(itemID, weaponModel);
                Destroy(gameObject);
            }

        }

    }
}
