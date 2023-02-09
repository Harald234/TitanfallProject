using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TitanHealth : NetworkBehaviour, IDamageable
{
    private float health = 100f;
    private float armor = 3f;
    public TextMeshProUGUI healthText;

    public GameObject deathEffect;

    public GameObject rig;
    public Animator animator;

    public Collider[] ragDollColliders;
    public Rigidbody[] ragDollRigidBodies;

    public bool dead;

    private void Awake()
    {
        ragDollColliders = rig.GetComponentsInChildren<Collider>();
        ragDollRigidBodies = rig.GetComponentsInChildren<Rigidbody>();

        foreach (Collider col in ragDollColliders)
        {
            col.enabled = false;
        }

        foreach (Rigidbody body in ragDollRigidBodies)
        {
            body.isKinematic = true;
        }
    }

    public void Damage(float damage, float armorPiercing)
    {
        if (armorPiercing > armor)
        {
            health -= damage;
        }
        else if (armorPiercing == armor)
        {
            health -= (damage * 0.2f);
        }

        if (health <= 0 && !dead)
            Die();
    }

    void ActivateRagdoll()
    {
        animator.enabled = false;

        ragDollColliders = rig.GetComponentsInChildren<Collider>();
        ragDollRigidBodies = rig.GetComponentsInChildren<Rigidbody>();

        foreach (Collider col in ragDollColliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody body in ragDollRigidBodies)
        {
            body.isKinematic = false;
        }

        //boxCollider.enabled = false;
    }

    [ObserversRpc]
    public void Die()
    {
        ActivateRagdoll();

        //Debug.Log("Titan Dead");
        Instantiate(deathEffect, transform.position + new Vector3(0f, 4f, 0f), Quaternion.identity);
        dead = true;
        //Destroy(gameObject, 5f);
    }
}
