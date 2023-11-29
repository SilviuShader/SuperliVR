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

            var currentRadius = GetRadius(referencePos, ref maxDist);

            for (var i = 0; i < _distanceSearchSteps; i++)
            {
                if (Physics.CheckCapsule(rayOrigin + (direction * currentRadius),
                        rayOrigin + direction * (goToDist - currentRadius),
                        currentRadius * 0.5f, _sceneRaycastMask))
                {
                    maxDist = goToDist;
                    goToDist = (maxDist + minDist) * 0.5f;
                }
                else
                {
                    minDist = goToDist;
                    goToDist = (maxDist + minDist) * 0.5f;
                }

                currentRadius = GetRadius(referencePos,ref goToDist);
            }

            _currentScaleMultiplier = GetRadius(referencePos, ref goToDist);
            
            transform.localScale = _initialScale * _currentScaleMultiplier;
            transform.position = rayOrigin + goToDist * direction; 
        }

        private float GetRadius(ReferencePositionsInfo referencePositions, ref float distance)
        {
            var radius = 0.0f;
            var previousSubtract = 0.0f;
            for (var j = 0; j < _integrationSteps; j++)
            {
                radius = (WandDistanceToCameraDistance(referencePositions, distance) * _placedScaleMultiplier) / _pickUpDistance;
                var changedRadius = false;
                if (radius >= _maxScaleFactor)
                {
                    radius = _maxScaleFactor;
                    changedRadius = true;
                }

                if (radius < _minScaleFactor)
                {
                    radius = _minScaleFactor;
                    changedRadius = true;
                }

                if (changedRadius)
                {
                    var cameraDist = (radius * _pickUpDistance) / _placedScaleMultiplier;
                    distance = CameraDistanceToWandDistance(referencePositions, cameraDist); // TODO: Test if this works in VR
                }

                var currentSubtract = radius;
                distance += previousSubtract - currentSubtract;
                previousSubtract = currentSubtract;
            }

            Debug.Log(radius);

            return radius;
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
        }
    }
}