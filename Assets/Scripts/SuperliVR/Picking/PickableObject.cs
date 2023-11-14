using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Vector3 = UnityEngine.Vector3;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(MeshRenderer))]
    public class PickableObject : MonoBehaviour
    {
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
            direction = referenceCamera.transform.forward;
            var rayOrigin = referenceCamera.transform.position; // TODO: Use wand position here
            var raycasts = 10;

            _currentScale = _initialScale;

            _currentScale = 0.01f * Vector3.one;

            string message = "";

            /*
            if(Physics.CheckCapsule())

            for (var i = 0; i < raycasts; i++)
            {
                message += _currentScale.x + " ";
                if (Physics.CheckCapsule(rayOrigin, rayOrigin + direction * _maxRaycastDistance))
                {
                    message += "(" + hit.collider.name + ")";
                    distance = hit.distance;
                }
                else
                {
                    message += "(pula)";
                }

                _currentScale = (distance * _placedScale) / _pickUpDistance;
            }

            Debug.Log(message);

            */

            var maxDist = _maxRaycastDistance;
            if (Physics.Raycast(rayOrigin, direction, out var hit, _maxRaycastDistance, _sceneRaycastMask))
                maxDist = hit.distance; // TODO: Change this when chaning to origin of wand.

            var smallRadius = 0.0f;
            var bigRadius = GetRadius(ref maxDist);

            Debug.Log(smallRadius + " " + bigRadius);

            var epsilon = 0.01f;

            var currentRadius = (smallRadius + bigRadius) * 0.5f;
            var goToDist = maxDist * 0.9f;

            for (var i = 0; i < raycasts; i++)
            {
                if (Physics.CheckCapsule(rayOrigin + (direction * currentRadius), rayOrigin + direction * (goToDist - epsilon - currentRadius),
                        currentRadius * 0.5f, _sceneRaycastMask))
                {
                    bigRadius = currentRadius;
                    currentRadius = (bigRadius + smallRadius) / 2.0f;
                }
                else
                {
                    smallRadius = currentRadius;
                    currentRadius = (bigRadius + smallRadius) / 2.0f;
                }
            }

            goToDist = (currentRadius * _pickUpDistance) / _placedScale.x;
            _currentScale = Vector3.one * currentRadius;
            
            transform.localScale = _currentScale;
            transform.position = rayOrigin + goToDist * direction; 
        }

        private float GetRadius(ref float distance)
        {
            var radius = 0.0f;
            var previousSubtract = 0.0f;
            for (var j = 0; j < _integrationSteps; j++)
            {
                radius = (distance * _placedScale.x) / _pickUpDistance;
                var currentSubtract = radius;
                distance += previousSubtract - currentSubtract;
                previousSubtract = currentSubtract;
            }

            return radius;
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