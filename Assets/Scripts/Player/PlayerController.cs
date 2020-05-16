﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovementController _movementController;
    private PlayerCombatController _combatController;

    public int health;

    private void Awake()
    {
        _movementController = GetComponent<PlayerMovementController>();
        _combatController = GetComponent<PlayerCombatController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Provisional to trigger death state
        if (Input.GetKeyDown(KeyCode.K))
        {
            Hit(100);
        }
    }

    public void Hit(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            //Trigger Death Animation
            //
            SetDeadState();
            StartCoroutine(UIManager.Instance.ShowDeathMenu());
        }
        else
        {
            float number = UnityEngine.Random.Range(1, 4);
            _movementController.animator.SetTrigger("Hit" + number.ToString());
        }
    }

    private void SetDeadState()
    {
        _movementController.SetState(new DeadState(_movementController));
        _combatController.SetState(new CombatDeadState());
    }

    public void Spawn()
    {
        _movementController.Spawn();
        _movementController.SetState(new MoveState(_movementController));
        _combatController.SetState(new IdleState(_combatController));
    }
}
