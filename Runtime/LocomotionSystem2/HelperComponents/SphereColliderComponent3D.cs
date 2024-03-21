namespace Cat.LocomotionSystem
{
    using System.Collections.Generic;
    using Cat.Utilities;
    using UnityEngine;
    /// <summary>
    /// This component represents a sphere collider in a 3D world.
    /// </summary>
    public class SphereColliderComponent3D : ColliderComponent3D
    {
        SphereCollider sphereCollider = null;

        public override Vector3 Size
        {
            get
            {
                return Vector3.one * 2f * sphereCollider.radius;
            }
            set
            {
                sphereCollider.radius = value.x / 2f;
            }
        }

        public override Vector3 BoundsSize => sphereCollider.bounds.size;


        public override Vector3 Offset
        {
            get
            {
                return sphereCollider.center;
            }
            set
            {
                sphereCollider.center = value;
            }
        }

        protected override int InternalOverlapBody(Vector3 position, Quaternion rotation, float bottomOffset, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D filter)
        {
            return PhysicsExtension.OverlapSphere(
                position,
                Size.x / 2f,
                unfilteredResults,
                filteredResults,
                filter
            );
        }

        protected override void Awake()
        {

            sphereCollider = GetComponentInChildren<SphereCollider>();
            if (collider == null) sphereCollider = gameObject.AddComponent<SphereCollider>();
            collider = sphereCollider;

            base.Awake();

        }


    }

}
