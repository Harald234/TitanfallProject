using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class CruchScript : NetworkBehaviour
{
    public Transform crushCheck;
    public GameObject titanMain;

    private void OnTriggerEnter(Collider other)
    {
        ServerCrush(crushCheck, titanMain);
    }


    [ServerRpc(RequireOwnership = false)]
    void ServerCrush(Transform crushCheck, GameObject titan)
    {
        Collider[] enemies = Physics.OverlapSphere(crushCheck.position, 10f);

        for (int i = 0; 9 < enemies.Length; i++)
        {
            if (enemies[i].gameObject == titan) continue;

            IDamageable damageable = enemies[i].GetComponent<IDamageable>();
            if (damageable != null)
                damageable.Damage(100, 5);
        }

    }
}
