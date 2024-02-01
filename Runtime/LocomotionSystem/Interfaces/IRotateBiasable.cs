
using UnityEngine;

namespace TUI.LocomotionSystem
{
    /// <summary>
    /// A simple definition of a bias rotation applied to a coordinate.
    /// </summary>
    public interface IRotateBiasable: ITransformGetable
    {
        public Quaternion RotationBias { get; set; }
        public Quaternion VelocityBias { get; set; }
        public bool Initialized { get; }
    }
}
