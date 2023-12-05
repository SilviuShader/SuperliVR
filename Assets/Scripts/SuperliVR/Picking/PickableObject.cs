using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(MeshRenderer))]
    public class PickableObject : MonoBehaviour
    {
        private class ReferencePositionsInfo
        {
            public Vector3 CameraOrigin;
            public Vector3 CameraForward;
            public Vector3 WandOrigin;
            public Vector3 WandDirection;
        }

        [SerializeField]
        private LayerMask    _sceneRaycastMask   = -1;
        [SerializeField]
        private float        _maxRaycastDistance  = 100.0f;
        [SerializeField, Min(1)]
        private int          _integrationSteps    = 20;
        [SerializeField]
        private int          _distanceSearchSteps = 20;
        [SerializeField]
        private float        _maxScaleFactor      = 100.0f;
        [SerializeField]
        private float        _minScaleFactor      = 0.1f;

        private Vector3      _initialScale;
        private float        _placedScaleMultiplier;
        private float        _currentScaleMultiplier;
        
        private Collider     _collider;
        private Rigidbody    _rigidbody;
        private MeshRenderer _meshRenderer;

        private LayerMask    _pickableLayer;
        private LayerMask    _currentlyPickedLayer;

        private float        _pickUpDistance;
        private float        _initialBoundingRadius;

        private float        ObjectBoundingRadius
        {
            get
            {
                var worldBounds = _collider.bounds;
                var worldExtents = worldBounds.extents;
                var objectExtent = MaxComponent(worldExtents) / MinComponent(transform.localScale);

                return objectExtent;
            }
        }

        public void PickUp(UnityEngine.Camera referenceCamera)
        {
            _rigidbody.isKinematic = true;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _currentScaleMultiplier = _placedScaleMultiplier;
            _pickUpDistance = Mathf.Max(_placedScaleMultiplier, PointToCameraDistance(transform.position, referenceCamera.transform.position, referenceCamera.transform.forward));
            _collider.isTrigger = true;

            gameObject.layer = _currentlyPickedLayer;
        }

        public void DropDown()
        {
            _rigidbody.isKinematic = false;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            _placedScaleMultiplier = _currentScaleMultiplier;
            _collider.isTrigger = false;

            gameObject.layer = _pickableLayer;

        }

        public void SetDirection(UnityEngine.Camera referenceCamera, Vector3 wandPosition, Vector3 direction)
        {
            var referencePos = new ReferencePositionsInfo
            {
                CameraForward = referenceCamera.transform.forward,
                CameraOrigin  = referenceCamera.transform.position,
                WandDirection = direction,
                WandOrigin    = wandPosition
            };

            direction = referencePos.WandDirection;
            var rayOrigin = referencePos.WandOrigin;

            _currentScaleMultiplier = _placedScaleMultiplier;

            var maxDist = _maxRaycastDistance;
            if (Physics.Raycast(rayOrigin, direction, out var hit, _maxRaycastDistance, _sceneRaycastMask))
                maxDist = hit.distance;

            var minDist = 0.0f;
            var goToDist = (minDist + maxDist) * 0.5f;

            var currentScaleMultiplier = GetScaleMultiplier(referencePos, ref maxDist);

            for (var i = 0; i < _distanceSearchSteps; i++)
            {
                var realSize = currentScaleMultiplier * _initialBoundingRadius * MaxComponent(_initialScale);
                
                if (Physics.CheckCapsule(rayOrigin + (direction * realSize * 2.0f),
                        rayOrigin + direction * (goToDist - realSize * 2.0f),
                        realSize, _sceneRaycastMask))
                {
                    maxDist = goToDist;
                    goToDist = (maxDist + minDist) * 0.5f;
                }
                else
                {
                    minDist = goToDist;
                    goToDist = (maxDist + minDist) * 0.5f;
                }

                currentScaleMultiplier = GetScaleMultiplier(referencePos,ref goToDist);
            }

            _currentScaleMultiplier = GetScaleMultiplier(referencePos, ref goToDist);
            
            transform.localScale = _initialScale * _currentScaleMultiplier;
            transform.position = rayOrigin + goToDist * direction; 
        }

        private float GetScaleMultiplier(ReferencePositionsInfo referencePositions, ref float distance)
        {
            var scaleMultiplier = 0.0f;
            var previousSubtract = 0.0f;
            for (var j = 0; j < _integrationSteps; j++)
            {
                scaleMultiplier = (WandDistanceToCameraDistance(referencePositions, distance) * _placedScaleMultiplier) / _pickUpDistance;
                var changedRadius = false;
                if (scaleMultiplier >= _maxScaleFactor)
                {
                    scaleMultiplier = _maxScaleFactor;
                    changedRadius = true;
                }

                if (scaleMultiplier < _minScaleFactor)
                {
                    scaleMultiplier = _minScaleFactor;
                    changedRadius = true;
                }

                if (changedRadius)
                {
                    var cameraDist = (scaleMultiplier * _pickUpDistance) / _placedScaleMultiplier;
                    distance = CameraDistanceToWandDistance(referencePositions, cameraDist);
                }

                var currentSubtract = scaleMultiplier * MaxComponent(_initialScale) * _initialBoundingRadius * 2.0f;
                distance += previousSubtract - currentSubtract;
                previousSubtract = currentSubtract;
            }
            
            return scaleMultiplier;
        }

        static float WandDistanceToCameraDistance(
            ReferencePositionsInfo referencePositions,
            float distance)
        {
            var worldPoint = referencePositions.WandOrigin + referencePositions.WandDirection * distance;
            return PointToCameraDistance(worldPoint, referencePositions.CameraOrigin, referencePositions.CameraForward);
        }

        static float CameraDistanceToWandDistance(ReferencePositionsInfo referencePositions,
            float distance)
        {
            var worldPoint = referencePositions.CameraOrigin + referencePositions.CameraForward * distance;
            var pointToWand = worldPoint - referencePositions.WandOrigin;
            return Mathf.Abs(Vector3.Dot(pointToWand, referencePositions.WandDirection));
        }

        static float PointToCameraDistance(Vector3 point, Vector3 cameraPosition, Vector3 cameraForward)
        {
            var cameraToPoint = point - cameraPosition;
            return Mathf.Abs(Vector3.Dot(cameraToPoint, cameraForward));
        }

        private void Awake()
        {
            _collider     = GetComponent<Collider>();
            _rigidbody    = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _pickableLayer = LayerMask.NameToLayer("Pickable");
            _currentlyPickedLayer = LayerMask.NameToLayer("CurrentlyPicked");

            _sceneRaycastMask &= ~(1 << _currentlyPickedLayer);

            _initialScale = transform.localScale;
            _placedScaleMultiplier = _currentScaleMultiplier = 1.0f;
            gameObject.layer = _pickableLayer;
            _initialBoundingRadius = ObjectBoundingRadius;
        }

        private float MaxComponent(Vector3 vec)
        {
            if (vec.x > vec.y && vec.x > vec.z)
                return vec.x;

            if (vec.y > vec.x && vec.y > vec.z)
                return vec.y;

            return vec.z;
        }

        private float MinComponent(Vector3 vec)
        {
            if (vec.x < vec.y && vec.x < vec.z)
                return vec.x;

            if (vec.y < vec.x && vec.y < vec.z)
                return vec.y;

            return vec.z;
        }
    }
}