using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class HideFirstPerson : NetworkBehaviour
{
    private void FixedUpdate()
    {
        if (base.IsOwner)
        {
            SetLayerRecursively(gameObject, 8);
        }
        else
        {
            SetLayerRecursively(gameObject, 10);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
