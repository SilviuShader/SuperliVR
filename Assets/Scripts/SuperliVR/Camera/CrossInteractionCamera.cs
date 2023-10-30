using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;
using Utils;

namespace SuperliVR.Camera
{
    [RequireComponent(typeof(TrackedPoseDriver))]
    public class CrossInteractionCamera : MonoBehaviour
    {
        [SerializeField]
        private float             _defaultEyeHeight     = 2.0f;
        [SerializeField, Range(1f, 360f)]
        private float             _defaultRotationSpeed = 50.0f;
        [SerializeField, Range(-89f, 89f)]
        private float             _minVerticalAngle     = -89f, 
                                  _maxVerticalAngle     = 89f;

        private TrackedPoseDriver _trackedPoseDriver;

        private Vector2           _orbitAngles          = Vector2.zero;
        private Vector2           _orbitInput           = Vector2.zero;

        public void Orbit(InputAction.CallbackContext context)
        {
            var rawInput = context.ReadValue<Vector2>();
            _orbitInput = new Vector2(-rawInput.y, rawInput.x);
        }

        private void Awake()
        {
            _trackedPoseDriver = GetComponent<TrackedPoseDriver>();
            _trackedPoseDriver.trackingType         = TrackedPoseDriver.TrackingType.RotationAndPosition;
            _trackedPoseDriver.updateType           = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            _trackedPoseDriver.UseRelativeTransform = false;
        }

        private void OnValidate()
        {
            if (_maxVerticalAngle < _minVerticalAngle)
                _maxVerticalAngle = _minVerticalAngle;
        }

        private void Update()
        {
            if (!VRHelper.Instance.VRMode)
                RegularCameraUpdate();
        }

        private void RegularCameraUpdate()
        {
            Quaternion lookRotation;
            if (ManualRotation())
            {
                ConstrainAngles();
                lookRotation = Quaternion.Euler(_orbitAngles);
            }
            else
            {
                lookRotation = transform.localRotation;
            }
            
            transform.SetLocalPositionAndRotation(Vector2.up * _defaultEyeHeight, lookRotation);
        }

        private bool ManualRotation()
        {
            const float e = 0.001f;
            if (_orbitInput.x < -e || _orbitInput.x > e || _orbitInput.y < -e || _orbitInput.y > e)
            {
                _orbitAngles += _defaultRotationSpeed * Time.unscaledDeltaTime * _orbitInput;
                return true;
            }

            return false;
        }

        private void ConstrainAngles()
        {
            _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);

            if (_orbitAngles.y < 0f)
                _orbitAngles.y += 360f;
            else if (_orbitAngles.y >= 360.0f)
                _orbitAngles.y -= 360.0f;
        }
    }
}