﻿/* **************** VERSION 2 ****************** */
using UnityEngine;


namespace Steerings
{
    public class Wander : SteeringBehaviour
    {
        public float wanderRate = 30f;
        public float wanderRadius = 10f;
        public float wanderOffset = 20f;

        protected float targetOrientation = 0f;

        public override SteeringOutput GetSteering()
        {
            SteeringOutput result = Wander.GetSteering(ownKS, ref targetOrientation, wanderRate, wanderRadius, wanderOffset);

            if (ownKS.linearVelocity.magnitude > 0.001f)
            {
                transform.rotation = Quaternion.Euler(0, VectorToOrientation(ownKS.linearVelocity), 0);
                ownKS.orientation = transform.rotation.eulerAngles.z;
            }
            result.angularActive = false;

            return result;
        }

        public static SteeringOutput GetSteering(KinematicState ownKS, ref float targetOrientation, float wanderRate, float wanderRadius, float wanderOffset)
        {

            // change target orientation (change location of surrogate target on unit circle)
            targetOrientation += wanderRate * (Random.value - Random.value);

            // place surrogate target on circle of wanderRadius
            SURROGATE_TARGET.transform.position = OrientationToVector(targetOrientation) * wanderRadius;

            // place circle  "in front"
            SURROGATE_TARGET.transform.position += ownKS.position + OrientationToVector(ownKS.orientation) * wanderOffset;

            return Seek.GetSteering(ownKS, SURROGATE_TARGET);
        }
    }
}
