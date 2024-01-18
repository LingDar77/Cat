using System.Collections.Generic;
using UnityEngine;

namespace TUI.KinematicLocomotionSystem
{
    /// <summary>
    /// The system that manages the simulation of KinematicCharacterMotor and PhysicsMover
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class KinematicCharacterSystem : MonoBehaviour
    {
        private static KinematicCharacterSystem _instance;

        public static List<KinematicCharacterMotor> CharacterMotors = new List<KinematicCharacterMotor>();
        public static List<PhysicsMover> PhysicsMovers = new List<PhysicsMover>();

        /// <summary>
        /// Creates a KinematicCharacterSystem instance if there isn't already one
        /// </summary>
        public static void EnsureCreation()
        {
            if (_instance == null)
            {
                GameObject systemGameObject = new GameObject("KinematicCharacterSystem");
                _instance = systemGameObject.AddComponent<KinematicCharacterSystem>();

                systemGameObject.hideFlags = HideFlags.NotEditable;
                _instance.hideFlags = HideFlags.NotEditable;

                GameObject.DontDestroyOnLoad(systemGameObject);
            }
        }

        /// <summary>
        /// Gets the KinematicCharacterSystem instance if any
        /// </summary>
        /// <returns></returns>
        public static KinematicCharacterSystem GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Sets the maximum capacity of the character motors list, to prevent allocations when adding characters
        /// </summary>
        /// <param name="capacity"></param>
        public static void SetCharacterMotorsCapacity(int capacity)
        {
            if (capacity < CharacterMotors.Count)
            {
                capacity = CharacterMotors.Count;
            }
            CharacterMotors.Capacity = capacity;
        }

        /// <summary>
        /// Registers a KinematicCharacterMotor into the system
        /// </summary>
        public static void RegisterCharacterMotor(KinematicCharacterMotor motor)
        {
            CharacterMotors.Add(motor);
        }

        /// <summary>
        /// Unregisters a KinematicCharacterMotor from the system
        /// </summary>
        public static void UnregisterCharacterMotor(KinematicCharacterMotor motor)
        {
            CharacterMotors.Remove(motor);
        }

        /// <summary>
        /// Sets the maximum capacity of the physics movers list, to prevent allocations when adding movers
        /// </summary>
        /// <param name="capacity"></param>
        public static void SetPhysicsMoversCapacity(int capacity)
        {
            if (capacity < PhysicsMovers.Count)
            {
                capacity = PhysicsMovers.Count;
            }
            PhysicsMovers.Capacity = capacity;
        }

        /// <summary>
        /// Registers a PhysicsMover into the system
        /// </summary>
        public static void RegisterPhysicsMover(PhysicsMover mover)
        {
            PhysicsMovers.Add(mover);

            mover.Rigidbody.interpolation = RigidbodyInterpolation.None;
        }

        /// <summary>
        /// Unregisters a PhysicsMover from the system
        /// </summary>
        public static void UnregisterPhysicsMover(PhysicsMover mover)
        {
            PhysicsMovers.Remove(mover);
        }

        // This is to prevent duplicating the singleton gameobject on script recompiles
        private void OnDisable()
        {
            Destroy(this.gameObject);
        }

        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            // float deltaTime = Time.fixedDeltaTime;

            // PreSimulationInterpolationUpdate(deltaTime);
            Simulate(Time.deltaTime, CharacterMotors, PhysicsMovers);
            // PostSimulationInterpolationUpdate(deltaTime);
        }

        /// <summary>
        /// Ticks characters and/or movers
        /// </summary>
        public static void Simulate(float deltaTime, List<KinematicCharacterMotor> motors, List<PhysicsMover> movers)
        {
            int characterMotorsCount = motors.Count;

            // Character controller update phase 1
            for (int i = 0; i < characterMotorsCount; i++)
            {
                var motor = motors[i];
                motor.UpdatePhase1(deltaTime);

                motor.UpdatePhase2(deltaTime);

                motor.transform.SetPositionAndRotation(motor._transientPosition, motor._transientRotation);
            }

        }

    }
}