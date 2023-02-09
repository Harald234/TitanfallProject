using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class ProjectileWeapon : NetworkBehaviour
{
    bool alreadyShot;
    bool activated;
    bool firstSwitch = true;

    public RaycastHit rayHit;

    float timeSinceLastShot;
    public ProjectileWeaponTemplate gunValues;

    PlayerInputHandling inputHandling;

    public AudioSource source;
    public GameObject muzzlePoint;
    public GameObject publicMuzzlePoint;

    GameObject playerMain;
    Recoil recoil;

    Animator animator;

    RangerMovement moveScript;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {

        }
        else
        {
            GetComponent<ProjectileWeapon>().enabled = false;
        }
    }

    public void Switch()
    {
        if (firstSwitch)
        {
            activated = true;
            inputHandling = GetComponentInParent<PlayerInputHandling>();
            recoil = GetComponentInParent<Recoil>();
            animator = GetComponent<Animator>();
            moveScript = GetComponentInParent<RangerMovement>();

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
        timeSinceLastShot = 0f;
        gunValues.ammoLeft--;
        animator.SetTrigger("Fire");
        ServerShoot(playerMain, gameObject, muzzlePoint, publicMuzzlePoint, muzzlePoint.transform, gunValues, moveScript);
    }


    [ServerRpc(RequireOwnership = false)]
    private void ServerShoot(GameObject player, GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash, Transform point, ProjectileWeaponTemplate values, RangerMovement moveScript)
    {

        for (int i = 0; i < values.bulletsPerShot; i++)
        {
            //Spread
            float x = Random.Range(-values.spread, values.spread);
            float y = Random.Range(-values.spread, values.spread);

            //Calculate Direction with Spread
            Vector3 direction = player.GetComponentInChildren<Camera>().transform.forward + new Vector3(x, y, 0);

            var projectile = Instantiate(values.projectile, point.position, point.rotation);

            base.Spawn(projectile, base.Owner);

            Projectile(projectile, direction, values, moveScript);
        }

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
    void OnGunFire(GameObject weapon, GameObject muzzleFlash, GameObject publicMuzzleFlash, GameObject player, ProjectileWeaponTemplate values)
    {
        player.GetComponentInChildren<Recoil>().FireRecoil(values.recoilX, values.recoilY, values.recoilZ);

        weapon.GetComponent<AudioSource>().Stop();
        weapon.GetComponent<AudioSource>().Play();
        muzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
        publicMuzzleFlash.GetComponentInChildren<ParticleSystem>().Play();
    }

    [ObserversRpc]
    void Projectile(GameObject projectile, Vector3 direction, ProjectileWeaponTemplate values, RangerMovement moveScript)
    {
        projectile.GetComponent<Rigidbody>().AddForce(direction.normalized * values.bulletVelocityForce, ForceMode.Impulse);
        projectile.GetComponent<Rigidbody>().velocity += new Vector3(moveScript.controller.velocity.x, moveScript.controller.velocity.y, moveScript.controller.velocity.z);
    }
}
