using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName ="Gun", menuName ="Weapon/Gun")]
public class GunValues : ScriptableObject
{
    public new string name;
    public string type;
    public string family;

    public bool Automatic;
    public bool hitscan;

    public float damage;
    public float range;
    public float spread;

    public float ammoLeft;
    public float magSize;
    public float fireRate;
    public float reloadTime;
    [HideInInspector]
    public bool isReloading;

    
}
