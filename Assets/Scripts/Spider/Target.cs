using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    public SpiderAgent agent;
    float distanceToAgent;

    void OnTriggerEnter(Collider col)
    {
        // Touched target.
        if (col.gameObject.CompareTag("spiderAgent"))
        {
            gameObject.SetActive(false);
            agent.ReachedTarget();
            Respawn();
        }
    }
    public void Respawn()
    {
        this.gameObject.SetActive(true);
        Vector3 randomPosition;
        do
        {
            randomPosition = new Vector3(Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.x, 1f, Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.z);
            distanceToAgent = Vector3.Distance(randomPosition, agent.transform.position);
        } while (distanceToAgent < 3f);

        transform.position = randomPosition;
    }
}


