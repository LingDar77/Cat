using UnityEngine;

namespace TUI.LocomotionSystem.Filter
{
    public interface IColliderFilter
    {
        bool ShouldCollide(Collider other);
    }
}