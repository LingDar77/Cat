namespace Cat.LocomotionSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using Cat.Utilities;


    /// <summary>
    /// An implementation of a ColliderComponent for 3D colliders.
    /// </summary>
    public abstract class ColliderComponent3D : ColliderComponent
    {

        protected new Collider collider = null;

        public RaycastHit[] UnfilteredHits { get; protected set; } = new RaycastHit[20];
        public List<RaycastHit> FilteredHits { get; protected set; } = new List<RaycastHit>(10);

        public Collider[] UnfilteredOverlaps { get; protected set; } = new Collider[20];
        public List<Collider> FilteredOverlaps { get; protected set; } = new List<Collider>(10);
        protected CollisionFilterDelegate3D internalHitFilter;
        protected OverlapFilterDelegate3D internalOverlapFilter;

        public PhysicMaterial Material
        {
            get => collider.sharedMaterial;
            set => collider.sharedMaterial = value;
        }

        protected abstract int InternalOverlapBody(Vector3 position, Quaternion rotation, float bottomOffset, Collider[] unfilteredResults, List<Collider> filteredResults, OverlapFilterDelegate3D filter);

        public sealed override int OverlapBody(Vector3 position, Quaternion rotation, float bottomOffset)
        {
            return InternalOverlapBody(position, rotation, bottomOffset, UnfilteredOverlaps, FilteredOverlaps, internalOverlapFilter);
        }

        public override bool ComputePenetration(ref Vector3 position, ref Quaternion rotation, PenetrationDelegate Action)
        {
            int overlaps = OverlapBody(position, rotation, BottomOffset);

            if (overlaps == 0)
                return false;

            for (int i = 0; i < overlaps; i++)
            {
                Collider otherCollider = FilteredOverlaps[i];

                if (otherCollider == collider)
                    continue;

                if (otherCollider.isTrigger)
                    continue;

                bool overlapped = Physics.ComputePenetration(
                    collider,
                    position,
                    rotation,
                    otherCollider,
                    otherCollider.transform.position,
                    otherCollider.transform.rotation,
                    out Vector3 direction,
                    out float distance
                );

                if (!overlapped)
                    continue;

                Action?.Invoke(ref position, ref rotation, otherCollider.transform, direction, distance);
            }

            return true;
        }

        protected bool InternalHitFilter(RaycastHit raycastHit)
        {
            if (raycastHit.collider == collider)
                return false;

            if (raycastHit.collider.isTrigger)
                return false;

            return true;
        }

        protected bool InternalOverlapFilter(Collider collider)
        {
            if (collider == this.collider)
                return false;

            if (collider.isTrigger)
                return false;

            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            PhysicMaterial material = new PhysicMaterial("Frictionless 3D");
            material.dynamicFriction = 0f;
            material.staticFriction = 0f;
            material.frictionCombine = PhysicMaterialCombine.Minimum;
            material.bounceCombine = PhysicMaterialCombine.Minimum;
            material.bounciness = 0f;

            collider.sharedMaterial = material;
            this.collider.hideFlags = HideFlags.NotEditable;

            internalHitFilter = InternalHitFilter;
            internalOverlapFilter = InternalOverlapFilter;
        }

        protected override void OnEnable()
        {
            collider.enabled = true;
        }

        protected override void OnDisable()
        {
            collider.enabled = false;
        }
    }


}
