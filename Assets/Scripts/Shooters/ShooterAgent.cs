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

public class ShooterAgent : Agent
{
    public float score;
    private float MovementSpeed;
    private float RotationSpeed;
    [HideInInspector] public float Health;
    [HideInInspector] public float Ammo;
    public string otherTeamLayer;
    public int Team;
    [HideInInspector] public float episodeTime;
    [HideInInspector] public float MissPenalty = -0.05f;
    [HideInInspector] public float GotShotPenalty = -0.5f;
    [HideInInspector] public float MaxRangeReward;
    [HideInInspector] public float initialHealth;
    [HideInInspector] public float initialAmmo;
    [HideInInspector] public float Damage;
    public ArenaManager arenaManager;
    public Projectile projectile;
    public Transform shootingPoint;
    [HideInInspector] public Rigidbody rbAgent;
    [HideInInspector] EnvironmentParameters EnvironmentParameters;
    // public event Action OnEnvironmentReset;

    private void FixedUpdate()
    {
        // IncrReward(-1f/MaxStep);
        episodeTime++;
        if(Team == 0)
        {
            transform.parent.Find("BlueBoard").GetComponent<TextMeshPro>().text = $"Score:{score.ToString("0.000")}\nHealth:{(Health/initialHealth).ToString("0.000")}\nAmmo:{(Ammo/initialAmmo).ToString("0.000")}";    
        }
        else
        {
            transform.parent.Find("RedBoard").GetComponent<TextMeshPro>().text = $"Time:{(episodeTime/MaxStep).ToString("0.000")}\n\nScore:{score.ToString("0.000")}\nHealth:{(Health/initialHealth).ToString("0.000")}\nAmmo:{(Ammo/initialAmmo).ToString("0.000")}";
        }
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

        spawnedProjectile.Initialize(this, direction);
    }
    
    public void GetShot(float damage)
    {
        ApplyDamage(damage);
    }
    
    private void ApplyDamage(float damage)
    {
        this.Health -= damage;
        if (this.Health <= 0f)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // arenaManager.ApplyTeamRewardAndPenalty(Team);
        // gameObject.SetActive(false);
        arenaManager.RegisterDeath(Team);
        // EndEpisode();
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
        sensor.AddObservation(this.Health/EnvironmentParameters.GetWithDefault("initialHealth", initialHealth));
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
        // OnEnvironmentReset?.Invoke();
        EnvironmentReset();
        Respawn();
    }

    public void IncrReward(float increment)
    {
        AddReward(increment);
        score += increment;
    }

    void EnvironmentReset()
    {
        this.score = 0f;
        this.episodeTime = 0;
        this.initialHealth = EnvironmentParameters.GetWithDefault("initialHealth", 100f);
        this.initialAmmo = EnvironmentParameters.GetWithDefault("initialAmmo", 25f);        
        this.Health = initialHealth;
        this.Ammo = initialAmmo;
        this.Damage = EnvironmentParameters.GetWithDefault("shootDamage", 2f);
        this.MovementSpeed = EnvironmentParameters.GetWithDefault("movementSpeed", 3f);
        this.RotationSpeed = EnvironmentParameters.GetWithDefault("rotationSpeed", 130f);
    }

    public void EndEpisodeDueKill()
    {
        EndEpisode();
    }

    public void Respawn()
    {
        // this.gameObject.SetActive(true);
        if (this.Team == 0)
        {
            transform.position = new Vector3(Random.Range(-4f, -2f), 0.5f, Random.Range(-4f, 4f)) + transform.parent.transform.position;    
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else
        {
            transform.position = new Vector3(Random.Range(2f, 4f), 0.5f, Random.Range(-4f, 4f)) + transform.parent.transform.position;
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);   
        }
    }
}
