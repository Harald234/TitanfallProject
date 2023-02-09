using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem.XR;
using UnityEngine.AI;

public class BotHealth : NetworkBehaviour, IDamageable, IRagdollable
{
    [SyncVar] public float health = 100f;

    public NavMeshAgent agent;

    public GameObject rig;
    public CapsuleCollider capsuleCollider;
    public Animator animator;

    public Collider[] ragDollColliders;
    public Rigidbody[] ragDollRigidBodies;

    private Transform hipsBone;
    public Transform main;

    public bool isDead;

    void Awake()
    {
        hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);

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
        health -= damage;
        if (health <= 0)
            Die();
    }

    public IEnumerator Ragdoll(Vector3 direction)
    {
        ActivateRagdoll();
        ApplyForce(direction);

        yield return new WaitForSeconds(2f);

        DeactivateRagdoll();
    }

    void ApplyForce(Vector3 direction)
    {
        foreach (Rigidbody body in ragDollRigidBodies)
        {
            body.AddForce(direction * 15f, ForceMode.Impulse);
        }
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

        capsuleCollider.enabled = false;
        agent.enabled = false;
    }

    private void AlignToRagdoll()
    {
        Vector3 originalHipsPosition = hipsBone.position;
        main.position = hipsBone.position;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            main.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        hipsBone.position = originalHipsPosition;
    }

    void DeactivateRagdoll()
    {
        AlignToRagdoll();

        animator.enabled = true;
        StartCoroutine(WalkCooldown());

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

        capsuleCollider.enabled = true;
        animator.SetTrigger("stand");
    }

    IEnumerator WalkCooldown()
    {
        yield return new WaitForSeconds(2f);
        agent.enabled = true;
    }


    void Die()
    {
        ActivateRagdoll();
        agent.enabled = false;
        isDead = true;
    }
}
