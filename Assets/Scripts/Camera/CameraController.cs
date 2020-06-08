﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Basics")]
    public CameraRail rail;
    public Transform target;

    [Header("Movement")]
    public float maxMovementSpeed;
    //public float slowingRadius;

    [Header("Rotation")]
    public float rotationSpeed;
    public bool lookAtPlayer;

    private Rigidbody rb;
    private Vector3 desiredVelocity = Vector3.zero;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rail != null)
        {


            Vector3 pos = rail.ProjectPosition(target.position);

            transform.position = Vector3.Lerp(transform.position, pos, maxMovementSpeed * Time.deltaTime);

            /*float distanceFromPosition = (pos - transform.position).magnitude;
            float stoppingFactor;

            stoppingFactor = Mathf.Clamp(distanceFromPosition / slowingRadius, 0.0f, 1.0f);

            desiredVelocity = (pos - transform.position).normalized * maxMovementSpeed * stoppingFactor;

            rb.velocity += desiredVelocity - rb.velocity;*/



            if (lookAtPlayer)
            {
                transform.LookAt(target.position);
            }
            else
            {
                Vector3 rot = rail.ProjectRotation(target.position, transform.position);

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rot), rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void FixedUpdate()
    {

    }

    public void SetPositionImmediately()
    {
        transform.position = rail.ProjectPosition(target.position, true);
        transform.rotation = Quaternion.Euler(rail.ProjectRotation(target.position, transform.position));
    }
}
