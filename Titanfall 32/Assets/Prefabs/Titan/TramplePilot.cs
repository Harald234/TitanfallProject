using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class TramplePilot : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            Trample(other.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void Trample(GameObject pilot)
    {
        pilot.TryGetComponent(out IRagdollable damageable);
        if (damageable != null)
            damageable.Ragdoll(new Vector3(0f, 0f, 0f));

    }
}
