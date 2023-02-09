using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MoveObject : MonoBehaviour
{
    public Transform target;
    bool move;
    float speed = 4f;

    public void Move()
    {
        move = true;
    }

    private void Update()
    {
        if (!move) return;
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }
}
