
using UnityEngine;

namespace TUI
{
    /// <summary>
    /// A simple definition of a bias rotation applied to a coordinate.
    /// </summary>
    public interface IRotateBiasable: ITransformGetable
    {
        public Quaternion Bias { get; set; }
        public bool Initialized { get; }
    }
}
