﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAttackState : State
{
    private PlayerCombatController _controller;
    private AttackSO _attackObject;
    private float _currentTime;
    private List<EnemyBlackboard> _enemyHitList;

    public SimpleAttackState(PlayerCombatController controller)
    {
        _controller = controller;
        _currentTime = 0;
        _enemyHitList = new List<EnemyBlackboard>();
    }

    public override void Enter()
    {
        _controller.simpleAttackCount++;

        switch (_controller.simpleAttackCount)
        {
            case 1:
                _controller.animator.SetTrigger("Attack1");
                break;
            case 2:
                _controller.animator.SetTrigger("Attack2");
                break;
            case 3:
                _controller.animator.SetTrigger("Attack3");
                break;
        }
        _controller.animator.SetBool("isChargingAttack", true);

        if (_controller.simpleAttackCount < _controller.attacksToCombo)
        {
            _attackObject = _controller.basicAttack;
        }
        else
        {
            _attackObject = _controller.comboAttack;
            _controller.simpleAttackCount = 0;
        }
        
        _controller.SetMovementControllerCombatState(_attackObject.attackTime);
    }

    public override void Update()
    {
        if (_currentTime < _attackObject.attackTime)
        {
            _currentTime += Time.deltaTime;
        }
        else
        {
            ExitState();
        }
    }

    public override void Interact()
    {
        if (_controller.swordTrigger.hitObject.CompareTag("Enemy"))
        {
            var enemyBlackboard = _controller.swordTrigger.hitObject.GetComponent<EnemyBlackboard>();
            if (enemyBlackboard.CanBeDamaged())
            {
                if (_enemyHitList.Contains(enemyBlackboard)) return;
                enemyBlackboard.Hit((int)_attackObject.damage, _controller.transform.forward);

                if (_attackObject == _controller.comboAttack)
                {
                    _controller.simpleAttackCount = 0;
                    VibrationManager.Instance.Vibrate(VibrationManager.Presets.HARD_HIT);
                }
                else
                {
                    VibrationManager.Instance.Vibrate(VibrationManager.Presets.NORMAL_HIT);
                }

                _enemyHitList.Add(enemyBlackboard);
                _controller.SetTarget(enemyBlackboard);
                _controller.AnimStop();
                _controller.EnemyHitSound();
                return;
            }

            _controller.ArmoredHitSound();
            return;
        }

        if(_controller.swordTrigger.hitObject.CompareTag("Wood"))
        {
            _controller.WoodObjectHitSound();
            return;
        }

        if (_controller.swordTrigger.hitObject.CompareTag("Metal"))
        {
            _controller.MetalObjectHitSound();
            return;
        }
    }

    public override void ExitState()
    {
        _controller.swordTrigger.DisableTrigger();
        _controller.SetState(new IdleState(_controller));
        _controller.SetMovementControllerToMove();
    }
}
