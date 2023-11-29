using System.Numerics;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
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
        private float        _maxRaycastDistance = 100.0f;
        [SerializeField, Min(1)]
        private int          _integrationSteps   = 10;

        private Vector3      _initialScale;
        private Vector3      _placedScale;
        private Vector3      _currentScale;

        private Collider     _collider;
        private Rigidbody    _rigidbody;
        private MeshRenderer _meshRenderer;

        private LayerMask    _pickableLayer;
        private LayerMask    _currentlyPickedLayer;

        private float        _pickUpDistance;

        public void PickUp(float pickUpDistance)
        {
            _rigidbody.isKinematic = true;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _currentScale = _placedScale;
            _pickUpDistance = pickUpDistance;
            _collider.isTrigger = true;

            gameObject.layer = _currentlyPickedLayer;
        }

        public void DropDown()
        {
            _rigidbody.isKinematic = false;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            _placedScale = _currentScale;
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
            var raycasts = 20;

            _currentScale = _initialScale;

            _currentScale = 0.01f * Vector3.one;
            
            var maxDist = _maxRaycastDistance;
            if (Physics.Raycast(rayOrigin, direction, out var hit, _maxRaycastDistance, _sceneRaycastMask))
                maxDist = hit.distance; // TODO: Change this when chaning to origin of wand.

            var epsilon = 0.01f;

            var minDist = 0.0f;
            var goToDist = (minDist + maxDist) * 0.5f;

            var currentRadius = GetRadius(referencePos, ref maxDist);

            for (var i = 0; i < raycasts; i++)
            {
                if (Physics.CheckCapsule(rayOrigin + (direction * currentRadius),
                        rayOrigin + direction * (goToDist - epsilon - currentRadius),
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
            
            currentRadius = GetRadius(referencePos, ref goToDist);
            
            _currentScale = Vector3.one * currentRadius;
            
            transform.localScale = _currentScale;
            transform.position = rayOrigin + goToDist * direction; 
        }

        private float GetRadius(ReferencePositionsInfo referencePositions, ref float distance)
        {
            var radius = 0.0f;
            var previousSubtract = 0.0f;
            for (var j = 0; j < _integrationSteps; j++)
            {
                radius = (WandDistanceToCameraDistance(referencePositions, distance) * _placedScale.x) / _pickUpDistance;
                var currentSubtract = radius;
                distance += previousSubtract - currentSubtract;
                previousSubtract = currentSubtract;
            }

            return radius;
        }

        static float WandDistanceToCameraDistance(
            ReferencePositionsInfo referencePositions,
            float distance)
        {
            var worldPoint = referencePositions.WandOrigin + referencePositions.WandDirection * distance;
            var cameraToPoint = worldPoint - referencePositions.CameraOrigin;
            return Vector3.Dot(cameraToPoint, referencePositions.CameraForward);
        }

        private void Awake()
        {
            _collider     = GetComponent<Collider>();
            _rigidbody    = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _pickableLayer = LayerMask.NameToLayer("Pickable");
            _currentlyPickedLayer = LayerMask.NameToLayer("CurrentlyPicked");

            _sceneRaycastMask &= ~(1 << _currentlyPickedLayer);

            _currentScale = _placedScale = _initialScale = transform.localScale;
            gameObject.layer = _pickableLayer;
        }
    }
}