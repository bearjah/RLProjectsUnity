using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    // private int max_ammo = 30;
    public int numForSpawn = 1;

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
                if(target.Ammo < target.initialAmmo)
                {
                    target.Ammo += 25;
                    if(target.Ammo > target.initialAmmo)
                    {
                        target.Ammo = target.initialAmmo;
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

