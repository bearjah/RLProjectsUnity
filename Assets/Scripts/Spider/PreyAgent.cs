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

public class PreyAgent : Agent
{
    public float score;
    private float distanceToSpider;
    private bool isRandomRespawn;
    [HideInInspector] public Rigidbody rbAgent;
    public SpiderAgent spider;
    [HideInInspector] public float MovementSpeed;
    [HideInInspector] public float RotationSpeed;
    [HideInInspector] EnvironmentParameters EnvironmentParameters;
    public event Action OnEnvironmentReset;

    private void FixedUpdate()
    {
        var survival = 1f;
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

    public void AddRewardWithScore(float reward)
    {
        AddReward(reward);
        score += reward;
        transform.parent.Find("PreyScore").GetComponent<TextMeshPro>().text = $"PreyScore:\n{score.ToString("0.000")}";
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int forwardMovement = Mathf.FloorToInt(vectorAction[0]);
        int rightMovement = Mathf.FloorToInt(vectorAction[1]);
        int rotation = Mathf.FloorToInt(vectorAction[2]);
        MoveAgent(forwardMovement, rightMovement, rotation);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0f;
        actionsOut[1] = 0f;
        actionsOut[2] = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 2f;
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
    }

    public override void OnEpisodeBegin()
    {
        OnEnvironmentReset?.Invoke();
        EnvironmentReset();
    }
    
    void EnvironmentReset()
    {
        this.score = 0f;
        Respawn();
        this.MovementSpeed = EnvironmentParameters.GetWithDefault("movementSpeed", 1.0f);
        this.RotationSpeed = EnvironmentParameters.GetWithDefault("rotationSpeed", 100f);
        this.isRandomRespawn = EnvironmentParameters.GetWithDefault("randomRespawnTarget", 0f) == 1f ? true : false;
    }

    public void Respawn()
    {
        this.gameObject.SetActive(true);
        if(isRandomRespawn)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = new Vector3(Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.x, 1f, Random.Range(-4.5f, 4.5f) + transform.parent.transform.position.z);
                distanceToSpider = Vector3.Distance(randomPosition, spider.transform.position);
            } while (distanceToSpider < 3f);

            transform.position = randomPosition;
            return;
        }
        transform.position = new Vector3(0f + transform.parent.transform.position.x, 1f, 0f + transform.parent.transform.position.z);
    }

    private void OnCollisionEnter(Collision other)
    {
        // Touched target.
        if (other.gameObject.CompareTag("spiderAgent"))
        {
            AddRewardWithScore(-1f);
            gameObject.SetActive(false);
            spider.ReachedTarget();
            EndEpisode();
        }
    }
}