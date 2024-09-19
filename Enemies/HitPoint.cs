using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoint : MonoBehaviour
{
    public int Max_Damage;
    public int Min_Damage;

    private void OnTriggerEnter(Collider other)
    {
        //Íæ¼ÒÊÜµ½ÉËº¦¿ÛÑª
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player_Control>().PlayerHealth(Random.Range(Min_Damage, Max_Damage));
        }
    }
}
