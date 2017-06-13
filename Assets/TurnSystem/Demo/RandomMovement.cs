using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour {

    public float RoamRadius;

    private NavMeshAgent agent;
    private Coroutine roam;

	// Use this for initialization
	void Awake ()
    {
        agent = GetComponent<NavMeshAgent>();
	}

    public void StartRoam()
    {
        StopRoam();
        agent.enabled = true;
        roam = StartCoroutine(DoRoam());
    }

    public void StopRoam()
    {
        if (roam != null)
        {
            agent.enabled = false;
            StopCoroutine(roam);
            roam = null;
        }
    }

    void RandomPath()
    {
        agent.SetDestination(RandomRoamDestination());
    }

    public Vector3 RandomRoamDestination()
    {
        // Get random direction and distance from current position
        Vector3 direction = Random.insideUnitSphere * RoamRadius;
        direction += transform.position;

        // Confine to navmesh
        NavMeshHit hit;
        NavMesh.SamplePosition(direction, out hit, RoamRadius, 1);
        Vector3 finalPosition = hit.position;

        return finalPosition;
    }

    IEnumerator DoRoam()
    {
        while (true)
        {
            // Move to random position within radius
            RandomPath();

            yield return new WaitUntil(() => agent.remainingDistance <= agent.radius);
        }
    }
}
