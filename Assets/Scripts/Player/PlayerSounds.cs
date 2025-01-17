﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("Basics")]
    [FMODUnity.EventRef] public string stoneStepSoundPath;
    [FMODUnity.EventRef] public string woodStepSoundPath;
    [FMODUnity.EventRef] public string damageReceivedSoundPath;
    [FMODUnity.EventRef] public string deathSoundPath;
    [FMODUnity.EventRef] public string jumpSoundPath;
    [FMODUnity.EventRef] public string landSoundPath;

    [Header("Combat")]
    [FMODUnity.EventRef] public string attackSoundPath_1;
    [FMODUnity.EventRef] public string attackSoundPath_2;
    [FMODUnity.EventRef] public string comboAttackSoundPath;
    [FMODUnity.EventRef] public string areaAttackSoundPath;
    [FMODUnity.EventRef] public string areaAttackChargingSoundPath_1;
    [FMODUnity.EventRef] public string areaAttackChargingSoundPath_2;
    [FMODUnity.EventRef] public string areaAttackChargedSoundPath;
    [FMODUnity.EventRef] public string enemyHitSoundPath;
    [FMODUnity.EventRef] public string enemyArmoredHitSoundPath;
    [FMODUnity.EventRef] public string woodObjectHitSoundPath;
    [FMODUnity.EventRef] public string metalObjectHitSoundPath;

    [Header("Sword / Scanner")]
    [FMODUnity.EventRef] public string scannerOnSoundPath;
    [FMODUnity.EventRef] public string scannerOffSoundPath;
    [FMODUnity.EventRef] public string scannerActiveSoundPath;
    [FMODUnity.EventRef] public string stabSoundPath;
    [FMODUnity.EventRef] public string swordBackSoundPath;
    [FMODUnity.EventRef] public string checkpointSoundPath;

    [Header("Environment")]
    [FMODUnity.EventRef] public string draggableObjectSoundPath;
    [FMODUnity.EventRef] public string woodenPlanksSoundPath;
    [FMODUnity.EventRef] public string creepyShadowLaugh_1;
    [FMODUnity.EventRef] public string creepyShadowLaugh_2;
    [FMODUnity.EventRef] public string creepyShadowCry;

    [Header("Crowd")]
    [FMODUnity.EventRef] public string crowdSoundPath;
    [FMODUnity.EventRef] public string applauseSoundPath;
    [FMODUnity.EventRef] public string cheersSoundPath; 
    [FMODUnity.EventRef] public string confettiPopSoundPath;
    [FMODUnity.EventRef] public string finalRoundSoundPath;

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (AudioManager.Instance.ValidEvent(woodStepSoundPath))
            {
                AudioManager.Instance.PlayOneShotSound(woodStepSoundPath, transform);
            }
        }
    }*/
}
