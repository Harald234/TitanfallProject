using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerLayer : NetworkBehaviour
{
    private void FixedUpdate()
    {
        if (base.IsOwner)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            gameObject.layer = playerLayer;
        }
        else
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            gameObject.layer = enemyLayer;
        }
    }
}
