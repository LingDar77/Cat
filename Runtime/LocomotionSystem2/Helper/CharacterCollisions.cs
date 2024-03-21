namespace Cat.LocomotionSystem
{
    using UnityEngine;

    public struct RayArrayInfo
    {
        public float averageDistance;
        public Vector3 averageNormal;
    }

    // IMPORTANT: This class needs to be serializable in order to be compatible with assembly reloads.
    [System.Serializable]
    public class CharacterCollisions
    {

        BuiltinLocomotionSystem1 characterActor = null;
        PhysicsComponent physicsComponent = null;

        CollisionInfo collisionInfo = new CollisionInfo();

        public void Initialize(BuiltinLocomotionSystem1 characterActor, PhysicsComponent physicsComponent)
        {
            this.characterActor = characterActor;
            this.physicsComponent = physicsComponent;
        }

        // Important: PhysX (3D Physics) does not use the "contact offset" for depenetration purposes (like 2D does). Instead, it defines an internal restDistance which is then 
        // used to eliminate penetration between two bodies. Contact generation is still defined by the contact offset.
        // On the other hand, Box2D uses the "contact offset" for both contact generation and de-penetration.
        // This means that 3D collision will be handled using the "skin width" concept (collision shape smaller than the collider). For 2D both collision shape and collider are the same.

        public float ContactOffset => CharacterConstants.SkinWidth;
        public float CollisionRadius => characterActor.BodySize.x / 2f - ContactOffset;
        float BackstepDistance => 2f * ContactOffset;

        /// <summary>
        /// Checks vertically for the ground using a SphereCast.
        /// </summary>
        public CollisionInfo CheckForGround(Vector3 position, float stepOffset, float stepDownDistance, in HitInfoFilter hitInfoFilter)
        {
            float preDistance = stepOffset + BackstepDistance;
            Vector3 displacement = -characterActor.Up * Mathf.Max(CharacterConstants.GroundCheckDistance, stepDownDistance);

            Vector3 castDisplacement = displacement + Vector3.Normalize(displacement) * preDistance;

            Vector3 origin = characterActor.GetBottomCenter(position, preDistance);

            int hits = physicsComponent.SphereCast(
                out HitInfo hitInfo,
                origin,
                CollisionRadius,
                castDisplacement,
                in hitInfoFilter
            );

            UpdateCollisionInfo(collisionInfo, position, in hitInfo, displacement, castDisplacement, preDistance, true, in hitInfoFilter);
            return collisionInfo;
        }



        /// <summary>
        /// Checks vertically for the ground using a Raycast.
        /// </summary>
        public CollisionInfo CheckForGroundRay(Vector3 position, in HitInfoFilter hitInfoFilter)
        {

            float preDistance = characterActor.BodySize.x / 2f;

            // GroundCheckDistance is not high enough (usually), especially if the character is on top of a slope.
            Vector3 displacement = -characterActor.Up * Mathf.Max(CharacterConstants.GroundCheckDistance, characterActor.stepDownDistance);

            Vector3 castDisplacement = displacement + Vector3.Normalize(displacement * preDistance);

            Vector3 origin = characterActor.GetBottomCenter(position);


            physicsComponent.SimpleRaycast(out HitInfo hitInfo, origin, castDisplacement, in hitInfoFilter);

            UpdateCollisionInfo(collisionInfo, position, in hitInfo, displacement, castDisplacement, preDistance, false, in hitInfoFilter);

            return collisionInfo;


        }


        public CollisionInfo CastBody(Vector3 position, Vector3 displacement, float bottomOffset, in HitInfoFilter hitInfoFilter)
        {

            float preDistance = BackstepDistance;

            Vector3 bottom = characterActor.GetBottomCenter(position, bottomOffset);
            Vector3 top = characterActor.GetTopCenter(position);
            Vector3 direction = Vector3.Normalize(displacement);

            bottom -= direction * preDistance;
            top -= direction * preDistance;


            Vector3 castDisplacement = displacement + direction * preDistance;

            int hits = physicsComponent.CapsuleCast(
                out HitInfo hitInfo,
                bottom,
                top,
                CollisionRadius,
                castDisplacement,
                in hitInfoFilter
            );

            UpdateCollisionInfo(collisionInfo, position, in hitInfo, displacement, castDisplacement, preDistance, false, in hitInfoFilter);


            return collisionInfo;
        }


        /// <summary>
        /// Performs an overlap test at a given position.
        /// </summary>
        [System.Obsolete("Use CheckOverlap instead.")]
        public bool CheckOverlapWithLayerMask(Vector3 position, float bottomOffset, in HitInfoFilter hitInfoFilter) =>
            CheckOverlap(position, bottomOffset, in hitInfoFilter);

        /// <summary>
        /// Performs an overlap test at a given position.
        /// </summary>
        /// <param name="position">Target position.</param>
        /// <param name="bottomOffset">Bottom offset of the capsule.</param>
        /// <param name="hitInfoFilter">Overlap filter.</param>
        /// <returns>True if the overlap test detects some obstacle.</returns>
        public bool CheckOverlap(Vector3 position, float bottomOffset, in HitInfoFilter hitInfoFilter)
        {
            Vector3 bottom = characterActor.GetBottomCenter(position, bottomOffset);
            Vector3 top = characterActor.GetTopCenter(position);
            float radius = characterActor.BodySize.x / 2f - CharacterConstants.SkinWidth;

            bool overlap = physicsComponent.OverlapCapsule(
                bottom,
                top,
                radius,
                in hitInfoFilter
            );

            return overlap;
        }

        /// <summary>
        /// Checks if the character fits at a specific location.
        /// </summary>
        public bool CheckBodySize(Vector3 size, Vector3 position, in HitInfoFilter hitInfoFilter)
        {
            Vector3 bottom = characterActor.GetBottomCenter(position, size);
            float radius = size.x / 2f;

            // BottomCenterToTopCenter = Up displacement
            Vector3 castDisplacement = characterActor.GetBottomCenterToTopCenter(size);

            physicsComponent.SphereCast(
                out HitInfo hitInfo,
                bottom,
                radius,
                castDisplacement,
                in hitInfoFilter
            );

            bool overlap = hitInfo.hit;
            return !overlap;
        }

        /// <summary>
        /// Checks if the character fits in place.
        /// </summary>
        public bool CheckBodySize(Vector3 size, in HitInfoFilter hitInfoFilter) => CheckBodySize(size, characterActor.Position, in hitInfoFilter);


        public void UpdateCollisionInfo(
            CollisionInfo collisionInfo, Vector3 position, in HitInfo hitInfo, Vector3 displacement, Vector3 castDisplacement,
            float preDistance, bool calculateEdge = true, in HitInfoFilter hitInfoFilter = new HitInfoFilter()
        )
        {
            if (hitInfo.hit)
            {
                Vector3 characterUp = characterActor.Up;
                Vector3 castDirection = Vector3.Normalize(castDisplacement);

                displacement = castDirection * (hitInfo.distance - preDistance - ContactOffset);

                if (calculateEdge)
                {
                    Vector3 edgeCenterReference = characterActor.GetBottomCenter(position + displacement, 0f);
                    UpdateEdgeInfo(in edgeCenterReference, in hitInfo.point, in hitInfoFilter, out HitInfo upperHitInfo, out HitInfo lowerHitInfo);

                    collisionInfo.SetData(in hitInfo, characterUp, displacement, in upperHitInfo, in lowerHitInfo);
                }
                else
                {
                    collisionInfo.SetData(in hitInfo, characterUp, displacement);
                }
            }
            else
            {
                collisionInfo.Reset();
            }

        }

        void UpdateEdgeInfo(in Vector3 edgeCenterReference, in Vector3 contactPoint, in HitInfoFilter hitInfoFilter, out HitInfo upperHitInfo, out HitInfo lowerHitInfo)
        {
            Vector3 castDirection = contactPoint - edgeCenterReference;
            castDirection.Normalize();

            Vector3 castDisplacement = castDirection * CharacterConstants.EdgeRaysCastDistance;

            Vector3 upperHitPosition = edgeCenterReference + characterActor.Up * CharacterConstants.EdgeRaysSeparation;
            Vector3 lowerHitPosition = edgeCenterReference - characterActor.Up * CharacterConstants.EdgeRaysSeparation;


            physicsComponent.SimpleRaycast(out upperHitInfo, upperHitPosition, castDisplacement, in hitInfoFilter);

            physicsComponent.SimpleRaycast(out lowerHitInfo, lowerHitPosition, castDisplacement, in hitInfoFilter);

        }
    }

}