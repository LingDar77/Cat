namespace Cat.LocomotionSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using Cat.Utilities;
    /// <summary>
    /// This component represents a capsule collider in a 2D world.
    /// </summary>
    public class BoxColliderComponent3D : ColliderComponent3D
    {
        BoxCollider boxCollider = null;


        public override Vector3 Size
        {
            get
            {
                return boxCollider.size;
            }
            set
            {
                boxCollider.size = value;
            }
        }

        public override Vector3 BoundsSize => boxCollider.bounds.size;

        public override Vector3 Offset
        {
            get
            {
                return boxCollider.center;
            }
            set
            {
                boxCollider.center = value;
            }
        }

        protected override int InternalOverlapBody(Vector3 position, Quaternion rotation, float bottomOffset, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D filter)
        {
            Vector3 bottom = position + rotation * Vector3.up * (Size.x / 2f + bottomOffset);

            return PhysicsExtension.OverlapBox(
                bottom,
                Size,
                rotation,
                unfilteredResults,
                filteredResults,
                filter
            );
        }

        protected override void Awake()
        {
            boxCollider = GetComponentInChildren<BoxCollider>();
            if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();
            collider = boxCollider;

            base.Awake();

        }


    }



}
