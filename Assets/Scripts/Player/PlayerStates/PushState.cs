﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushState : PlayerState
{
    private PlayerMovementController _controller;

    public PushState(PlayerMovementController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Debug.Log("Push State");
    }

    public override void Update()
    {
        var vector3D = PlayerUtils.RetargetVector(_controller.movementVector, _controller.cameraTransform, _controller.joystickDeadZone);
        _controller.RotateTowardsForward(vector3D);
        vector3D *= Mathf.Lerp(_controller.minSpeed, _controller.maxSpeed, _controller.movementVector.magnitude);
        
        if (_controller.moveObject && _controller.moveObject.canMove && _controller.inputMoveObject && !_controller.scannerSword.UsingScannerInHand() && vector3D.magnitude >= _controller.joystickDeadZone)
        {
            if (PlayerUtils.InputDirectionTolerance(_controller.moveObject.moveVector, _controller.moveObject.angleToAllowMovement, _controller.cameraTransform, _controller.movementVector) && _controller.moveObject.canPull)
            {
                _controller.characterController.Move(_controller.moveObject.moveVector * (_controller.moveObject.speedWhenMove * Time.deltaTime));
                _controller.moveObject.Pull();
            }

            if (PlayerUtils.InputDirectionTolerance(-_controller.moveObject.moveVector, _controller.moveObject.angleToAllowMovement, _controller.cameraTransform, _controller.movementVector) && _controller.moveObject.canPush)
            {
                _controller.characterController.Move(-_controller.moveObject.moveVector * (_controller.moveObject.speedWhenMove * Time.deltaTime));
                _controller.moveObject.Push();
            }

            if (!_controller.moveObject.moving)
            {
                if(_controller.moveObject.swordStabbed) _controller.scannerIntersect.DeleteIntersections();
                else _controller.scannerIntersect.CheckIntersections(_controller.moveObject.GetComponent<BoxCollider>());
                _controller.moveObject.moving = true;
            }
        }
        else if (_controller.moveObject && vector3D.magnitude < _controller.joystickDeadZone)
        {
            _controller.moveObject.moving = false;
        }
    }

    public override void ExitState()
    {
        
    }
}