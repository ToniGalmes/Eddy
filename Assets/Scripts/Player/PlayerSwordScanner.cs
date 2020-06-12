﻿using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.VFX;

public class PlayerSwordScanner : MonoBehaviour
{
    private InputActions input;
    private PlayerMovementController playerMovement;
    private PlayerSounds playerSounds;
    private SimulateParent _parenting;

    public float scannerRadius;
  
    public Transform floorDetectionPoint;
    public float hitObjectDistance;
    public LayerMask stabSwordLayers;
    public float swordBackTime;

    [Header("MOVE AWAY TO STAB")]
    public float moveAwayDistance;
    public float moveAwaySpeed;
    public float moveAwayTime;

    private Transform playerHand;
    private GameObject swordHolder;

    [HideInInspector] public bool activeScanner;

    private bool scannerInput;
    private bool swordUnlocked;
    private ScannerIntersectionManager _scannerIntersectionManager;
    private SwordProgressiveColliders _swordProgressiveColliders;
    private PlayerInsideVolume _playerInsideVolume;
    private SphereCollider _sphereCollider;

    private Vector3 swordInitPos;
    private Quaternion swordInitRot;

    private RaycastHit stabbingHit;
    private bool horizontalStab;
    private bool movingAwayToStab;
    private Vector3 moveAwayVector;

    private EventInstance scannerSoundEvent;

    [Header("VFX")]
    public VisualEffect[] swordVfx;
    public Material swordBasicMat;
    public Material swordActiveMat;

    private void Awake()
    {
        _sphereCollider = GetComponent<SphereCollider>();
        _swordProgressiveColliders = GetComponent<SwordProgressiveColliders>();
        _scannerIntersectionManager = GetComponent<ScannerIntersectionManager>();
        _playerInsideVolume = GameObject.Find("Player").GetComponent<PlayerInsideVolume>();
        _parenting = GetComponent<SimulateParent>();
    }

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovementController>();
        playerSounds = FindObjectOfType<PlayerSounds>();

        playerHand = transform.parent;
        swordInitPos = transform.localPosition;
        swordInitRot = transform.localRotation;

        swordUnlocked = false;
        activeScanner = false;
        scannerInput = false;
        movingAwayToStab = false;

        transform.GetChild(0).localScale *= scannerRadius * 2f;
        _sphereCollider.radius = scannerRadius;

        input = new InputActions();
        input.Enable();

        input.PlayerControls.Scanner.started += ctx => ScannerOnInput();
        input.PlayerControls.Scanner.canceled += ctx => ScannerOffInput();
    }

    void Update()
    {
        //Shortcut
        if (Input.GetKeyDown(KeyCode.Return) && !swordUnlocked)
        {
            UnlockSword();
        }
        //

        if (input.PlayerControls.Sword.triggered && CanStab() && swordUnlocked)
        {
            if (HoldingSword())
            {
                if (Physics.Raycast(playerMovement.transform.position, playerMovement.transform.forward, out stabbingHit, hitObjectDistance, stabSwordLayers))
                {
                    if (stabbingHit.collider.gameObject.layer == LayerMask.NameToLayer("Hide"))
                    {
                        if (!_sphereCollider.bounds.Contains(stabbingHit.point))
                        {
                            DistanceToStab(playerMovement.transform.position, stabbingHit.point);
                            horizontalStab = true;
                            Stab(stabbingHit.collider.gameObject);
                            return;
                        }
                    }
                    else if (stabbingHit.collider.gameObject.layer == LayerMask.NameToLayer("Appear"))
                    {
                        if (_sphereCollider.bounds.Contains(stabbingHit.point))
                        {
                            DistanceToStab(playerMovement.transform.position, stabbingHit.point);
                            horizontalStab = true;
                            Stab(stabbingHit.collider.gameObject);
                            return;
                        }
                    }
                    else
                    {
                        DistanceToStab(playerMovement.transform.position, stabbingHit.point);
                        horizontalStab = true;
                        Stab(stabbingHit.collider.gameObject);
                        return;
                    }                   
                }

                if(Physics.Raycast(floorDetectionPoint.position, -playerMovement.transform.up, out stabbingHit, playerMovement.characterController.height / 2 + 0.5f, stabSwordLayers))
                {
                    if (stabbingHit.collider.gameObject.layer == LayerMask.NameToLayer("Hide"))
                    {
                        if (!_sphereCollider.bounds.Contains(stabbingHit.point))
                        {
                            horizontalStab = false;
                            Stab(stabbingHit.collider.gameObject);
                        }
                    }
                    else if (stabbingHit.collider.gameObject.layer == LayerMask.NameToLayer("Appear"))
                    {
                        if (_sphereCollider.bounds.Contains(stabbingHit.point))
                        {
                            horizontalStab = false;
                            Stab(stabbingHit.collider.gameObject);
                        }
                    }
                    else
                    {
                        horizontalStab = false;
                        Stab(stabbingHit.collider.gameObject);
                    }
                }
            }
            else
            {
                SwordBack();
            }       
        }

        if (activeScanner)
        {
            if (!AudioManager.Instance.isPlaying(scannerSoundEvent))
            {
                if (AudioManager.Instance.ValidEvent(playerSounds.scannerActiveSoundPath))
                {
                    scannerSoundEvent = AudioManager.Instance.PlayEvent(playerSounds.scannerActiveSoundPath, transform);
                }
            }
        }
        else
        {
            scannerSoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }

        //animation
        if (HoldingSword()) playerMovement.animator.SetBool("isHoldingSword", true);
        else playerMovement.animator.SetBool("isHoldingSword", false);
    }

    private void LateUpdate()
    {
        if (movingAwayToStab)
        {
            playerMovement.characterController.Move(moveAwayVector * Time.deltaTime * moveAwaySpeed);
        }
    }

    private void DistanceToStab(Vector3 playerPos, Vector3 hitPointPos)
    {
        if(Vector3.Distance(playerPos, hitPointPos) < moveAwayDistance)
        {
            moveAwayVector = (playerPos - hitPointPos).normalized;
            StartCoroutine(MoveAway());
        }
    }

    private IEnumerator MoveAway()
    {
        movingAwayToStab = true;
        yield return new WaitForSeconds(moveAwayTime);
        movingAwayToStab = false;
    }

    private void ScannerOnInput()
    {
        scannerInput = true;
        if (HoldingSword() && CanUseScanner())
        {
            if (!activeScanner)
            {
                ScannerOn();
            }
        }
    }

    private void ScannerOffInput()
    {
        scannerInput = false;
        if (activeScanner && HoldingSword() && !playerMovement.inputMoveObject)
        {
            ScannerOff();    
        }
    }

    public void ScannerOn()
    {
        if (!_playerInsideVolume.CanActivateScanner()) return;

        EnemyBlackboard[] enemies = GameObject.FindObjectsOfType<EnemyBlackboard>();

        foreach (EnemyBlackboard enemy in enemies)
        {
            enemy.EnemyInVolume(true);
        }

        activeScanner = true;
        if (UIHelperController.Instance.actionToComplete == UIHelperController.HelperAction.Scanner) UIHelperController.Instance.DisableHelper();
        transform.GetChild(0).gameObject.SetActive(true);
        foreach (VisualEffect vfx in swordVfx)
        {
            vfx.Play();
        }
        GetComponent<MeshRenderer>().material = swordActiveMat;
        _sphereCollider.enabled = true;
        _swordProgressiveColliders.EnableSword();

        playerMovement.animator.SetBool("isUsingScanner", true);

        if (AudioManager.Instance.ValidEvent(playerSounds.scannerOnSoundPath))
        {
            AudioManager.Instance.PlayOneShotSound(playerSounds.scannerOnSoundPath, transform);
        }
    }

    public void ScannerOff()
    {
        if (!_playerInsideVolume.CanDisableScanner()) return;

        EnemyBlackboard[] enemies = GameObject.FindObjectsOfType<EnemyBlackboard>();

        foreach (EnemyBlackboard enemy in enemies)
        {
            enemy.EnemyInVolume(false);
        }

        activeScanner = false;

        transform.GetChild(0).gameObject.SetActive(false);
        foreach (VisualEffect vfx in swordVfx)
        {
            vfx.Stop();
        }
        GetComponent<MeshRenderer>().material = swordBasicMat;
        _sphereCollider.enabled = false;
        _swordProgressiveColliders.DisableSword();
        _scannerIntersectionManager.DeleteIntersections();

        playerMovement.animator.SetBool("isUsingScanner", false);

        if (AudioManager.Instance.ValidEvent(playerSounds.scannerOffSoundPath))
        {
            AudioManager.Instance.PlayOneShotSound(playerSounds.scannerOffSoundPath, transform);
        }
    }

    private void Stab(GameObject obj)
    {
        if (horizontalStab) playerMovement.animator.SetTrigger("NailForward");
        else playerMovement.animator.SetTrigger("NailDown");
        swordHolder = obj;
        playerMovement.SetState(new StabSwordState(playerMovement));
    }

    public void FinishStab()
    {
        if (CanFinallyStab())
        {
            transform.parent = null;

            if (swordHolder.CompareTag("MoveObject"))
            {
                var moveObject = swordHolder.GetComponent<PushPullObject>();
                if (moveObject) moveObject.swordStabbed = true;
                else
                {
                    moveObject = swordHolder.transform.parent.gameObject.GetComponent<PushPullObject>();
                    moveObject.swordStabbed = true;
                }

                transform.parent = null;
                _parenting.InjectTransform(moveObject.transform);
            }

            if (swordHolder.CompareTag("CheckPoint"))
            {
                CheckPoint c = swordHolder.GetComponent<CheckPoint>();
                c.Activate();

                if (AudioManager.Instance.ValidEvent(playerSounds.checkpointSoundPath))
                {
                    AudioManager.Instance.PlayOneShotSound(playerSounds.checkpointSoundPath, transform);
                }
            }

            if (!swordHolder.CompareTag("MoveObject")) transform.parent = swordHolder.transform;

            if (swordHolder.GetComponent<Switchable>() != null)
            {
                swordHolder.GetComponent<Switchable>().SwitchOn();

                if (AudioManager.Instance.ValidEvent(playerSounds.switchSoundPath))
                {
                    AudioManager.Instance.PlayOneShotSound(playerSounds.switchSoundPath, transform);
                }
            }

            if (activeScanner) _scannerIntersectionManager.CheckIntersections();

            if (AudioManager.Instance.ValidEvent(playerSounds.stabSoundPath))
            {
                AudioManager.Instance.PlayOneShotSound(playerSounds.stabSoundPath, transform);
            }
        } 
    }

    private bool CanFinallyStab()
    {
        if(swordHolder.layer == LayerMask.NameToLayer("Hide"))
        {
            if (!_sphereCollider.bounds.Contains(stabbingHit.point))
            
            return true;
        }
        else if (swordHolder.layer == LayerMask.NameToLayer("Appear"))
        {
            if (_sphereCollider.bounds.Contains(stabbingHit.point))
            {
                return true;
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    public void SwordBack()
    {
        if (!_playerInsideVolume.CanDisableScanner()) return;

        StartCoroutine(WaitToRecoverSword());
    }

    public void SwordRecovered()
    {
        if (!_playerInsideVolume.CanDisableScanner()) return;

        EnemyBlackboard[] enemies = GameObject.FindObjectsOfType<EnemyBlackboard>();

        foreach (EnemyBlackboard enemy in enemies)
        {
            enemy.EnemyInVolume(false);
        }

        if (swordHolder.GetComponent<Switchable>() != null)
        {
            swordHolder.GetComponent<Switchable>().SwitchOff();
        }

        if (!scannerInput && activeScanner) ScannerOff();

        transform.parent = null;
        _parenting.UnParent();

        transform.parent = playerHand;
        transform.parent = playerHand;

        transform.localPosition = swordInitPos;
        transform.localRotation = swordInitRot;

        if (AudioManager.Instance.ValidEvent(playerSounds.swordBackSoundPath))
        {
            AudioManager.Instance.PlayOneShotSound(playerSounds.swordBackSoundPath, transform);
        }
    }

    private IEnumerator WaitToRecoverSword()
    {
        yield return new WaitForSeconds(swordBackTime);
        SwordRecovered();
    }

    public void UnlockSword()
    {
        GetComponent<MeshRenderer>().enabled = true;
        swordUnlocked = true;
        Debug.Log("Sword Unlocked");
    }

    private bool CanUseScanner()
    {
        return playerMovement.GetState().GetType() != typeof(EdgeState)
            && playerMovement.GetState().GetType() != typeof(PushState);
    }


    private bool CanStab()
    {
        return playerMovement.GetState().GetType() == typeof(MoveState)
            || (playerMovement.GetState().GetType() == typeof(JumpState) && transform.parent != playerHand);
    }

    public bool HoldingSword()
    {
        return transform.parent == playerHand && SwordUnlocked();
    }

    public bool SwordUnlocked()
    {
        return swordUnlocked;
    }

    public void LockSword()
    {
        swordUnlocked = false;
    }

    public bool UsingScannerInHand()
    {
        return activeScanner && HoldingSword();
    } 
}
