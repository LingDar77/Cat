namespace TUI.Intergration.LocomotionSystem
{
    using FishNet;
    using FishNet.Managing.Timing;
    using TUI.LocomotionSystem;

    public class FishNetLocomotionSystem : BuiltinLocomotionSystem
    {
        private TimeManager TimeManager;

        private void OnEnable()
        {
            TimeManager = InstanceFinder.TimeManager;
            TimeManager.OnTick += OnTick;
        }
        private void OnDisable()
        {
            TimeManager.OnTick -= OnTick;
        }
        protected override void Start()
        {
        }
        private void OnTick()
        {
            DoSimulation((float)TimeManager.TickDelta);
        }
    }
}