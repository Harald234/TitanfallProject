using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ControllBot : MonoBehaviour
{
    Vector3 destination;
    Vector3 lookTarget;

    NavMeshAgent agent;
    public Animator animator;

    bool isRunning;

    public bool lookAtPlayer;
    public bool stay;
    public bool findRandomPoint;

    BotHealth healthScript;

    public float range;

    GameObject[] centrePoints;
    Transform centerPoint = null;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        healthScript = GetComponentInChildren<BotHealth>();

        centrePoints = GameObject.FindGameObjectsWithTag("BotCenter");

        float currentDistance = Mathf.Infinity;
        for (int i = 0; i < centrePoints.Length; i++)
        {
            float newDistance = Vector3.Distance(centrePoints[i].transform.position, transform.position);
            if (newDistance < currentDistance)
            {
                currentDistance = newDistance;
                centerPoint = centrePoints[i].transform;
            }
        }
    }

    public void LookAtPlayer(Vector3 location)
    {
        Vector3 targetPosition = new Vector3(location.x, this.transform.position.y, location.z);
        transform.LookAt(targetPosition);
    }

    public void Move(Vector3 location)
    {
        destination = location;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void Update()
    {
        if (healthScript.isDead) return;

        if (stay)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }

        if (findRandomPoint)
        {
            if (agent.remainingDistance <= agent.stoppingDistance) //done with path
            {
                Vector3 point;
                if (RandomPoint(centerPoint.position, range, out point)) //pass in our centre point and radius of area
                {
                    Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                    agent.SetDestination(point);
                }
            }
        }
        else
            agent.SetDestination(destination);

        float velocity = agent.velocity.magnitude;
        if (velocity > 0.15f)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
        animator.SetBool("isRunning", isRunning);
    }
}
