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
        if (Input.GetKeyDown(KeyCode.D))
        {
            Hit(100);
        }
        //Provisional to Respawn while UI is not finished
        if (Input.GetKeyDown(KeyCode.S))
        {
            Spawn();
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
            UIManager.Instance.ShowDeathMenu(); //This must be called in the death animation
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
