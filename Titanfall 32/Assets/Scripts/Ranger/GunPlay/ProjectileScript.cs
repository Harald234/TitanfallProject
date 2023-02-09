using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using static UnityEngine.GraphicsBuffer;
using System.Data.Common;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using FishNet;

public class ProjectileScript : NetworkBehaviour
{
    public ProjectileTemplate projectile;
    public Rigidbody rb;
    int collisions;
    public AudioSource source;

    public PhysicMaterial physics_mat;

    bool alreadyExploded;
    bool shouldSeek;
    GameObject currentEnemy = null;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
        Destroy(gameObject, projectile.lifeTime);
        rb = GetComponent<Rigidbody>();
        if (projectile.isSeeking)
            StartCoroutine(SeekTimer());
    }

    private void Setup()
    {
        //Create a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = projectile.bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;

        //Set gravity
        rb.useGravity = projectile.useGravity;
    }

    void Explode()
    {
        alreadyExploded = true;
        ServerExplode(projectile, transform, gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerExplode(ProjectileTemplate projectile, Transform projectileTransform, GameObject projectileObject)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, projectile.explosionRange);

        for (int i = 0; i < enemies.Length; i++)
        {
            IDamageable damageable = enemies[i].GetComponent<IDamageable>();
            if (damageable != null)
                damageable.Damage(projectile.damage, projectile.armorPiercing);
        }

        GameObject effect = Instantiate(projectile.impactEffect, projectileTransform.position, Quaternion.identity);
        base.Spawn(effect);
        projectileObject.GetComponent<AudioSource>().Play();

        Destroy(effect, 5f);
        Destroy(projectileObject, 5f);
    }

    [ServerRpc(RequireOwnership = false)]
    void ServerImpact(ProjectileTemplate projectile, Transform projectileTransform, GameObject projectileObject)
    {

        GameObject effect = Instantiate(projectile.impactEffect, projectileTransform.position, Quaternion.identity);
        base.Spawn(effect);
        projectileObject.GetComponent<AudioSource>().Play();

        Destroy(effect, 1f);
        Destroy(projectileObject);
    }

    [ServerRpc(RequireOwnership = false)]
    void DamageEnemy(GameObject enemy, ProjectileTemplate projectile)
    {
        var damageable = enemy.GetComponent<IDamageable>();

        if (damageable != null)
            damageable.Damage(projectile.damage, projectile.armorPiercing);
    }

    [ServerRpc(RequireOwnership = false)]
    void PullPlayers(Transform transform)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].gameObject.layer == 8) return;

            Vector3 direction = (enemies[i].transform.position - transform.position);

            IRagdollable ragdoll = enemies[i].GetComponent<IRagdollable>();
            if (ragdoll != null)
            {
                StartCoroutine(ragdoll.Ragdoll(direction));
            }
        }
    }

    private void FixedUpdate()
    {
        //HitEnemies();
        if (projectile.hasGravityField)
        {
            PullPlayers(transform);
        }
    }

    private void Update()
    {
        if (shouldSeek)
        {
            RotateSeekingProjectile();
        }
    }


    void RotateSeekingProjectile()
    {
        transform.position = Vector3.Lerp(transform.position, currentEnemy.transform.position, 2f * Time.deltaTime);
    }


    void SeekTarget()
    {
        float currentDistance = Mathf.Infinity;
        float distance;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 10f, 1<<9);
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<IDamageable>() == null) continue;

            distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance < currentDistance)
            {
                currentDistance = distance;
                currentEnemy = enemy.gameObject;
            }
        }

        if (currentEnemy == null) Destroy(gameObject);

        shouldSeek = true;
    }

    IEnumerator SeekTimer()
    {
        yield return new WaitForSeconds(0.25f);
        rb.velocity = new Vector3(0f, 0f, 0f);
        SeekTarget();
    }

    
    /*
    void HitEnemies()
    {
        if (!base.IsOwner) return;

        if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out hit, 1f, LayerMask.GetMask("Ground", "Enemy")))
        {
            collisions++;
            Debug.Log("hit");

            var enemy = hit.collider.GetComponent<IDamageable>();
            if (enemy != null && !alreadyExploded && projectile.explode)
                Explode();
            else if (enemy != null && !projectile.explode)
            {
                Debug.Log("enemy");
                DamageEnemy(hit.collider.gameObject, projectile);
            }

            if (collisions > projectile.maxCollisions)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
                rb.isKinematic = true;
                GetComponent<Renderer>().enabled = false;

                if (!alreadyExploded && projectile.explode)
                    Explode();
                else
                    ServerImpact(projectile, transform, gameObject);
            }
        }   
    }
    */

    
    private void OnCollisionEnter(Collision collision)
    {
        if (!base.IsOwner) return;

        //Don't count collisions with other bullets
        if (collision.collider.CompareTag("Bullet")) return;
        if (collision.gameObject.layer == 8) return;
        if (collision.gameObject.layer == 10) return;
        if (collision.gameObject.layer == 11) return;
        if (collision.gameObject.layer == 14) return;

        //Count up collisions
        collisions++;

        //Explode if bullet hits an enemy directly
        
        var enemy = collision.collider.GetComponent<IDamageable>();
        if (enemy != null && !alreadyExploded && projectile.explode && collision.gameObject.layer != 8)
            Explode();
        else if (enemy != null && !projectile.explode && !projectile.hasGravityField && collision.gameObject.layer != 8)
        {
            DamageEnemy(collision.gameObject, projectile);
        }

        if (collisions > projectile.maxCollisions)
        {
            rb.velocity = new Vector3(0f, 0f, 0f);
            rb.isKinematic = true;
            GetComponent<Renderer>().enabled = false;

            if (!alreadyExploded && projectile.explode)
                Explode();
            else
                ServerImpact(projectile, transform, gameObject);
        }
    }

}
