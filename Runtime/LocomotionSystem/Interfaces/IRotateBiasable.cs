
using UnityEngine;

namespace SFC
{
    /// <summary>
    /// A simple definition of a bias rotation applied to a coordinate.
    /// </summary>
    public interface IRotateBiasable
    {
        public Transform transform { get; }
        public Quaternion Bias { get; set; }
        public bool Initialized { get; }
    }
}
