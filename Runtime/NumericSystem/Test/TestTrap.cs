namespace Cat.NumericSystem
{
    using UnityEngine;
    using Cat.Utilities;
    public class TestTrap : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (!other.transform.TryGetComponentInParent<MaximumLife>(out var life)) return;

            life.CurrentValue -= 10;
        }
    }
}