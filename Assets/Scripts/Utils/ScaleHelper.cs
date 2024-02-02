using UnityEngine;

namespace Utils
{
    static class ScaleHelper
    {
        public static float ObjectBoundingRadius(Collider collider)
        {
            var worldBounds = collider.bounds;
            var worldExtents = worldBounds.extents;
            var objectExtent = MaxComponent(worldExtents);

            return objectExtent;
        }

        public static float MaxComponent(Vector3 vec)
        {
            if (vec.x > vec.y && vec.x > vec.z)
                return vec.x;

            if (vec.y > vec.x && vec.y > vec.z)
                return vec.y;

            return vec.z;
        }

        private static float MinComponent(Vector3 vec)
        {
            if (vec.x < vec.y && vec.x < vec.z)
                return vec.x;

            if (vec.y < vec.x && vec.y < vec.z)
                return vec.y;

            return vec.z;
        }
    }
}
