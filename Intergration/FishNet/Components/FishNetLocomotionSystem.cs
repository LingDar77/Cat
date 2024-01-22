namespace TUI.Intergration.LocomotionSystem
{
    using FishNet;
    using FishNet.Managing.Timing;
    using TUI.LocomotionSystem;
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
        protected override void Start()
        {
        }
        private void OnUpdate()
        {
            DoSimulation(Time.deltaTime);
        }
    }
}