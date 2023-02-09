using FishNet.Object;
using FishNet.Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponSwitching : NetworkBehaviour
{
    PlayerInputHandling inputHandling;
    RangerMovement moveScript;

    public GameObject primary;
    public GameObject secondary;
    public GameObject antiTitan;

    public GameObject[] weapons = new GameObject[1];
    GameObject[] spawnedWeapons = new GameObject[1];

    public GameObject[] publicWeapons = new GameObject[1];
    GameObject[] publicSpawnedWeapons = new GameObject[1];

    public GameObject publicPrimary;
    public GameObject publicSecondary;
    public GameObject publicAntiTitan;

    public Animator animator;
    public GameObject publicWeaponSwitcher;

    public TwoBoneIKConstraint primaryRig;
    public TwoBoneIKConstraint secondaryRig;
    public TwoBoneIKConstraint antiTitanRig;

    public string[] weaponTypes = new string[] { "Primary", "Secondary", "AntiTitan" };
    public bool[] isActive = new bool[] { false, false, false };

    /*private void Start()
    {
        

        inputHandling = GetComponentInParent<PlayerInputHandling>();

        Select();

        //Instantiate(primary, transform);

        /*SetupWeapons(primary, gameObject);

        Instantiate(secondary, transform);
        Instantiate(antiTitan, transform);

        Instantiate(publicPrimary, publicWeaponSwitcher.transform);
        Instantiate(publicSecondary, publicWeaponSwitcher.transform);
        Instantiate(publicAntiTitan, publicWeaponSwitcher.transform);

        Select();
    }*/

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)       
        {
            inputHandling = GetComponentInParent<PlayerInputHandling>();
            moveScript = GetComponentInParent<RangerMovement>();
            Select();

            /*for (int i = 0; i < weapons.Length; i++)
            {
                SpawnWeapons(weapons[i], publicWeapons[i], transform, publicWeaponSwitcher.transform, this, i);
            }*/
        }
        else
        {
            GetComponent<WeaponSwitching>().enabled = false;
        }
    }

    /*[ServerRpc]
    public void SpawnWeapons(GameObject weapon, GameObject publicWeapon, Transform weaponSwitcher, Transform publicWeaponSwitcher, WeaponSwitching script, int i)
    {
        GameObject weaponSpawn = Instantiate(weapon, weaponSwitcher);
        GameObject publicWeaponSpawn = Instantiate(publicWeapon, publicWeaponSwitcher);

        base.Spawn(weaponSpawn, base.Owner);
        base.Spawn(publicWeaponSpawn, base.Owner);
        SetSpawnedWeapons(weaponSpawn, publicWeaponSpawn, script, i);
    }

    [ObserversRpc]
    public void SetSpawnedWeapons(GameObject weapon, GameObject publicWeapon, WeaponSwitching script, int i)
    {
        script.spawnedWeapons[i] = weapon;
        script.publicSpawnedWeapons[i] = publicWeapon;
        weapon.GetComponent<GunScript>().Switch();
    }*/

    public void HandleAnimations()
    {
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            animator.SetBool(weaponTypes[i], isActive[i]);
        }
    }

    public void Select()
    {
        if (moveScript.canMove == false) return;
        ServerSelectWeapon(transform, inputHandling.weapon);
    }

    [ServerRpc]
    public void ServerSelectWeapon(Transform weaponSwitcher, int index)
    {
            SelectWeapon(weaponSwitcher, index);
    }

    [ObserversRpc(BufferLast = true)]
    public void SelectWeapon(Transform weaponSwitcher, int index)
    {

        int i = 0;
        foreach (Transform weapon in publicWeaponSwitcher.transform)
        {
            i++;
            if (i == index)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
        }

        i = 0;
        foreach (Transform weapon in weaponSwitcher.transform)
        {
            i++;
            if (i == index)
            {
                weapon.gameObject.SetActive(true);
                if (weapon.gameObject.TryGetComponent<GunScript>(out GunScript gun))
                    gun.Switch();
                if (weapon.gameObject.TryGetComponent<ProjectileWeapon>(out ProjectileWeapon projectileWeapon))
                    projectileWeapon.Switch();
            }
            else
                weapon.gameObject.SetActive(false);
        }
    }



    public void HandleRig()
    {
        switch (inputHandling.weapon)
        {
            case 1:
            case 5:
            case 7:
                primaryRig.weight = 1;
                secondaryRig.weight = 0;
                antiTitanRig.weight = 0;
                break;
            case 2:
            case 4:
            case 8:
                secondaryRig.weight = 1;
                primaryRig.weight = 0;
                antiTitanRig.weight = 0;
                break;
            case 3:
            case 6:
                antiTitanRig.weight = 1;
                secondaryRig.weight = 0;
                primaryRig.weight = 0;
                break;
        }
    }
}
