using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int numForSpawn = 1;
    // Start is called before the first frame update
    void Start()
    {
        if(numForSpawn == 1)
        {
            transform.position = new Vector3(Random.Range(-2f, -1f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
        }
        if(numForSpawn == 2)
        {
            transform.position = new Vector3(Random.Range(1f, 2f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider coll)
    {
        var layerName = LayerMask.LayerToName(coll.gameObject.layer);
        if (layerName == "TeamA" || layerName == "TeamB")
        {
            if (coll.gameObject.TryGetComponent(out ShooterAgent target))
            {
                // target.IncrReward(1.0f);
                if(target.Health < target.initialHealth)
                {
                    target.Health += 10f;
                    if(target.Health > target.initialHealth)
                    {
                        target.Health = target.initialHealth;
                    }
                }
                if(numForSpawn == 1)
                {
                    transform.position = new Vector3(Random.Range(-2f, -1f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
                }
                if(numForSpawn == 2)
                {
                    transform.position = new Vector3(Random.Range(1f, 2f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
                }
            }
        }
    }
}
