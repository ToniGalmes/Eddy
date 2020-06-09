﻿using Steerings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Seek))]
[RequireComponent(typeof(WanderPlusAvoid))]
[RequireComponent(typeof(ChargingEnemyPassiveFSM))]

public class ChargingEnemyAggressiveFSM : MonoBehaviour
{
    public enum States
    {
        INITIAL,
        ENEMY_PASSIVE,
        NOTICE,
        CHASE
    }

    private States currentState;

    private ChargingEnemyBlackboard blackboard;
    private WanderPlusAvoid wanderPlusAvoid;
    private Seek seek;
    private ChargingEnemyPassiveFSM enemyPassiveFsm;
    private CapsuleCollider enemyCol;

    private float timer;

    private void Start()
    {
        blackboard = GetComponent<ChargingEnemyBlackboard>();
        wanderPlusAvoid = GetComponent<WanderPlusAvoid>();
        seek = GetComponent<Seek>();
        enemyPassiveFsm = GetComponent<ChargingEnemyPassiveFSM>();
        enemyCol = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        currentState = States.INITIAL;
    }

    private void OnDisable()
    {
        blackboard.animator.SetBool("isCharging", false);
        wanderPlusAvoid.enabled = false;
        seek.enabled = false;
        enemyPassiveFsm.enabled = false;
        blackboard.attackCollider.enabled = false;

        enemyCol.height = 2.0f;
        enemyCol.center = Vector3.zero;

        timer = 0;

        blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        switch (currentState)
        {
            case States.INITIAL:
                ChangeState(States.ENEMY_PASSIVE);
                break;
            case States.ENEMY_PASSIVE:

                RaycastHit hit;
                if (Physics.Raycast(transform.position, blackboard.player.transform.position - transform.position, out hit, blackboard.detectionDistanceOnSight, blackboard.sightObstaclesLayers))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        if (Mathf.Acos(Vector3.Dot((blackboard.player.transform.position - transform.position).normalized, Vector3.forward)) <= blackboard.visionAngle)
                        {
                            if (Math.Abs(blackboard.player.transform.position.y - transform.position.y) < blackboard.maxVerticalDistance)
                            {
                                ChangeState(States.NOTICE);
                                break;
                            }
                        }
                    }
                }

                if (Vector3.Distance(transform.position, blackboard.player.transform.position) < blackboard.detectionDistanceOffSight)
                {
                    if (Math.Abs(blackboard.player.transform.position.y - transform.position.y) < blackboard.maxVerticalDistance)
                    {
                        ChangeState(States.NOTICE);
                    }
                }
                break;
            case States.NOTICE:

                if (timer >= blackboard.timeInNotice)
                {
                    ChangeState(States.CHASE);
                    break;
                }

                LookAtPlayer();
                timer += Time.deltaTime;

                break;
            case States.CHASE:

                if (Vector3.Distance(transform.position, blackboard.player.transform.position) >= blackboard.playerOutOfRangeDistance || Math.Abs(blackboard.player.transform.position.y - transform.position.y) >= blackboard.maxVerticalDistance)
                {
                    ChangeState(States.ENEMY_PASSIVE);
                    break;
                }

                CheckConstraints();

                break;
        }
    }

    private void ChangeState(States newState)
    {
        switch (currentState)
        {
            case States.INITIAL:
                break;
            case States.ENEMY_PASSIVE:
                enemyPassiveFsm.enabled = false;
                break;
            case States.NOTICE:
                break;
            case States.CHASE:
                blackboard.animator.SetBool("isCharging", false);
                enemyCol.height = 2.0f;
                enemyCol.center = Vector3.zero;
                blackboard.attackCollider.enabled = false;
                seek.enabled = false;
                blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation;
                break;
        }

        switch (newState)
        {
            case States.INITIAL:
                break;
            case States.ENEMY_PASSIVE:
                enemyPassiveFsm.enabled = true;
                break;
            case States.NOTICE:
                blackboard.animator.SetTrigger("hasNoticed");
                blackboard.NoticeSound();
                break;
            case States.CHASE:
                blackboard.animator.SetBool("isCharging", true);
                enemyCol.height = blackboard.enemyColliderChaseHeight;
                enemyCol.center = enemyCol.center - new Vector3(0, (2.0f - blackboard.enemyColliderChaseHeight) / 2, 0);
                blackboard.attackCollider.enabled = true;
                blackboard.ownKS.maxSpeed = blackboard.chasingSpeed;
                seek.enabled = true;
                seek.target = blackboard.player.gameObject;
                break;
        }
        currentState = newState;
        blackboard.statesText.text = currentState.ToString();
    }

    public void HitHandler(GameObject objectHit)
    {
        HornedEnemyWall enemyWall = objectHit.GetComponent<HornedEnemyWall>();

        if (enemyWall == null)
        {
            if (objectHit.tag == "Player")
            {
                blackboard.player.GetComponent<PlayerController>().Hit((int)blackboard.attackPoints);
            }

            blackboard.AttackSound();
            blackboard.stunned = true;
        }
    }

    private void LookAtPlayer()
    {
        transform.LookAt(blackboard.player.transform);

        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.x = 0;
        eulerAngles.z = 0;

        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void CheckConstraints()
    {
        RaycastHit floorHit;
        if (Physics.Raycast(transform.position + Vector3.down * (blackboard.col.height / 2), Vector3.down, out floorHit, 0.5f))
        {
            if (floorHit.collider.gameObject.layer != LayerMask.NameToLayer("TriggerDetection")
            && floorHit.collider.gameObject.layer != LayerMask.NameToLayer("ScannerLayer")
            && floorHit.collider.gameObject.layer != LayerMask.NameToLayer("EnemyLimits")
            && floorHit.collider.gameObject.layer != LayerMask.NameToLayer("VoidCollider"))
            {
                blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
        else
        {
            blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
