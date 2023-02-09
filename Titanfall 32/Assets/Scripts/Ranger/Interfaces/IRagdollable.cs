using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRagdollable
{
    public IEnumerator Ragdoll(Vector3 direction);
}
