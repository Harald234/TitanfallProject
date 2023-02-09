using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/ProjectileWeapon")]
public class ProjectileWeaponTemplate : ScriptableObject
{
    public new string name;
    public string type;
    public string family;

    public GameObject projectile;

    public bool Automatic;

    public float bulletVelocityForce;

    public int bulletsPerShot;
    public float spread;

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
