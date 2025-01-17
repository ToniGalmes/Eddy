﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRelativeToScanner : MonoBehaviour
{
    [SerializeField] private Transform topRightCheck;
    [SerializeField] private Transform topLeftCheck;
    [SerializeField] private Transform bottomRightCheck;
    [SerializeField] private Transform bottomLeftCheck;
    [SerializeField] private SphereCollider _swordSphereCollider;
    
    private List<GameObject> _playerGameObjects = new List<GameObject>();
    private PlayerSwordScanner _playerSwordScanner;
    private void Awake()
    {
        var children = transform.root.GetComponentsInChildren<Transform>();
        _playerSwordScanner = FindObjectOfType<PlayerSwordScanner>();
        
        foreach (var c in children)
        {
            if(c.gameObject.layer == LayerMask.NameToLayer("Player")) _playerGameObjects.Add(c.gameObject);
        }
    }

    private void Update()
    {
        if (BottomIsInScanner() && transform.root.gameObject.layer != LayerMask.NameToLayer("playerinScanner") && _playerSwordScanner.activeScanner)
        {
            SetPlayerObjectsToLayer(LayerMask.NameToLayer("playerinScanner"));
            return;
        }

        if (BottomIsInScanner() && !TopIsInScanner() && transform.root.gameObject.layer == LayerMask.NameToLayer("playerinScanner")) return;
        
        if (TopIsInScanner() && transform.root.gameObject.layer != LayerMask.NameToLayer("playerinScanner") && _playerSwordScanner.activeScanner)
        {
            SetPlayerObjectsToLayer(LayerMask.NameToLayer("playerinScanner"));
        }
        else if (!TopIsInScanner() && transform.root.gameObject.layer != LayerMask.NameToLayer("Player") || !_playerSwordScanner.activeScanner)
        {
            SetPlayerObjectsToPlayerLayer();
        }
    }

    private void SetPlayerObjectsToLayer(int layer)
    {
        foreach (var go in _playerGameObjects)
        {
            go.layer = layer;
        }
    }
    
    private void SetPlayerObjectsToPlayerLayer()
    {
        SetPlayerObjectsToLayer(LayerMask.NameToLayer("Player"));
    }

    private bool TopIsInScanner()
    {
        return Vector3.Distance(_swordSphereCollider.transform.position, topRightCheck.position) <= _swordSphereCollider.radius &&
               Vector3.Distance(_swordSphereCollider.transform.position, topLeftCheck.position) <= _swordSphereCollider.radius;
    }

    private bool BottomIsInScanner()
    {
        return Vector3.Distance(_swordSphereCollider.transform.position, bottomRightCheck.position) <= _swordSphereCollider.radius &&
               Vector3.Distance(_swordSphereCollider.transform.position, bottomLeftCheck.position) <= _swordSphereCollider.radius;
    }
}
