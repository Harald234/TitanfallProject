using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
//using static Unity.VisualScripting.Member;

public class FireTitanRifle : NetworkBehaviour
{
    public EnterVanguardTitan enterScript;
    public VanguardMovement moveScript;
    VanguardCamera cameraScript;

    public Animator Arms;
    public Animator titanAnimator;
    public Animator XO16Animator;
    public Animator secondXO16Animator;

    public Camera cam;
    public GameObject muzzleFlash;
    public GameObject publicMuzzleFlash;

    public GameObject impactEffect;
    public GameObject hitEffect;

    public AudioClip gunShot;
    public AudioSource audioSource;

    bool canShoot;
    bool canReload;
    [NonSerialized] public bool isReloading = false;

    //public float timeBetweenShots, timeBetweenShooting;
    float timeSinceLastShot = 0f;
    float fireRate = 400f;
    float spread = 0.08f;
    float currentSpread;
    float armorPiercing = 4;
    float range = 300f;
    float damage = 100f;

    public int bulletsLeft, magazinSize;

    RaycastHit rayHit;

    private void Awake()
    {
        cameraScript = GetComponent<VanguardCamera>();
        muzzleFlash.GetComponentInChildren<ParticleSystem>().Stop();
        publicMuzzleFlash.GetComponentInChildren<ParticleSystem>().Stop();
    }

    void HandleInput()
    {

        if (canShoot && bulletsLeft > 0 && !isReloading && timeSinceLastShot > 1f / (fireRate / 60f))
        {
            Shoot();
        }

        if (canReload && bulletsLeft < magazinSize && !moveScript.isDashing && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    public void OnFire(InputValue value)
    {
        if (!base.IsOwner)
            return;
        canShoot = value.isPressed;
    }

    public void OnReload(InputValue value)
    {
        if (!base.IsOwner)
            return;
        canReload = value.isPressed;
    }

    void Update()
    {
        if (enterScript.inTitan)
        {
            HandleInput();

            timeSinceLastShot += 1f * Time.deltaTime;

            if (moveScript.isWalking)
                currentSpread = (spread / 2);
            else
                currentSpread = spread;
        }
    }

    void Shoot()
    {
        timeSinceLastShot = 0f;
        bulletsLeft--;

        //Spread
        float x = UnityEngine.Random.Range(-currentSpread, currentSpread);
        float y = UnityEngine.Random.Range(-currentSpread, currentSpread);

        //Calculate Direction with Spread
        Vector3 direction = cameraScript.cam.transform.forward + new Vector3(x, y, 0);

        if (Physics.Raycast(cameraScript.cam.transform.position, direction, out rayHit, range, LayerMask.GetMask("Ground", "Enemy")))
            ServerShoot(gameObject, muzzleFlash, publicMuzzleFlash, impactEffect, hitEffect, rayHit.transform.gameObject, rayHit.point, rayHit.normal);

        bool hit = Physics.Raycast(cameraScript.cam.transform.position, direction, out rayHit, range);
        if (hit == false)
        {
            OnGunFire(gameObject, muzzleFlash, publicMuzzleFlash);
        }

    }


    [ServerRpc(RequireOwnership = false)]
    private void ServerShoot(GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash, GameObject impactEffect, GameObject hitEffect, GameObject hitObject, Vector3 hitPoint, Vector3 normal)
    {
        if (hitObject != null && hitObject.TryGetComponent(out IDamageable enemy))
        {
            enemy.Damage(damage, armorPiercing);

            GameObject impact = Instantiate(hitEffect, hitPoint, Quaternion.identity) as GameObject;
            impact.transform.forward = normal;

            base.Spawn(impact);

            Destroy(impact, 1.5f);
        }
        else
        {
            GameObject impact = Instantiate(impactEffect, hitPoint, Quaternion.identity) as GameObject;
            impact.transform.forward = normal;

            base.Spawn(impact);

            Destroy(impact, 1.5f);
        }
        //Graphics
        OnGunFire(weapon, muzzleFlash, publicMuzzleFlash);
    }

    [ObserversRpc]
    void OnGunFire(GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash)
    {
        weapon.GetComponentInChildren<Recoil>().FireRecoil(-2f, 2f, 0.4f);

        if (!base.IsOwner) return;
        weapon.GetComponent<AudioSource>().Stop();
        weapon.GetComponent<AudioSource>().Play();
        muzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
        publicMuzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Arms.SetTrigger("reload");
        titanAnimator.SetTrigger("reload");
        XO16Animator.SetTrigger("reload");
        secondXO16Animator.SetTrigger("reload");

        yield return new WaitForSeconds(2.5f);

        bulletsLeft = magazinSize;
        isReloading = false;
    }
}