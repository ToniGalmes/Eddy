﻿using UnityEngine;

namespace Steerings
{
	public class KinematicState : MonoBehaviour
	{
		public float maxAcceleration = 2f;
		public float maxSpeed = 10f;
		public float maxAngularAcceleration = 45f;
		public float maxAngularSpeed = 45f;

		[HideInInspector] public Vector3 position;
		[HideInInspector] public float orientation;
		[HideInInspector] public Vector3 linearVelocity;
		[HideInInspector] public float angularSpeed;

		void Start ()
		{
			position = transform.position;
			orientation = transform.eulerAngles.y;
			linearVelocity = Vector3.zero;
			angularSpeed = 0f;
		}
	}
}