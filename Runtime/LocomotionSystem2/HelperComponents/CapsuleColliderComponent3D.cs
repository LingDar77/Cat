namespace Cat.LocomotionSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using Cat.Utilities;
    /// <summary>
    /// This component represents a capsule collider in a 3D world.
    /// </summary>
    public class CapsuleColliderComponent3D : ColliderComponent3D
    {
        CapsuleCollider capsuleCollider = null;

        public override Vector3 Size
        {
            get
            {
                return new Vector2(2f * capsuleCollider.radius, capsuleCollider.height);
            }
            set
            {
                capsuleCollider.radius = value.x / 2f;
                capsuleCollider.height = value.y;
            }
        }

        public override Vector3 BoundsSize => capsuleCollider.bounds.size;

        public override Vector3 Offset
        {
            get => capsuleCollider.center;
            set => capsuleCollider.center = value;
        }

        protected override int InternalOverlapBody(Vector3 position, Quaternion rotation, float bottomOffset, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D filter)
        {
            Vector3 bottom = position + rotation * Vector3.up * (Size.x / 2f + bottomOffset);
            Vector3 top = position + rotation * Vector3.up * (Size.y + bottomOffset - Size.x / 2f);

            return PhysicsExtension.OverlapCapsule(
                bottom,
                top,
                Size.x / 2f,
                unfilteredResults,
                filteredResults,
                filter
            );
        }

        protected override void Awake()
        {
            if (!TryGetComponent(out capsuleCollider))
            {
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }
            collider = capsuleCollider;

            base.Awake();

        }

    }

}
