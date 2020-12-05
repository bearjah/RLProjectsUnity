using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Target : MonoBehaviour
{
    public SpiderAgent spider;
    private float distanceToSpider;
    private bool isRandomRespawn = true;

    private void Awake()
    {
        Respawn();
    }
    void OnTriggerEnter(Collider col)
    {
        // Touched target.
        if (col.gameObject.CompareTag("spiderAgent"))
        {
            gameObject.SetActive(false);
            spider.ReachedTarget();
            Respawn();
        }
    }
    public void Respawn()
    {
        this.gameObject.SetActive(true);
        if(isRandomRespawn)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = new Vector3(Random.Range(-4.5f, 4.5f), 1f, Random.Range(-4.5f, 4.5f)) + transform.parent.transform.position;
                distanceToSpider = Vector3.Distance(randomPosition, spider.transform.position);
            } while (distanceToSpider < 3f);

            transform.position = randomPosition;
            return;
        }
        transform.position = new Vector3(0f, 1f, 0f) + transform.parent.transform.position;
        // Vector3 randomPosition;
        // do
        // {
        //     randomPosition = new Vector3(Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.x, 1f, Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.z);
        //     distanceToSpider = Vector3.Distance(randomPosition, agent.transform.position);
        // } while (distanceToSpider < 3f);

        // transform.position = randomPosition;
    }
}


