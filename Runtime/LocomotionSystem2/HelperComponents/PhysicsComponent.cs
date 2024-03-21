using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cat.LocomotionSystem
{

    /// <summary>
    /// This component offers a set of useful functionalities for 2D and 3D physics.
    /// </summary>
    public abstract class PhysicsComponent : MonoBehaviour
    {

        //protected int hits = 0;


        public HitInfo[] HitsBuffer { get; protected set; } = new HitInfo[20];

        /// <summary>
        /// Gets a list with all the current contacts.
        /// </summary>
        public List<Contact> Contacts { get; protected set; } = new List<Contact>(20);

        /// <summary>
        /// Gets a list with all the current triggers.
        /// </summary>
        public List<Trigger> Triggers { get; protected set; } = new List<Trigger>(20);

        protected abstract LayerMask GetCollisionLayerMask();

        /// <summary>
        /// Ignores the collision between this object and some other collider.
        /// </summary>
        public abstract void IgnoreCollision(in HitInfo hitInfo, bool ignore);

        /// <summary>
        /// Ignores the collision between this object and some other collider.
        /// </summary>
        public abstract void IgnoreCollision(Transform otherTransform, bool ignore);

        /// <summary>
        /// Ignores the collision between this object and a layer.
        /// </summary>
        public abstract void IgnoreLayerCollision(int targetLayer, bool ignore);

        /// <summary>
        /// Ignores the collision between this object and a layer mask.
        /// </summary>
        public abstract void IgnoreLayerMaskCollision(LayerMask layerMask, bool ignore);
        protected abstract int FilterOverlaps(int overlaps, LayerMask ignoredLayerMask);

        // Contacts ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        public void ClearContacts() => Contacts.Clear();

        protected abstract void GetClosestHit(out HitInfo hitInfo, int hits, Vector3 castDisplacement, in HitInfoFilter filter);

        // Casts ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Raycast wrapper for 2D/3D physics.
        /// </summary>
        public abstract bool SimpleRaycast(out HitInfo hitInfo, Vector3 origin, Vector3 castDisplacement, in HitInfoFilter filter);

        /// <summary>
        /// RaycastAll wrapper for 2D/3D physics.
        /// </summary>
        public abstract int Raycast(out HitInfo hitInfo, Vector3 origin, Vector3 castDisplacement, in HitInfoFilter filter);

        /// <summary>
        /// SphereCastAll wrapper for 2D/3D physics.
        /// </summary>
        public abstract int SphereCast(out HitInfo hitInfo, Vector3 center, float radius, Vector3 castDisplacement, in HitInfoFilter filter);

        /// <summary>
        /// CapsuleCastAll wrapper for 2D/3D physics.
        /// </summary>
        public abstract int CapsuleCast(out HitInfo hitInfo, Vector3 bottom, Vector3 top, float radius, Vector3 castDisplacement, in HitInfoFilter filter);

        /// <summary>
        /// BoxCastAll wrapper for 2D/3D physics. It returns (by reference) the closest hit.
        /// </summary>
        public abstract int BoxCast(out HitInfo hitInfo, Vector3 center, Vector3 size, Vector3 castDisplacement, Quaternion orientation, in HitInfoFilter filter);

        /// <summary>
        /// BoxCastAll wrapper for 2D/3D physics. It doesn't return any particular hit, instead, it updates all the hits from the buffer (HitInfo array).
        /// This buffer can be obtained via the HitsBuffer property.
        /// </summary>
        public abstract int BoxCast(Vector3 center, Vector3 size, Vector3 castDisplacement, Quaternion orientation, in HitInfoFilter filter);

        // Overlaps ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// OverlapSphere wrapper for 2D/3D physics.
        /// </summary>
        public abstract bool OverlapSphere(Vector3 center, float radius, in HitInfoFilter filter);

        /// <summary>
        /// OverlapCapsule wrapper for 2D/3D physics.
        /// </summary>
        public abstract bool OverlapCapsule(Vector3 bottom, Vector3 top, float radius, in HitInfoFilter filter);

        /// <summary>
        /// Returns a layer mask with all the valid collisions associated with the object, based on the collision matrix (physics settings).
        /// </summary>
        public LayerMask CollisionLayerMask { get; protected set; } = 0;

        protected virtual void Awake()
        {
            this.hideFlags = HideFlags.None;

            CollisionLayerMask = GetCollisionLayerMask();
        }

        RigidbodyComponent rigidbodyComponent = null;

        protected virtual void Start()
        {
            rigidbodyComponent = GetComponent<RigidbodyComponent>();

        }

        public static PhysicsComponent CreateInstance(GameObject gameObject)
        {
            Rigidbody rigidbody3D = gameObject.GetComponent<Rigidbody>();
            if (rigidbody3D != null)
            {
                if (!gameObject.TryGetComponent(out PhysicsComponent result))
                {
                    result = gameObject.AddComponent<PhysicsComponent>();
                }
                return result;
            }

            return null;
        }

        protected bool ignoreCollisionMessages = false;


        Coroutine postSimulationCoroutine = null;

        void OnEnable()
        {
            rigidbodyComponent = GetComponent<RigidbodyComponent>();

            if (rigidbodyComponent != null)
                rigidbodyComponent.OnBodyTypeChange += OnBodyTypeChange;

            if (postSimulationCoroutine == null)
                postSimulationCoroutine = StartCoroutine(PostSimulationUpdate());
        }

        void OnDisable()
        {
            if (rigidbodyComponent != null)
                rigidbodyComponent.OnBodyTypeChange -= OnBodyTypeChange;

            if (postSimulationCoroutine != null)
            {
                StopCoroutine(PostSimulationUpdate());
                postSimulationCoroutine = null;
            }
        }

        void OnBodyTypeChange()
        {
            ignoreCollisionMessages = true;
        }

        void FixedUpdate()
        {
            // Update the collision layer mask (collision matrix) of this object.
            // CollisionLayerMask = GetCollisionLayerMask();
            // --> Performance cost! This has been replaced by an internal mask that's modified every time IgnoreCollision is called. <--

            // If there are null triggers then delete them from the list
            for (int i = Triggers.Count - 1; i >= 0; i--)
            {
                if (Triggers[i].gameObject == null)
                    Triggers.RemoveAt(i);
            }
        }

        IEnumerator PostSimulationUpdate()
        {
            YieldInstruction waitForFixedUpdate = new WaitForFixedUpdate();
            while (true)
            {
                yield return waitForFixedUpdate;

                ignoreCollisionMessages = false;

            }
        }

        protected bool wasKinematic = false;
    }

}

