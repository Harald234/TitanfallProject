using FishNet.Object;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class GunScript : NetworkBehaviour
{
    //bools 
    bool alreadyShot;
    bool activated;
    bool firstSwitch = true;

    //Reference
    public RaycastHit rayHit;

    Camera cam;
    float timeSinceLastShot;
    public GunTemplate gunValues;

    PlayerInputHandling inputHandling;

    public AudioSource source;
    public GameObject muzzlePoint;
    public GameObject publicMuzzlePoint;

    GameObject playerMain;
    Recoil recoil;
    WeaponSwitching weaponSwitching;

    public GameObject impactEffect;
    public GameObject hitEffect;

    Animator animator;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {

        }
        else
        {
            GetComponent<GunScript>().enabled = false;
        }
    }

    public void Switch()
    {
        if (firstSwitch)
        {
            activated = true;
            Debug.Log("switch");
            inputHandling = GetComponentInParent<PlayerInputHandling>();
            cam = GetComponentInParent<Camera>();
            weaponSwitching = GetComponentInParent<WeaponSwitching>();
            recoil = GetComponentInParent<Recoil>();
            animator = GetComponent<Animator>();

            playerMain = transform.root.gameObject;
            gunValues.ammoLeft = gunValues.magSize;

            firstSwitch = false;

            SetWeapon();
        }
        else
            SetWeapon();
    }

    public void SetWeapon()
    {
        muzzlePoint.GetComponentInChildren<ParticleSystem>().Stop();
        publicMuzzlePoint.GetComponentInChildren<ParticleSystem>().Stop();
    
        gunValues.isReloading = false;
        recoil.snappiness = gunValues.snappiness;
        recoil.returnSpeed = gunValues.returnSpeed;

        for (int i = 0; i < weaponSwitching.weaponTypes.Length; i++)
        {
            weaponSwitching.isActive[i] = false;
            if (weaponSwitching.weaponTypes[i] == gunValues.type)
            {
                weaponSwitching.isActive[i] = true;
            }
        }

        weaponSwitching.HandleAnimations();
    }

    private void Update()
    {
        if (!activated) return;

        if (gunValues.Automatic)
        {
            if (inputHandling.canShoot && !gunValues.isReloading && gunValues.ammoLeft > 0 && timeSinceLastShot > 1f / (gunValues.fireRate / 60f))
            {
                Shoot();
            }
        }
        else
        {
            if (!inputHandling.canShoot)
            {
                alreadyShot = false;
            }
            if (inputHandling.canShoot && alreadyShot == false && !gunValues.isReloading && gunValues.ammoLeft > 0 && timeSinceLastShot > 1f / (gunValues.fireRate / 60f))
            {
                alreadyShot = true;
                Shoot();
            }
        }

        if (inputHandling.shouldReload && gunValues.ammoLeft < gunValues.magSize && gunValues.ammoLeft > 0 && !gunValues.isReloading)
        {
            StartCoroutine(Reload(gunValues.reloadTime));
        }
        else if ((inputHandling.shouldReload && gunValues.ammoLeft <= 0 && !gunValues.isReloading))
        {
            StartCoroutine(Reload(gunValues.emptyMagReloadTime));
        }

        timeSinceLastShot += 1f * Time.deltaTime;
    }


    void Shoot()
    {
        animator.SetTrigger("Fire");
        timeSinceLastShot = 0f;
        gunValues.ammoLeft--;

        for (int i = 0; i < gunValues.bulletsPerShot; i++)
        {
            //Spread
            float x = Random.Range(-gunValues.spread, gunValues.spread);
            float y = Random.Range(-gunValues.spread, gunValues.spread);

            //Calculate Direction with Spread
            Vector3 direction = GetComponentInParent<Camera>().transform.forward + new Vector3(x, y, 0);

            if (Physics.Raycast(GetComponentInParent<Camera>().transform.position, direction, out rayHit, gunValues.range, LayerMask.GetMask("Ground", "Enemy")))
                ServerShoot(playerMain, gameObject, muzzlePoint, publicMuzzlePoint, impactEffect, hitEffect, gunValues, rayHit.transform.gameObject, rayHit.point, rayHit.normal);
            else
            {
                bool hit = Physics.Raycast(GetComponentInParent<Camera>().transform.position, direction, out rayHit, gunValues.range);
                if (hit == false)
                {
                    OnGunFire(gameObject, muzzlePoint, publicMuzzlePoint, playerMain, gunValues);
                }
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void ServerShoot(GameObject player, GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash, GameObject impactEffect, GameObject hitEffect, GunTemplate values, GameObject hitObject, Vector3 hitPoint, Vector3 normal)
    {
        if (hitObject != null && hitObject.TryGetComponent(out IDamageable enemy))
        {
            enemy.Damage(values.damage, gunValues.armorPiercing);

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
        OnGunFire(weapon, muzzleFlash, publicMuzzleFlash, player, values);
    }

    private IEnumerator Reload(float wait)
    {
        gunValues.isReloading = true;

        yield return new WaitForSeconds(wait);

        gunValues.ammoLeft = gunValues.magSize;
        gunValues.isReloading = false;
    }

    [ObserversRpc]
    void OnGunFire(GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash, GameObject player, GunTemplate values)
    {
        player.GetComponentInChildren<Recoil>().FireRecoil(values.recoilX, values.recoilY, values.recoilZ);

        weapon.GetComponent<AudioSource>().Stop();
        weapon.GetComponent<AudioSource>().Play();
        muzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
        publicMuzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
    }
}
