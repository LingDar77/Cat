
using UnityEngine;

namespace SFC.Utillities
{
    public static class MathExtension
    {
        public static float MaxWithThreshold(this float lhs, float rhs = 0f, float threshold = .01f)
        {
            return Mathf.Abs(lhs) > rhs + threshold ? lhs : rhs;
        }
        public static bool NearlyEqualsTo(this float lhs, float rhs, float threshold = .01f)
        {
            return Mathf.Abs(lhs - rhs) <= threshold;
        }
        public static bool NearlyEqualsTo(this Vector3 lhs, Vector3 rhs, float threshold = .01f)
        {
            return lhs.x.NearlyEqualsTo(rhs.x, threshold) && lhs.y.NearlyEqualsTo(rhs.y, threshold) && lhs.z.NearlyEqualsTo(rhs.z, threshold);
        }
        public static bool NearlyEqualsTo(this Vector2 lhs, Vector2 rhs, float threshold = .01f)
        {
            return lhs.x.NearlyEqualsTo(rhs.x, threshold) && lhs.y.NearlyEqualsTo(rhs.y, threshold);
        }
        public static Vector3 TransformVelocityTowards(this Vector2 input, Transform towards, Transform origin)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            var inputForwardInWorldSpace = towards.forward;
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, origin.up)), 1f))
            {
                inputForwardInWorldSpace = -towards.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, origin.up);
            var forwardRotation = Quaternion.FromToRotation(origin.forward, inputForwardProjectedInWorldSpace);

            var translationInRigSpace = forwardRotation * new Vector3(input.x, 0f, input.y);
            var translationInWorldSpace = origin.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
        }
        public static Vector3 TransformVelocityTowards(this Vector3 input, Transform towards, Transform origin)
        {
            if (input == Vector3.zero)
                return Vector3.zero;

            var inputForwardInWorldSpace = towards.forward;
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, origin.up)), 1f))
            {
                inputForwardInWorldSpace = -towards.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, origin.up);
            var forwardRotation = Quaternion.FromToRotation(origin.forward, inputForwardProjectedInWorldSpace);

            var translationInRigSpace = forwardRotation * input;
            var translationInWorldSpace = origin.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
        }

        public static string LimitDecimal(this float number, int totalDigits, bool fixedLength = true)
        {
            var n = number.ToString();
            if (n.Length < totalDigits)
            {
                if (fixedLength)
                {
                    while (n.Length != totalDigits)
                    {
                        n += "0";
                    }
                }
                return n.ToString();
            }
            return number.ToString()[..totalDigits];
        }

        public static float NormalizeAngle(this float angle, float max = 180f)
        {
            angle = Mathf.Repeat(angle + max, 360f) + max - 360f;
            return angle;
        }

        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
    }
}
