using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName ="Gun", menuName ="Weapon/Gun")]
public class GunTemplate : ScriptableObject
{
    public new string name;
    public string type;
    public string family;

    public bool Automatic;

    public float damage;
    public float range;
    public float spread;
    public int bulletsPerShot;
    public float armorPiercing;

    public float ammoLeft;
    public float magSize;
    public float fireRate;
    public float reloadTime;
    public float emptyMagReloadTime;
    [HideInInspector]
    public bool isReloading;

    public float recoilX;
    public float recoilY;
    public float recoilZ;
    public float snappiness;
    public float returnSpeed;


}
