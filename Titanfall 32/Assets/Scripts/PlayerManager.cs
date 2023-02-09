using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;
    private void Awake()
    {
        instance = this;
    }

    

    public void DamagePlayer(int playerID, int damage, int attackerID)
    {
        if (!base.IsServer)
            return;
    }
}
