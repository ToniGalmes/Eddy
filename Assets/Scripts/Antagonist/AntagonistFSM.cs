﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AntagonistFSM : MonoBehaviour
{
    public States currentState;

    private AntagonistBlackboard blackboard;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rigidbody;

    int currentPipe;

    float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        blackboard = GetComponent<AntagonistBlackboard>();
        rigidbody = GetComponent<Rigidbody>();

        currentState = States.INITIAL;
    }

    public void Enter()
    {
        currentPipe = 0;
        timer = 0;
        this.enabled = true;
    }

    public void Exit()
    {
        blackboard.attackCollider.enabled = false;
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.INITIAL:
                ChangeState(States.GUARDING);
                break;
            case States.GUARDING:

                navMeshAgent.SetDestination(blackboard.player.transform.position);

                break;

            case States.PERSECUTION:

                navMeshAgent.SetDestination(blackboard.player.transform.position);

                break;
            case States.STUNNED:
                timer += Time.deltaTime;

                if (timer >= blackboard.stunnedTime)
                {
                    ChangeState(States.PERSECUTION);
                }

                break;
            case States.APPEARING_DOWN:

                timer += Time.deltaTime;

                if (timer >= blackboard.pipeTime)
                {
                    ChangeState(States.PERSECUTION);
                }

                break;
            case States.WAITINGFORPIPE:
                break;
        }
    }

    public void ChangeState(States newState)
    {
        switch (currentState)
        {
            case States.INITIAL:
                break;
            case States.GUARDING:
                break;
            case States.PERSECUTION:
                blackboard.attackCollider.enabled = false;
                break;
            case States.STUNNED:
                navMeshAgent.enabled = true;
                rigidbody.isKinematic = true;
                timer = 0;
                break;
            case States.APPEARING_DOWN:
                navMeshAgent.enabled = true;
                rigidbody.isKinematic = true;
                timer = 0;
                break;
            case States.WAITINGFORPIPE:
                navMeshAgent.enabled = true;
                break;
            case States.WAITTOBEHEAD:
                navMeshAgent.enabled = true;
                blackboard.enemyCollider.SetActive(true);
                break;
        }

        switch (newState)
        {
            case States.INITIAL:
                break;
            case States.GUARDING:
                navMeshAgent.speed = blackboard.guardingSpeed;
                break;
            case States.WAITINGFORPIPE:
                navMeshAgent.enabled = false;
                transform.position = blackboard.pipes[currentPipe].transform.position + Vector3.up * 4;
                currentPipe++;
                break;
            case States.PERSECUTION:
                navMeshAgent.speed = blackboard.persecutionSpeed;
                blackboard.attackCollider.enabled = true;
                blackboard.firstObstacle.SetActive(false);
                blackboard.secondObstacle.SetActive(false);
                break;
            case States.STUNNED:
                navMeshAgent.enabled = false;
                rigidbody.isKinematic = false;
                rigidbody.AddForce(-transform.forward * blackboard.obstacleImpactForce, ForceMode.Impulse);
                break;
            case States.APPEARING_DOWN:
                navMeshAgent.enabled = false;
                rigidbody.isKinematic = false;
                rigidbody.AddForce(Vector3.down * blackboard.downPipeImpulse, ForceMode.Impulse);
                break;
            case States.WAITTOBEHEAD:
                navMeshAgent.enabled = false;
                blackboard.enemyCollider.SetActive(false);
                transform.position = blackboard.destinies[2].transform.position;
                transform.rotation = blackboard.destinies[2].transform.rotation;
                break;
        }

        currentState = newState;
    }

    public enum States
    {
        INITIAL, GUARDING, PERSECUTION, STUNNED, WAITINGFORPIPE, APPEARING_DOWN, APPEARING_ROTATING, WAITTOBEHEAD
    }
}