namespace Cat.Intergration.LocomotionSystem
{
    using FishNet;
    using FishNet.Managing.Timing;
    using global::Cat.LocomotionSystem;
    using UnityEngine;

    public class FishNetLocomotionSystem : BuiltinLocomotionSystem
    {
        private TimeManager TimeManager;

        private void OnEnable()
        {
            TimeManager = InstanceFinder.TimeManager;
            TimeManager.OnUpdate += OnUpdate;
        }
        private void OnDisable()
        {
            TimeManager.OnUpdate -= OnUpdate;
        }
        protected override void Update()
        {
        }
        private void OnUpdate()
        {
            DoSimulation(Time.deltaTime);
        }
    }
}