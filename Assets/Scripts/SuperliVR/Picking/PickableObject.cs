using UnityEngine;
using UnityEngine.UIElements;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Rigidbody), typeof(MeshRenderer))]
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

            gameObject.layer = _currentlyPickedLayer;
        }

        public void DropDown()
        {
            _rigidbody.isKinematic = false;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            _placedScale = _currentScale;

            gameObject.layer = _pickableLayer;

        }

        public void SetDirection(UnityEngine.Camera referenceCamera, Vector3 wandPosition, Vector3 direction)
        {
            var distance = 0.0f;
            var rayOrigin = referenceCamera.transform.position; // TODO: Use wand position here
            var currentPosition = rayOrigin;

            var anyRaycast = false;
            var raycasts = 10;

            for (var i = 0; i < raycasts + 1; i++)
            {
                if (i < raycasts)
                {
                    var hitDistance = 0.0f;
                    if (Physics.Raycast(currentPosition, direction, out var hit, _maxRaycastDistance,
                            _sceneRaycastMask))
                    {
                        hitDistance = hit.distance; // TODO: Change here when using the wand pos
                        anyRaycast = true;
                    }
                    distance += hitDistance;

                    var previousSubtract = 0.0f;
                    for (var j = 0; j < _integrationSteps; j++)
                    {
                        _currentScale = (distance * _placedScale) / _pickUpDistance;
                        var currentSubtract = _currentScale.x / 2.0f;
                        distance += previousSubtract - currentSubtract;
                        previousSubtract = currentSubtract;
                    }
                }
                else
                {
                    if (!anyRaycast)
                        distance = _pickUpDistance;
                }

                currentPosition =
                    rayOrigin + distance *
                    direction; // TODO: After wand position is used for way origin, then pythagorean theory must be used here
            }

            transform.localScale = _currentScale;
            
            transform.position = currentPosition; 
        }

        private void Awake()
        {
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