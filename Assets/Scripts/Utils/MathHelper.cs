using UnityEngine;

namespace Utils
{
    public static class MathHelper
    {
        public static Vector3 ColorToVector3(Color col) => 
            new(col.r, col.g, col.b);

        public static Color Vector3ToColor(Vector3 vec) =>
            new(vec.x, vec.y, vec.z);
        
        public static Vector3 ReachValueSmooth(Vector3 from, Vector3 to, float centering, float alignmentDistance,
            float deltaTime)
        {
            var distanceBetween = Vector3.Distance(from, to);
            var t = 1.0f;

            if (distanceBetween > 0.01f && centering > 0.0f)
                t = Mathf.Pow(1.0f - centering, deltaTime);

            if (distanceBetween > alignmentDistance)
                t = Mathf.Min(t, alignmentDistance / distanceBetween);

            from = Vector3.Lerp(to, from, t);

            return from;
        }
    }
}
