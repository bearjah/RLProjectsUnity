using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody Rigidbody;
    private Vector3 Direction;
    private ShooterAgent shooter;
    private Vector3 shotPosition;
    public float rojectileSpeed = 15f;

    void SelfDestruct(bool isHit, Vector3 targetPosition)
    {
        // float range = (shotPosition - targetPosition).sqrMagnitude; 
        // shooter.IncrReward(isHit ? Mathf.Min(range, shooter.MaxRangeReward) : shooter.MissPenalty);
        shooter.IncrReward(isHit ? 1f : -0.2f);
        Destroy(this.gameObject);
    }

    void SelfDestructNoHit()
    {
        Vector3 zero = Vector3.zero;
        SelfDestruct(false, zero);
    }

    public void Initialize(ShooterAgent agent, Vector3 direction)
    {
        Direction = direction;
        this.gameObject.layer = LayerMask.NameToLayer("projectile");
        this.shooter = agent;
        var layerName = LayerMask.LayerToName(agent.gameObject.layer);
        if (layerName == "TeamA") this.gameObject.tag = "blueProjectile";
        else if (layerName == "TeamB") this.gameObject.tag = "redProjectile";
        shotPosition = agent.gameObject.transform.position;
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.velocity = Direction * rojectileSpeed;
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
            if (coll.gameObject.TryGetComponent(out ShooterAgent target))
            {
                if (target.Team == shooter.Team)
                {
                    return;
                }
                target.IncrReward(-0.5f);
                target.GetShot(shooter.Damage);
                SelfDestruct(true, target.gameObject.transform.position);
            }
            else
            {
                SelfDestruct(false, coll.gameObject.transform.position);
            }
        }
    }
}
