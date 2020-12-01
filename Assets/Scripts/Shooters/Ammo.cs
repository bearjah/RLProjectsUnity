using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    void Start()
    {
        transform.position = new Vector3(Random.Range(-4f, 4f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
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
                // target.IncrReward(2.0f);
                if(target.Ammo < target.initialAmmo)
                {
                    target.Ammo += 50;
                    if(target.Ammo > target.initialAmmo)
                    {
                        target.Ammo = target.initialAmmo;
                    }
                    transform.position = new Vector3(Random.Range(-2f, -1f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
                }
            }
        }
    }
}

