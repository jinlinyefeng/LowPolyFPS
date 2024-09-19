using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public abstract void GunFire();

    public abstract void Reload();

    public abstract void ExpandingCrossUpdate(float expanDegree);//控制准星开合

    public abstract void DoReloadAnimation();

    public abstract void AimIn();

    public abstract void AimOut(); 
}
