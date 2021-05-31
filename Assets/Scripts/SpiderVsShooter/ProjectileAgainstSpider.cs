using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAgainstSpider : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private Vector3 Direction;
    private ShooterVsSpiderAgent shooter;
    private SpiderVsShooterAgent target;
    private Vector3 shotPosition;
    public float projectileSpeed = 2f;

    void SelfDestruct(bool isHit)
    {
        shooter.AddRewardWithScore(isHit ? 1f : -0.2f);
        Destroy(this.gameObject);
    }

    void SelfDestructNoHit()
    {
        // Vector3 zero = Vector3.zero;
        SelfDestruct(false);
    }

    public void Initialize(ShooterVsSpiderAgent agent, SpiderVsShooterAgent target,Vector3 direction)
    {
        Direction = direction;
        this.gameObject.layer = LayerMask.NameToLayer("projectile");
        this.shooter = agent;
        this.target = target;
        var layerName = LayerMask.LayerToName(agent.gameObject.layer);
        if (layerName == "TeamA") this.gameObject.tag = "blueProjectile";
        else if (layerName == "TeamB") this.gameObject.tag = "redProjectile";
        shotPosition = agent.gameObject.transform.position;
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.velocity = Direction * projectileSpeed;
    }
    
    void Start()
    {
        Invoke(nameof(SelfDestructNoHit), 1f);
    }

    public void OnTriggerEnter(Collider coll)
    {
        var layerName = LayerMask.LayerToName(coll.gameObject.layer);
        if (layerName == "TeamA" || layerName == "TeamB" || layerName == "wall")
        {
            if (coll.gameObject.tag == "spiderAgent" || coll.gameObject.tag == "spiderLeg") // && (coll.gameObject.tag == "spiderAgent" || coll.gameObject.tag == "spiderLeg")
            {
                // target.AddRewardWithScore(-0.5f);
                target.GetShot(shooter.Damage);
                SelfDestruct(true);
            }
            else
            {
                SelfDestruct(false);
            }
        }
    }
}
