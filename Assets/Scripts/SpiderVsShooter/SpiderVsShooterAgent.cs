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

public class SpiderVsShooterAgent : Agent
{
    [HideInInspector] public float score;
    [HideInInspector] public Rigidbody rbAgent;
    public JointController[] Joints;
    public ShooterVsSpiderAgent target;
    // public Target target;
    private float inputDirectionAverage = 0f;
    private float deltaUp;
    private float episodeTime;
    private float maxDistance = Mathf.Sqrt(10 * 10 + 10 * 10);
    [HideInInspector] public float Health;
    [HideInInspector] public float initialHealth;

    [HideInInspector] EnvironmentParameters EnvironmentParameters;
    public event Action OnEnvironmentReset;

    private void FixedUpdate()
    {
        episodeTime++;
        Vector3 targetDir = target.transform.position - transform.position;
        targetDir.y = 0;
        var targetUp = Vector3.up;
        deltaUp  = (targetUp - this.transform.up.normalized).magnitude;
        // normalize to [0, 1]
        deltaUp /= 2f;

        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);

        float distanceReward = 1f - Mathf.Pow(1.5f * (distanceToTarget)/maxDistance, 2f/5f);
        float keepUpReward = Mathf.Pow(1f - Mathf.Max(deltaUp, 0.05f), 1f/Mathf.Max(inputDirectionAverage, 0.4f));
        AddRewardWithScore(distanceReward * keepUpReward / MaxStep);
        float speedTowardPrey = Vector3.Project(rbAgent.velocity, targetDir.normalized).magnitude;
        float speedDirection = Vector3.Dot(rbAgent.velocity.normalized, targetDir.normalized);
        transform.parent.Find("RewardSignals").GetComponent<TextMeshPro>().text = $"Distance: {distanceToTarget.ToString("0.000")}\nDeltaUp: {deltaUp.ToString("0.000")}\nAvgInputDir: {(inputDirectionAverage).ToString("0.000")}\nSpeed: {(speedTowardPrey * speedDirection).ToString("0.000")}\nEpisodeTime: {(episodeTime/MaxStep).ToString("0.000")}";
    }

    private void Awake()
    {
        rbAgent = GetComponent<Rigidbody>();
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    public static float ActionToDirection(int action)
    {
        return (float) action == 2 ? -1f : action;
    }

    public void MoveAgent(float[] directions)
    {
        int numLegs = 8; // TODO: derive this somehow
        for (int i = 0; i < numLegs; ++i)
        {
            Joints[i].RotateJoint(directions[i], 0f);
        }
        for (int i = numLegs; i < numLegs * 2; ++i)
        {
            Joints[i].RotateJoint(directions[i], directions[i + numLegs]);
        }
    }

    public void AddRewardWithScore(float reward)
    {
        AddReward(reward);
        score += reward;
        transform.parent.Find("Score").GetComponent<TextMeshPro>().text = $"SpiderScore:\n{score.ToString("0.000")}";
    }

    public void ReachedTarget()
    {
        AddRewardWithScore(10f);
        EndEpisode();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        Quaternion bodyRotation = transform.rotation;
        Vector3 bodyRotationNormalized = bodyRotation.eulerAngles / 180.0f - Vector3.one;
        sensor.AddObservation(bodyRotationNormalized);
        sensor.AddObservation(rbAgent.velocity);

        float maxRaycastDist = 2f;
        RaycastHit hit;
        GameObject bodyTouchingPoint;
        Transform touchingPoint;
        bodyTouchingPoint = transform.Find("BodyTouchPoint").gameObject;
        if (Physics.Raycast(bodyTouchingPoint.transform.position, Vector3.down, out hit, maxRaycastDist))
        {
            sensor.AddObservation(hit.distance / maxRaycastDist);
        }
        else sensor.AddObservation(1f);

        foreach (var bodyPart in Joints)
        {
            Quaternion LegRotation = bodyPart.transform.rotation;
            Vector3 LegRotationNormalized = LegRotation.eulerAngles / 180.0f - Vector3.one;
            sensor.AddObservation(LegRotationNormalized);
            // sensor.AddObservation(bodyPart.joint.targetRotation);
            // sensor.AddObservation(bodyPart.transform.position);
            if (bodyPart.bodyPartName == "Tibia")
            {
                touchingPoint = bodyPart.gameObject.transform.GetChild(0);
                if (Physics.Raycast(touchingPoint.transform.position, Vector3.down, out hit, maxRaycastDist))
                {
                    sensor.AddObservation(hit.distance / maxRaycastDist);
                }
                else sensor.AddObservation(1);   
            }
        }
        // Debug.Log("body rotation: " + transform.rotation);
        // Debug.Log("Tibia rotation: " + Joints[0].transform.rotation);
        // Debug.Log("Tibia target rotation: " + Joints[0].joint.targetRotation);
        // Debug.Log("Femur rotation: " + Joints[8].transform.rotation);
        // Debug.Log("Femur target rotation: " + Joints[8].joint.targetRotation);
        // if (Physics.Raycast(transform.position, Vector3.down, out hit, maxRaycastDist))
        // {
        //     Debug.Log("body height: " + hit.distance / maxRaycastDist);        
        // }
        // else
        // {
        //     Debug.Log("body height: 1");
        // }
        // touchingPoint = Joints[0].gameObject.transform.GetChild(0);
        // if (Physics.Raycast(touchingPoint.transform.position, Vector3.down, out hit, maxRaycastDist))
        // {
        //     Debug.Log("Touch point height:" + hit.distance / maxRaycastDist);    
        // }
        // else
        // {
        //     Debug.Log("Touch point height: 1");
        // }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        int numActions = vectorAction.Length;
        float[] directions = new float[numActions];
        inputDirectionAverage = 0f;
        for (int i = 0; i < numActions; ++i)
        {
            directions[i] = ActionToDirection(Mathf.FloorToInt(vectorAction[i]));
            inputDirectionAverage += Math.Abs(directions[i]);
        }
        inputDirectionAverage /= numActions;
        MoveAgent(directions);
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0f;
        actionsOut[1] = 0f;
        actionsOut[2] = 0f;
        actionsOut[3] = 0f;
        actionsOut[4] = 0f;
        actionsOut[5] = 0f;
        actionsOut[6] = 0f;
        actionsOut[7] = 0f;
        actionsOut[8] = 0f;
        actionsOut[9] = 0f;
        actionsOut[10] = 0f;
        actionsOut[11] = 0f;
        actionsOut[12] = 0f;
        actionsOut[13] = 0f;
        actionsOut[14] = 0f;
        actionsOut[15] = 0f;
        actionsOut[16] = 0f;
        actionsOut[17] = 0f;
        actionsOut[18] = 0f;
        actionsOut[19] = 0f;
        actionsOut[20] = 0f;
        actionsOut[21] = 0f;
        actionsOut[22] = 0f;
        actionsOut[23] = 0f;
        //movement
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 1f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            actionsOut[0] = 2f;
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[8] = 1f;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            actionsOut[8] = 2f;
        }
                
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[16] = 1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            actionsOut[16] = 2f;
        }
    }

    public override void OnEpisodeBegin()
    {
        OnEnvironmentReset?.Invoke();
        EnvironmentReset();
    }

    void EnvironmentReset()
    {
        this.initialHealth = EnvironmentParameters.GetWithDefault("initialHealth", 100f);
        Health = initialHealth;
        this.score = 0f;
        Respawn();
        episodeTime = 0f;
    }

    public void Respawn()
    {
        Vector3 randomPosition;
        float distanceToTarget;
        do
        {
            randomPosition = new Vector3(Random.Range(-3.3f, 3.3f), 1f, Random.Range(-3.3f, 3.3f)) + transform.parent.transform.position;
            distanceToTarget = Vector3.Distance(randomPosition, target.transform.position);
        } while (distanceToTarget < 3f);
        transform.position = randomPosition;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        this.gameObject.SetActive(true);
    }

    public void GetShot(float damage)
    {
        Debug.Log("SpiderGotShot");
        ApplyDamage(damage);
    }
    
    private void ApplyDamage(float damage)
    {
        this.Health -= damage;
        if (this.Health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        AddRewardWithScore(-4f);
        gameObject.SetActive(false);
        EndEpisode();
        target.ConfirmedKill();
    }
}