namespace Cat.Utilities
{
    using UnityEngine;

    public static class MathExtension
    {
        public static bool BelongsToLayerMask(this int layer, int layerMask)
        {
            return (layerMask & (1 << layer)) > 0;
        }
        public static bool BelongsToLayerMask(this LayerMask layer, int layerMask)
        {
            return (layerMask & (1 << layer)) > 0;
        }

        public static void AddMagnitude(this Vector3 vector, float magnitude)
        {
            if (vector == Vector3.zero)
                return;

            float vectorMagnitude = Vector3.Magnitude(vector);
            Vector3 vectorDirection = vector / vectorMagnitude;

            vector += vectorDirection * magnitude;
        }

        public static Vector3 ProjectOnTangent(this Vector3 inputVector, Vector3 planeNormal, Vector3 up)
        {
            Vector3 inputVectorDirection = Vector3.Normalize(inputVector);

            if (inputVectorDirection == -up)
            {
                inputVector += planeNormal * 0.01f;
            }
            else if (inputVectorDirection == up)
            {
                return Vector3.zero;
            }

            Vector3 rotationAxis = Vector3.Normalize(Vector3.Cross(inputVector, up));
            Vector3 tangent = Vector3.Normalize(Vector3.Cross(planeNormal, rotationAxis));

            return tangent * Vector3.Magnitude(inputVector);
        }

        public static Vector3 DeflectVector(this Vector3 inputVector, Vector3 planeA, Vector3 planeB, bool maintainMagnitude = false)
        {
            Vector3 direction = Vector3.Normalize(Vector3.Cross(planeA, planeB));

            if (maintainMagnitude)
                return direction * inputVector.magnitude;
            else
                return Vector3.Project(inputVector, direction);
        }
    }
}
