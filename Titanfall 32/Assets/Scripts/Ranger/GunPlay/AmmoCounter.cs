using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmmoCounter : MonoBehaviour
{
    public ProjectileWeaponTemplate template;
    public TextMeshPro textMesh;

    // Update is called once per frame
    void Update()
    {
        textMesh.text = template.ammoLeft.ToString();
    }
}
