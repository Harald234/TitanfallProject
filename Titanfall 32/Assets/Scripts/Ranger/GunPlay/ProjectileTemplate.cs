using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Weapon/Projectile")]
public class ProjectileTemplate : ScriptableObject
{
    public new string name;

    public float damage;
    public float armorPiercing;

    public bool explode;
    public float explosionRange;
    public bool isSeeking;
    public bool hasGravityField;

    public bool useGravity;
    public float bounciness;

    public int maxCollisions;
    public float lifeTime;

    //public PhysicMaterial physics_mat;

    public GameObject impactEffect;
    //public AudioClip impactSoundEffect;


}
