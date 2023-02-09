using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class cameraShake : NetworkBehaviour
{

    public IEnumerator Shake (float magnitude, float duration)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * 2;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;

    }
}
