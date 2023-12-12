using UnityEngine;

namespace SFC.Example
{
    public class TestMover : MonoBehaviour
    {
        [ContextMenu("Teleport2Zero")]
        public void Teleport2Zero()
        {
            Move2(Vector3.zero);
        }
        [ContextMenu("Teleport2Random")]
        public void Teleport2Random()
        {
            var target = Vector3.one * Random.Range(2, 4);
            Move2(target);
        }

        public void Move2(Vector3 target)
        {
            if (TryGetComponent<Rigidbody>(out var rigidbody))
            {
                rigidbody.MovePosition(target);
            }
            transform.position = target;
        }
    }
}
