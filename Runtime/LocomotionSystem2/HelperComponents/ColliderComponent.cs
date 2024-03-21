namespace Cat.LocomotionSystem
{
    using UnityEngine;

    /// <summary>
    /// This component is an encapsulation of the Collider and Collider2D components, containing the most commonly used 
    /// properties and methods from these components.
    /// </summary>
    public abstract class ColliderComponent : MonoBehaviour
    {
        /// <summary>
        /// The size of the collider.
        /// </summary>
        public abstract Vector3 Size { get; set; }

        /// <summary>
        /// The distance between the center of the collider and the position of the object.
        /// </summary>
        public abstract Vector3 Offset { get; set; }

        /// <summary>
        /// The collider bounding volume.
        /// </summary>
        public abstract Vector3 BoundsSize { get; }


        public Vector3 Center => transform.position + transform.rotation * Offset;

        public float BottomOffset => Offset.y - 0.5f * Size.y;

        public static ColliderComponent CreateInstance(GameObject gameObject, bool includeChildren = true)
        {
            Collider collider3D = includeChildren ? gameObject.GetComponentInChildren<Collider>() : gameObject.GetComponent<Collider>();

            if (collider3D != null)
            {
                // Box collider ------------------------------------------------------------
                BoxCollider boxCollider3D = null;

                try
                {
                    boxCollider3D = (BoxCollider)collider3D;
                }
                catch (System.Exception) { }

                if (boxCollider3D != null)
                    return gameObject.AddComponent<BoxColliderComponent3D>();


                // Circle collider ------------------------------------------------------------
                SphereCollider sphereCollider3D = null;

                try
                {
                    sphereCollider3D = (SphereCollider)collider3D;
                }
                catch (System.Exception) { }

                if (sphereCollider3D != null)
                    return gameObject.AddComponent<SphereColliderComponent3D>();

                // Capsule collider ------------------------------------------------------------
                CapsuleCollider capsuleCollider3D = null;

                try
                {
                    capsuleCollider3D = (CapsuleCollider)collider3D;
                }
                catch (System.Exception) { }

                if (capsuleCollider3D != null)
                    return gameObject.AddComponent<CapsuleColliderComponent3D>();
            }


            return null;

        }

        public delegate void PenetrationDelegate(ref Vector3 bodyPosition, ref Quaternion bodyRotation, Transform otherColliderTransform, Vector3 penetrationDirection, float penetrationDistance);

        /// <summary>
        /// Calcules the amount of penetration between this body and nearby neighbors. Alternatively, an action (delegate)
        /// can be passed in, so the resulting position/rotation can be modified if needed. 
        /// </summary>
        /// <param name="position">The position reference.</param>
        /// <param name="rotation">The rotation reference.</param>
        /// <param name="Action">This delegate will be called after the penetration value is calculated.</param>
        /// <returns>True if there was any valid overlap.</returns>
        public abstract bool ComputePenetration(ref Vector3 position, ref Quaternion rotation, PenetrationDelegate Action);


        /// <summary>
        /// Performs an overlap check using the body shape. The filter used in this case corresponds to the internal one from the PhysicsBody2D/3D class.
        /// If a custom filter is required, 2D/3D implementations must be used instead.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public abstract int OverlapBody(Vector3 position, Quaternion rotation, float bottomOffset);

        protected abstract void OnEnable();
        protected abstract void OnDisable();

        protected virtual void Awake()
        {
            hideFlags = HideFlags.None;
        }

    }

}