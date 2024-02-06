using System;
using System.Linq;
using UnityEngine;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public class PickableObject : MonoBehaviour
    {
        private class ReferencePositionsInfo
        {
            public Vector3 CameraOrigin;
            public Vector3 CameraForward;
            public Vector3 WandOrigin;
            public Vector3 WandDirection;
        }

        public  float          CurrentScaleMultiplier => _currentScaleMultiplier;
        public  float          PlacedScaleMultiplier => _placedScaleMultiplier;

        [SerializeField]
        private LayerMask      _sceneRaycastMask   = -1;
        [SerializeField]
        private float          _maxRaycastDistance  = 100.0f;
        [SerializeField, Min(1)]
        private int            _integrationSteps    = 20;
        [SerializeField]       
        private int            _distanceSearchSteps = 20;
        [SerializeField]
        private float          _maxScaleFactor      = 500.0f;
        [SerializeField]
        private float          _minScaleFactor      = 0.1f;
 
        private float          _workingMaxScaleFactor;
        private float          _workingMinScaleFactor;
                               
        private Vector3        _initialWorkingScale;
        private Vector3        _workingScale;
        private float          _placedScaleMultiplier;
        private float          _currentScaleMultiplier;
        private Vector3        _previousPickUpDirection;
                               
        private Collider       _collider;
        private Rigidbody      _rigidbody;
        private MeshRenderer[] _meshRenderers;

        private LayerMask      _pickableLayer;
        private LayerMask      _currentlyPickedLayer;
                               
        private float          _pickUpDistance;
        private float          _initialBoundingRadius;

        public void PickUp(UnityEngine.Camera referenceCamera)
        {
            _rigidbody.isKinematic = true;
            foreach (var meshRenderer in _meshRenderers)
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _currentScaleMultiplier = _placedScaleMultiplier;
            _previousPickUpDirection = ProjectXZPlane(referenceCamera.transform.forward);
            _pickUpDistance = Mathf.Max(_placedScaleMultiplier, PointToCameraDistance(transform.position, referenceCamera.transform.position, _previousPickUpDirection));
            _collider.isTrigger = true;
            
            gameObject.layer = _currentlyPickedLayer;
        }

        public void DropDown()
        {
            _rigidbody.isKinematic = false;
            foreach (var meshRenderer in _meshRenderers)
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            _placedScaleMultiplier = _currentScaleMultiplier;
            _collider.isTrigger = false;

            gameObject.layer = _pickableLayer;
            
            ResetState(); 

            _workingMaxScaleFactor = (_maxScaleFactor / _initialWorkingScale.x) * _workingScale.x;
            _workingMinScaleFactor = (_minScaleFactor / _initialWorkingScale.x) * _workingScale.x;
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

            var currentScaleMultiplier = GetScaleMultiplier(referencePos, ref goToDist);

            for (var i = 0; i < _distanceSearchSteps; i++)
            {
                var realSize = currentScaleMultiplier * _initialBoundingRadius;

                if (Physics.CheckCapsule(rayOrigin + (direction * realSize),
                        rayOrigin + direction * (goToDist - realSize),
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

                currentScaleMultiplier = GetScaleMultiplier(referencePos, ref goToDist);
            }

            _currentScaleMultiplier = GetScaleMultiplier(referencePos, ref goToDist);
            
            transform.localScale = _workingScale * _currentScaleMultiplier;
            transform.position = rayOrigin + goToDist * direction;

            var currentDirection = ProjectXZPlane(direction);

            var deltaRotation = Quaternion.FromToRotation(_previousPickUpDirection, currentDirection);

            transform.rotation = deltaRotation * transform.rotation;
            _previousPickUpDirection = currentDirection;
        }

        private float GetScaleMultiplier(ReferencePositionsInfo referencePositions, ref float distance)
        {
            var scaleMultiplier = 0.0f;
            var previousSubtract = 0.0f;
            for (var j = 0; j < _integrationSteps; j++)
            {
                scaleMultiplier = (WandDistanceToCameraDistance(referencePositions, distance) * _placedScaleMultiplier) / _pickUpDistance;
                var changedRadius = false;
                if (scaleMultiplier >= _workingMaxScaleFactor)
                {
                    scaleMultiplier = _workingMaxScaleFactor;
                    changedRadius = true;
                }

                if (scaleMultiplier < _workingMinScaleFactor)
                {
                    scaleMultiplier = _workingMinScaleFactor;
                    changedRadius = true;
                }

                if (changedRadius)
                {
                    var cameraDist = (scaleMultiplier * _pickUpDistance) / _placedScaleMultiplier;
                    distance = CameraDistanceToWandDistance(referencePositions, cameraDist);
                }

                var currentSubtract = scaleMultiplier * _initialBoundingRadius * 0.5f;
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
            _collider      = GetComponent<Collider>();
            _rigidbody     = GetComponent<Rigidbody>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();

            var currentMeshRenderer = GetComponent<MeshRenderer>();
            if (currentMeshRenderer != null)
                _meshRenderers = _meshRenderers.Concat(new[] { currentMeshRenderer }).ToArray();
            
            _pickableLayer = LayerMask.NameToLayer("Pickable");
            _currentlyPickedLayer = LayerMask.NameToLayer("CurrentlyPicked");

            _sceneRaycastMask &= ~(1 << _currentlyPickedLayer);

            _workingMaxScaleFactor = _maxScaleFactor;
            _workingMinScaleFactor = _minScaleFactor;

            _initialWorkingScale = transform.localScale;
            ResetState();
        }

        private void Update()
        {
            if (_rigidbody.isKinematic)
                return;

            if (transform.hasChanged)
            {
                Awake();
            }
        }

        private void ResetState()
        {
            _workingScale = transform.localScale;
            _placedScaleMultiplier = _currentScaleMultiplier = 1.0f;
            gameObject.layer = _pickableLayer;
            _initialBoundingRadius = ScaleHelper.ObjectBoundingRadius(_collider);

            _previousPickUpDirection = Vector3.forward;
        }

        private Vector3 ProjectXZPlane(Vector3 vec)
        {
            vec.y = 0.0f;
            var length = vec.magnitude;
            if (length > Mathf.Epsilon)
                return vec / length;
            
            return Vector3.forward;
        }
    }
}