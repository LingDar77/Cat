namespace TUI.Intergration.LocomotionSystem
{
    using System;
    using FishNet;
    using FishNet.Managing.Timing;
    using TUI.LocomotionSystem;

    public class FishNetLocomotionSystem : BuiltinLocomotionSystem
    {
        private TimeManager TimeManager;

        private void Start()
        {
            TimeManager = InstanceFinder.TimeManager;
            TimeManager.OnTick += OnTick;
        }

        protected override void Update()
        {
            // base.Update();
        }
        private void OnTick()
        {
            var time = (float)TimeManager.TickDelta;
            PrepareSimulatioin(time);
            Simulation(time);
            transform.SetPositionAndRotation(TargetPosition, TargetRotation);
        }
    }
}