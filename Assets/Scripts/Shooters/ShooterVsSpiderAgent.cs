using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using TMPro;

[RequireComponent(typeof(Rigidbody))]

public class ShooterVsSpiderAgent : Agent
{
    public float score;
    private float MovementSpeed;
    private float RotationSpeed;
    [HideInInspector] public float Ammo;
    public string otherTeamLayer;
    [HideInInspector] public float initialAmmo;
    [HideInInspector] public float Damage;
    public ProjectileAgainstSpider projectile;
    public Transform shootingPoint;
    [HideInInspector] public Rigidbody rbAgent;
    private float distanceToSpider;
    private bool isRandomRespawn;
    public SpiderVsShooterAgent spider;
    // private float maxDistance = Mathf.Sqrt(10 * 10 + 10 * 10);
    [HideInInspector] EnvironmentParameters EnvironmentParameters;
    public event Action OnEnvironmentReset;

    private void FixedUpdate()
    {
        var survival = 1f;
        // float distanceFromCenter = Mathf.Sqrt(Mathf.Pow(transform.position.x, 2f) + Mathf.Pow(transform.position.z, 2f)) - transform.parent.transform.position;
        // var distanceFromCenterNormalized = distanceFromCenter/(maxDistance/2f);
        // var distanceFromCenterReward = 1f - Mathf.Pow(distanceFromCenterNormalized, 2f/5f);
        // AddRewardWithScore(distanceFromCenterReward/MaxStep);
        if((Mathf.Abs(transform.position.x) > 4f + transform.parent.transform.position.x) || (Mathf.Abs(transform.position.z) > 4f + transform.parent.transform.position.z))
        {
            AddRewardWithScore(-1f * survival / MaxStep);
            return;
        }
        AddRewardWithScore(survival / MaxStep);        
    }

    private void Awake()
    {
        rbAgent = GetComponent<Rigidbody>();
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    private void Shoot()
    {
        var layerMask = (1 << LayerMask.NameToLayer(otherTeamLayer)) + (1 << LayerMask.NameToLayer("wall"));
        var direction = transform.forward;

        var spawnedProjectile = Instantiate(projectile, shootingPoint.position, Quaternion.Euler(0f, -90f, 0f));

        spawnedProjectile.Initialize(this, spider, direction);

        transform.parent.Find("RedBoard").GetComponent<TextMeshPro>().text = $"Ammo:{(Ammo/initialAmmo).ToString("0.000")}";
    }
    
    public void AddRewardWithScore(float reward)
    {
        AddReward(reward);
        score += reward;
        transform.parent.Find("ShooterScore").GetComponent<TextMeshPro>().text = $"ShooterScore:\n{score.ToString("0.000")}";
    }

    public void MoveAgent(int forwardMovement, int rightMovement, int rotation)
    {
        switch (forwardMovement)
        {
            case 1:
                rbAgent.MovePosition(transform.position + transform.forward * MovementSpeed * Time.deltaTime);
                break;
            case 2:
                rbAgent.MovePosition(transform.position - transform.forward * MovementSpeed * Time.deltaTime);
                break;
        }
        switch (rightMovement)
        {
            case 1:
                rbAgent.MovePosition(transform.position - transform.right * MovementSpeed * Time.deltaTime);
                break;
            case 2:
                rbAgent.MovePosition(transform.position + transform.right * MovementSpeed * Time.deltaTime);
                break;    
        }
        switch (rotation)
        {
            case 1:
                transform.Rotate(transform.up, RotationSpeed * Time.deltaTime);
                break;
            case 2:
                transform.Rotate(transform.up, -RotationSpeed * Time.deltaTime);
                break;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.Ammo/EnvironmentParameters.GetWithDefault("initialAmmo", initialAmmo));
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int forwardMovement = Mathf.FloorToInt(vectorAction[0]);
        int rightMovement = Mathf.FloorToInt(vectorAction[1]);
        int rotation = Mathf.FloorToInt(vectorAction[2]);
        int toShoot = Mathf.FloorToInt(vectorAction[3]);
        if (this.Ammo > 0 && toShoot == 1)
        {
            this.Ammo--;
            Shoot();
        }
        MoveAgent(forwardMovement, rightMovement, rotation);
    }

    public override void Heuristic(float[] actionsOut)
    {
        //movement
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2f;
        }
        else
        {
           actionsOut[0] = 0f; 
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 2f;
        }
        else
        {
           actionsOut[1] = 0f; 
        }
        //rotate
        if (Input.GetKey(KeyCode.E))
        {
            actionsOut[2] = 1f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            actionsOut[2] = 2f;
        }
        else
        {
            actionsOut[2] = 0f;
        }
        actionsOut[3] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    public override void OnEpisodeBegin()
    {
        OnEnvironmentReset?.Invoke();
        EnvironmentReset();
    }

    void EnvironmentReset()
    {
        this.initialAmmo = EnvironmentParameters.GetWithDefault("initialAmmo", 200f);
        this.Ammo = initialAmmo;
        this.Damage = EnvironmentParameters.GetWithDefault("shootDamage", 4f);
        this.score = 0f;
        this.MovementSpeed = EnvironmentParameters.GetWithDefault("movementSpeed", 1.6f);
        this.RotationSpeed = EnvironmentParameters.GetWithDefault("rotationSpeed", 100f);
        this.isRandomRespawn = EnvironmentParameters.GetWithDefault("randomRespawnTarget", 1f) == 1f ? true : false;
        Respawn();
        transform.parent.Find("RedBoard").GetComponent<TextMeshPro>().text = $"Ammo:{(Ammo/initialAmmo).ToString("0.000")}";
    }

    public void Respawn()
    {
        this.gameObject.SetActive(true);
        if(isRandomRespawn)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(-4.5f, 4.5f)) + transform.parent.transform.position;
                distanceToSpider = Vector3.Distance(randomPosition, spider.transform.position);
            } while (distanceToSpider < 3f);

            transform.position = randomPosition;
            return;
        }
        transform.position = new Vector3(0f, 0.6f, 0f) + transform.parent.transform.position;
    }

    private void OnCollisionEnter(Collision other)
    {
        // Touched target.
        if (other.gameObject.CompareTag("spiderAgent") || other.gameObject.CompareTag("spiderLeg"))
        {
            AddRewardWithScore(-1f);
            gameObject.SetActive(false);
            spider.ReachedTarget();
            EndEpisode();
        }
    }

    public void ConfirmedKill()
    {
        AddRewardWithScore(100f);
        EndEpisode();
    }
}
