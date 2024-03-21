namespace Cat.LocomotionSystem
{

    using UnityEngine;
    /// <summary>
    /// This class contains all the character body properties, such as width, height, body shape, physics, etc.
    /// </summary>
    public class LocomotionShape : MonoBehaviour
    {

        [SerializeField]
        Vector2 bodySize = new Vector2(1f, 2f);

        [SerializeField]
        float mass = 50f;

        /// <summary>
        /// Gets the RigidbodyComponent component associated to the character.
        /// </summary>
        public RigidbodyComponent RigidbodyComponent { get; private set; }

        /// <summary>
        /// Gets the ColliderComponent component associated to the character.
        /// </summary>
        public ColliderComponent ColliderComponent { get; private set; }

        /// <summary>
        /// Gets the mass of the character.
        /// </summary>
        public float Mass => mass;

        /// <summary>
        /// Gets the body size of the character (width and height).
        /// </summary>
        public Vector2 BodySize => bodySize;


        /// <summary>
        /// Initializes the body properties and components.
        /// </summary>
        void Awake()
        {

            ColliderComponent = gameObject.AddComponent<CapsuleColliderComponent3D>();
            RigidbodyComponent = gameObject.AddComponent<RigidbodyComponent3D>();

        }

        BuiltinLocomotionSystem1 characterActor = null;


        void OnValidate()
        {
            if (characterActor == null)
                characterActor = GetComponent<BuiltinLocomotionSystem1>();

            bodySize = new Vector2(
                Mathf.Max(bodySize.x, 0f),
                Mathf.Max(bodySize.y, bodySize.x + CharacterConstants.ColliderMinBottomOffset)
            );

            if (characterActor != null)
                characterActor.OnValidate();
        }

    }

}
