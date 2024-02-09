namespace Cat.NumericSystem
{
    using UnityEngine;
    using Cat.Utillities;
    public class TestTrap : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (!other.transform.TryGetComponentInParent<MaximumLife>(out var life)) return;

            life.CurrentValue -= 10;
        }
    }
}