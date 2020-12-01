using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    // Start is called before the first frame update
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
                if(target.Health < target.initialHealth)
                {
                    target.Health += 10f;
                    if(target.Health > target.initialHealth)
                    {
                        target.Health = target.initialHealth;
                    }
                    transform.position = new Vector3(Random.Range(1f, 2f), 0.6f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
                }
            }
        }
    }
}
