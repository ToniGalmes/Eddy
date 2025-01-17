﻿using Steerings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ThrowHandsEnemyPassiveFSM))]
[RequireComponent(typeof(ArrivePlusAvoid))]

public class ThrowHandsEnemyAggressiveFSM : MonoBehaviour
{
    public enum States
    {
        INITIAL,
        ENEMY_PASSIVE,
        NOTICE,
        CHASE,
        ATTACK
    }

    private States currentState;

    private ThrowHandsEnemyBlackboard blackboard;
    private ThrowHandsEnemyPassiveFSM enemyPassiveFSM;
    private WanderPlusAvoid wanderPlusAvoid;
    private ArrivePlusAvoid arrivePlusAvoid;

    private float timer;
    private float timeAfterAttacks;

    private void Start()
    {
        blackboard = GetComponent<ThrowHandsEnemyBlackboard>();
        enemyPassiveFSM = GetComponent<ThrowHandsEnemyPassiveFSM>();
        wanderPlusAvoid = GetComponent<WanderPlusAvoid>();
        arrivePlusAvoid = GetComponent<ArrivePlusAvoid>();
    }

    private void OnEnable()
    {
        currentState = States.INITIAL;
    }

    private void OnDisable()
    {
        wanderPlusAvoid.enabled = false;
        enemyPassiveFSM.enabled = false;
        blackboard.animator.SetFloat("speed", 0);
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

                if (PlayerOnSight(transform.position, blackboard.detectionDistanceOnSight))
                {
                    ChangeState(States.NOTICE);
                    break;
                }

                if (Vector3.Distance(transform.position, blackboard.player.transform.position) < blackboard.detectionDistanceOffSight)
                {
                    if (Math.Abs(blackboard.player.transform.position.y - transform.position.y) < blackboard.maxVerticalDistance)
                    {
                        ChangeState(States.NOTICE);
                        break;
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
                blackboard.animator.SetFloat("speed", blackboard.ownKS.linearVelocity.magnitude);
                if (Vector3.Distance(transform.position, blackboard.player.transform.position) >= blackboard.playerOutOfRangeDistance || Math.Abs(blackboard.player.transform.position.y - transform.position.y) >= blackboard.maxVerticalDistance)
                {
                    ChangeState(States.ENEMY_PASSIVE);
                    break;
                }

                if(Vector3.Distance(transform.position, blackboard.player.transform.position) <= blackboard.attackRange)
                {
                    ChangeState(States.ATTACK);
                }

                CheckConstraints();

                break;
            case States.ATTACK:
                timeAfterAttacks -= Time.deltaTime;
                blackboard.animator.SetFloat("speed", 0);
                if (timeAfterAttacks <= 0)
                {
                    if (Vector3.Distance(transform.position, blackboard.player.transform.position) > blackboard.attackRange)
                    {
                        ChangeState(States.CHASE);
                        break;
                    }

                    ChangeState(States.ATTACK);
                    break;
                }       

                LookAtPlayer();

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
                enemyPassiveFSM.enabled = false;
                break;
            case States.NOTICE:
                break;
            case States.CHASE:
                arrivePlusAvoid.enabled = false;
                blackboard.rb.velocity = blackboard.ownKS.linearVelocity;
                blackboard.rb.constraints = RigidbodyConstraints.FreezeRotation;
                break;
            case States.ATTACK:
                break;
        }

        switch (newState)
        {
            case States.INITIAL:
                break;
            case States.ENEMY_PASSIVE:
                enemyPassiveFSM.enabled = true;
                break;
            case States.NOTICE:
                blackboard.animator.SetTrigger("hasNoticed");
                blackboard.NoticeSound();
                break;
            case States.CHASE:
                blackboard.ownKS.maxSpeed = blackboard.chasingSpeed;
                arrivePlusAvoid.enabled = true;
                arrivePlusAvoid.target = blackboard.player.gameObject;
                break;
            case States.ATTACK:
                blackboard.animator.SetTrigger("hasAttacked");
                timeAfterAttacks = blackboard.timeAfterAttacks;
                break;
        }
        currentState = newState;
        blackboard.statesText.text = currentState.ToString();
    }

    private bool PlayerOnSight(Vector3 start, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(start, blackboard.player.transform.position - start, out hit, distance, blackboard.sightObstaclesLayers))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                if (Mathf.Acos(Vector3.Dot((blackboard.player.transform.position - transform.position).normalized, Vector3.forward)) <= blackboard.visionAngle)
                {
                    if (Math.Abs(blackboard.player.transform.position.y - transform.position.y) < blackboard.maxVerticalDistance)
                    {
                        return true;
                    }
                }
            }
            else if (UndetectableObstacle(hit, blackboard.scannerSphereCollider))
            {
                float remainingDistance = blackboard.detectionDistanceOnSight - Vector3.Distance(hit.collider.gameObject.transform.position, transform.position);

                if (remainingDistance > 0)
                {
                    if (PlayerOnSight(hit.collider.gameObject.transform.position, remainingDistance))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool UndetectableObstacle(RaycastHit hit, SphereCollider scanner)
    {
        return HideLayer(hit, scanner) || AppearLayer(hit, scanner);
    }

    private static bool HideLayer(RaycastHit hit, SphereCollider scanner)
    {
        return hit.collider.gameObject.layer == LayerMask.NameToLayer("Hide") && scanner.bounds.Contains(hit.point);
    }

    private static bool AppearLayer(RaycastHit hit, SphereCollider scanner)
    {
        return hit.collider.gameObject.layer == LayerMask.NameToLayer("Appear") && !scanner.bounds.Contains(hit.point);
    }

    public void Attack()
    {
        Collider[] colliders = Physics.OverlapSphere(blackboard.attackPoint.position, blackboard.damageZoneRadius);
        
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag.Equals("Player"))
            {              
                blackboard.player.GetComponent<PlayerController>().Hit((int)blackboard.attackPoints);
                break;
            }
        }

        blackboard.AttackSound();
    }

    private void LookAtPlayer()
    {
        transform.LookAt(blackboard.player.transform);

        Vector3 playerEulerAngles = transform.rotation.eulerAngles;
        playerEulerAngles.x = 0;
        playerEulerAngles.z = 0;

        transform.rotation = Quaternion.Euler(playerEulerAngles);
        blackboard.ownKS.orientation = playerEulerAngles.y;
    }

    private void CheckConstraints()
    {
        RaycastHit floorHit;
        if (Physics.Raycast(transform.position + Vector3.down * (blackboard.col.height / 2), Vector3.down, out floorHit, 0.1f))
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
