namespace Cat.LocomotionSystem.Filter
{
    using UnityEngine;
    public interface IColliderFilter
    {
        bool ShouldCollide(Collider other);
    }
}