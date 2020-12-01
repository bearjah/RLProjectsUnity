using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArenaManager : MonoBehaviour
{
    public ShooterAgent[] agents;
    public int minDeadCount = 0;
    EnvironmentParameters EnvironmentParameters;

    void Awake()
    {
        EnvironmentParameters = Academy.Instance.EnvironmentParameters;
    }

    public int DeadAgentsCount(int team)
    {
        int remain = agents.Length/2;
        for (int i = 0; i < agents.Length; i++)
        {
            if (team == agents[i].Team && !(agents[i].isActiveAndEnabled))
            {
                remain--;
            }
        }
        // Debug.Log(remain);
        return remain;
    }

    public void RegisterDeath(int team)
    {
        if (DeadAgentsCount(team) <= minDeadCount)
        {
            foreach (var agent in agents)
            {
                agent.EndEpisodeDueKill();  //all agents need to be reset
            }
        }
    }

    public void ApplyTeamRewardAndPenalty(int team)
    {
        foreach (var agent in agents)
        {
            if (agent.Team != team)
            {
                agent.IncrReward(100f);
            }
            else
            {
                agent.IncrReward(-100f);
            }
        }
    }
}