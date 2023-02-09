using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;

public class PlayerHealth : NetworkBehaviour, IDamageable, IRagdollable
{
    [SyncVar] public float health = 100f;
    public TextMeshProUGUI healthText;
    public GameObject deathText;
    public CharacterController controller;

    public GameObject rig;
    public CapsuleCollider capsuleCollider;
    public Animator animator;

    public Collider[] ragDollColliders;
    public Rigidbody[] ragDollRigidBodies;

    RangerMovement moveScript;

    private Transform hipsBone;
    public Transform main;

    public Camera mainCamera;
    public Camera deathCamera;

    bool isRagdoll;
    bool isDead;

    public float regeneration = 5;

    private float timestamp = 0.0f;

    //GameObject[] centrePoints;
    //Transform centerPoint = null;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            healthText.enabled = false;
        }
    }

    void Start()
    {
        InvokeRepeating("Regenerate", 0.0f, 1.0f / regeneration);
    }

    void Awake()
    {
        deathText.SetActive(false);
        deathCamera.gameObject.SetActive(false);
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

        moveScript = GetComponentInParent<RangerMovement>();
    }


    public void Damage(float damage, float armorPiercing)
    {
        health -= damage;
        if (health <= 0)
        {
            timestamp = Time.time;
            ServerDie(gameObject, main.gameObject);
        }
    }

    void Regenerate()
    {
        if (health < 100f && health > 0f && Time.time > (timestamp + 10.0f))
            health += 1.0f;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        healthText.text = health.ToString();

        if (Input.GetKeyDown(KeyCode.H))
        {
            health = 0;
            ServerDie(gameObject, main.gameObject);
        }

        if (isRagdoll && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            Vector3 cameraPosition = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
            deathCamera.transform.LookAt(cameraPosition);
        }

        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ServerRespawn(this);
                Respawn();
            }
        }
    }

    void Respawn()
    {
        isDead = false;
        health = 100f;

        GameObject[] centrePoints = GameObject.FindGameObjectsWithTag("BotCenter");
        Transform centerPoint = null;

        for (int i = 0; i < centrePoints.Length; i++)
        {
            centerPoint = centrePoints[Random.Range(0, centrePoints.Length)].transform;
        }

        main.position = centerPoint.position;
    }

    [ServerRpc]
    void ServerRespawn(PlayerHealth script)
    {
        ObserverRespawn(script);
    }

    [ObserversRpc]
    void ObserverRespawn(PlayerHealth script)
    {
        //deathText.SetActive(false);
        script.RespawnRagdoll();
    }

    void RespawnRagdoll()
    {
        isRagdoll = false;
        animator.enabled = true;

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
        controller.enabled = true;
        moveScript.canMove = true;

        if (!base.IsOwner) return;
        deathCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }

    public IEnumerator Ragdoll(Vector3 direction)
    {
        ActivateRagdoll();
        ApplyForce(direction);

        yield return new WaitForSeconds(4f);

        DeactivateRagdoll();
    }

    void ApplyForce(Vector3 direction)
    {
        foreach (Rigidbody body in ragDollRigidBodies)
        {
            body.AddForce(-direction * 30f, ForceMode.Impulse);
        }
    }

    void ActivateRagdoll()
    {
        isRagdoll = true;
        moveScript.canMove = false;

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
        controller.enabled = false;

        ServerActivateCamera(deathCamera.gameObject, mainCamera.gameObject);
    }

    void ServerActivateCamera(GameObject deathCamera, GameObject mainCamera)
    {
        if (!base.IsOwner) return;
        deathCamera.SetActive(true);
        mainCamera.SetActive(false);
    }

    private void AlignToRagdoll()
    {
        Vector3 originalHipsPosition = hipsBone.position;
        main.position = hipsBone.position;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            main.position = new Vector3(transform.position.x, hitInfo.point.y + 1f, transform.position.z);
        }

        hipsBone.position = originalHipsPosition;
    }

    void DeactivateRagdoll()
    {
        AlignToRagdoll();

        StartCoroutine(WalkCooldown());
        animator.enabled = true;

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
        controller.enabled = true;
        moveScript.canMove = true;
        isRagdoll = false;
        deathCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerDie(GameObject player, GameObject main)
    {
        Die(player, main);
    }

    [ObserversRpc]
    public void Die(GameObject player, GameObject main)
    {
        player.GetComponent<PlayerHealth>().ActivateRagdoll();
        player.GetComponent<PlayerHealth>().isDead = true;
        main.GetComponent<RangerMovement>().ResetSpeed();
    }
}
