using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

public class PlayerInputHandling : NetworkBehaviour
{
    public bool canShoot, shouldReload;
    public int weapon = 1;
    WeaponSwitching weaponSwitch;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            GetComponent<PlayerInputHandling>().enabled = false;
        }
        else
        {
            weaponSwitch = GetComponentInChildren<WeaponSwitching>();
        }
    }

    public void OnFire(InputValue value)
    {
        if (!base.IsOwner) return;
        canShoot = value.isPressed;
    }
    public void OnReload(InputValue value)
    {
        if (!base.IsOwner) return;
        shouldReload = value.isPressed;
    }

    public void OnPrimary()
    {
        if (!base.IsOwner) return;
        weapon = 1;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }
    public void OnSecondary()
    {
        if (!base.IsOwner) return;
        weapon = 2;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }
    public void OnAntiTitan()
    {
        if (!base.IsOwner) return;
        weapon = 3;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

    public void OnSniperRifle()
    {
        if (!base.IsOwner) return;
        weapon = 4;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

    public void OnRifle()
    {
        if (!base.IsOwner) return;
        weapon = 5;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

    public void OnTripleLauncher()
    {
        if (!base.IsOwner) return;
        weapon = 6;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

    public void OnShotgun()
    {
        if (!base.IsOwner) return;
        weapon = 7;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

    public void OnProjectilePistol()
    {
        if (!base.IsOwner) return;
        weapon = 8;
        weaponSwitch.Select();
        weaponSwitch.HandleRig();
    }

}
