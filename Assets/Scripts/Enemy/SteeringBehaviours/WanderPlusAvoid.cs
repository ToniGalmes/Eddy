﻿using UnityEngine;

namespace Steerings
{
	public class WanderPlusAvoid : SteeringBehaviour
	{
		public float wanderRate = 30f;
		public float wanderRadius = 10f;
		public float wanderOffset = 20f;
		private float targetOrientation = 0f;

		public float lookAheadLength = 10f;
		public float avoidDistance = 10f;
		public float secondaryWhiskerAngle = 30f;
		public float secondaryWhiskerRatio = 0.7f;

		private bool avoidActive = false;


		public override SteeringOutput GetSteering ()
		{
			SteeringOutput result = WanderPlusAvoid.GetSteering (this.ownKS, wanderRate, wanderRate, wanderOffset, ref targetOrientation, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio, ref avoidActive);

			if (ownKS.linearVelocity.magnitude > 0.001f)
			{
				transform.rotation = Quaternion.Euler(0, VectorToOrientation(ownKS.linearVelocity), 0);
				ownKS.orientation = transform.rotation.eulerAngles.z;
			}
			result.angularActive = false;

			return result;
		}

		public static SteeringOutput GetSteering (KinematicState ownKS, float WanderRate, float wanderRadius, float wanderOffset, ref float targetOrientation, float lookAheadLength, float avoidDistance, float secondaryWhiskerAngle, float secondaryWhiskerRatio, ref bool avoidActive)
		{
			SteeringOutput so = ObstacleAvoidance.GetSteering(ownKS, lookAheadLength, avoidDistance, secondaryWhiskerAngle, secondaryWhiskerRatio);

			if (so == NULL_STEERING)
			{
				if (avoidActive)
				{
					// if avoidance was active last frame, update target orientation (otherwise the object would tend to regain
					// the orientation it had before avoiding a collision which would make it face the obstacle again)
					targetOrientation = ownKS.orientation;
				}
				avoidActive = false;
				return Wander.GetSteering (ownKS, ref targetOrientation, WanderRate, wanderRadius, wanderOffset);
			}
			else
			{
				avoidActive = true;
				return so;
			}

		}
	}
}
