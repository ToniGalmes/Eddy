﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    [HideInInspector] public Vector3 respawnPos;
    [HideInInspector] public EnemySpawnManager enemySpawnManager;
    [HideInInspector] public List<GameObject> nonRespawnableEnemies;
    [HideInInspector] public int checkpointSceneIndex = 4;
    [HideInInspector] public AdditiveSceneManager asm;
    private PlayerController _playerController;

    private void Start()
    {
        var player = GameObject.Find("Player");
        respawnPos = player.transform.position;
        checkpointSceneIndex = 4;
        _playerController = player.GetComponent<PlayerController>();
        enemySpawnManager = new EnemySpawnManager();
        nonRespawnableEnemies = new List<GameObject>();
    }

    public void Respawn()
    {
        _playerController.Spawn();
        ResetWaveController();
        ResetEnemies();
        GoToScene();
        RemoveDeadEnemies();
    }

    private void ResetWaveController()
    {
        var wc = FindObjectOfType<WaveController>();
        if(wc) wc.Reset();
    }

    private void ResetEnemies()
    {
        foreach (var e in enemySpawnManager.enemyList)
        {
            if (e.enemyB.dead)
            {
                if (!e.enemyB.respawnable)
                {
                    Destroy(e.enemyO);
                }
                else
                {
                    if (!e.enemyO) return;
                    if (!e.enemyO.activeSelf) e.enemyO.SetActive(true);
                    e.enemyO.transform.position = e.spawnPos;
                    e.enemyB.ResetHealth();
                }
            }
        }
        
        foreach (GameObject go in nonRespawnableEnemies)
        {
            Destroy(go);
        }
    }

    private void RemoveDeadEnemies()
    {
        GameObject[] deadEnemys = GameObject.FindGameObjectsWithTag("EnemyDead");
        for (int i = 0; i < deadEnemys.Length; i++)
        {
            Destroy(deadEnemys[i]);
        }
    }

    public int GetCurrentScene()
    {
        return gameObject.scene.buildIndex;
    }

    private void GoToScene()
    {
        StartCoroutine(asm.LoadScene(checkpointSceneIndex));
    }
    
    public void GoToScene(int idx)
    {
        StartCoroutine(asm.LoadScene(idx));
    }
}
