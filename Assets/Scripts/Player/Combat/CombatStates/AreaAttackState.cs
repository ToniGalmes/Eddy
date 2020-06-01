﻿using UnityEngine;

public class AreaAttackState : State
{
    private PlayerCombatController _controller;
    private AttackSO _attackObject;
    private float _currentTime;
    private float _damageMultiplier;

    public AreaAttackState(PlayerCombatController controller, float damageMultiplier)
    {
        _controller = controller;
        _damageMultiplier = damageMultiplier;
        _currentTime = 0;
    }

    public override void Enter()
    {
        Debug.Log("Area Attack");
        //_controller.swordTrigger.EnableTrigger();
        _attackObject = _controller.areaAttack;

        _controller.SetMovementControllerCombatState(_attackObject.attackTime);
        _controller.animator.SetTrigger("AreaAttack");
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
        if (_controller.swordTrigger.hitObject.tag == "Enemy")
        {
            if (_controller.swordTrigger.hitObject.GetComponent<EnemyBlackboard>().CanBeDamaged())
            {
                _controller.swordTrigger.hitObject.GetComponent<EnemyBlackboard>().healthPoints -= Mathf.Round(_attackObject.damage * _damageMultiplier);
                Debug.Log("Enemy damaged: " + _attackObject.damage);

                if (_damageMultiplier == 1) _controller.swordTrigger.hitObject.GetComponent<EnemyBlackboard>().stunned = true;

                _controller.EnemyHitSound();
                return;
            }

            _controller.ArmoredHitSound();
            return;
        }

        if (_controller.swordTrigger.hitObject.tag == "Wood")
        {
            _controller.WoodObjectHitSound();
            return;
        }

        if (_controller.swordTrigger.hitObject.tag == "Metal")
        {
            _controller.MetalObjectHitSound();
            return;
        }
    }

    public override void ExitState()
    {
        _controller.simpleAttackCount = 0;
        _controller.swordTrigger.DisableTrigger();
        _controller.SetMovementControllerToMove();
        _controller.SetState(new IdleState(_controller));
    }
}
