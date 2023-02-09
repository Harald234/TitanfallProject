using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    SkinnedMeshRenderer skin;

    float timeSinceLastShot = 0f;

    private void Start()
    {
        skin = GetComponent<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastShot += 1f * Time.deltaTime;

        if (timeSinceLastShot > 1f / (123 / 60f))
        {
            if (skin.enabled == true)
                skin.enabled = false;
            else
                skin.enabled = true;
            timeSinceLastShot = 0f;
        }
    }
}
