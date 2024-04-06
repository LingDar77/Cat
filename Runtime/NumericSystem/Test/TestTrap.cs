namespace Cat.NumericSystem
{
    using UnityEngine;
    using Cat.Utilities;
    public class TestTrap : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (!other.transform.TryGetComponentInParent<Life>(out var life)) return;

            life.CurrentValue -= 10;
        }
    }
}