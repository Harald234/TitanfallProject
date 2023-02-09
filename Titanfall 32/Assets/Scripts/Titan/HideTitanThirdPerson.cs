using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class HideTitanThirdPerson : NetworkBehaviour
{
    private void FixedUpdate()
    {
        if (base.IsOwner)
        {
            SetLayerRecursively(gameObject, 12);
        }
        else
        {
            SetLayerRecursively(gameObject, 8);
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
