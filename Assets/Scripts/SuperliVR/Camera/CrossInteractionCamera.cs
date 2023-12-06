using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;
using Utils;
using Valve.VR;

namespace SuperliVR.Camera
{
    [RequireComponent(typeof(TrackedPoseDriver))]
    public class CrossInteractionCamera : MonoBehaviour
    {
        [SerializeField]
        private float                  _defaultEyeHeight       = 2.0f;
        [SerializeField, Range(1f, 360f)]                      
        private float                  _defaultRotationSpeed   = 50.0f;
        [SerializeField, Range(1f, 360f)]
        private float                  _vrRotationSpeed        = 50.0f;
        [SerializeField, Range(-89f, 89f)]                     
        private float                  _minVerticalAngle       = -89f, 
                                       _maxVerticalAngle       = 89f;
        [SerializeField]
        private Transform              _rootParent             = default;
        [SerializeField]
        private Transform              _aim                    = null;

        // TODO: Wrap this
        [SerializeField]
        private SteamVR_Action_Vector2 _joystickMovementAction = SteamVR_Input.GetVector2Action("JoystickMovement");

        private TrackedPoseDriver      _trackedPoseDriver;
                                       
        private Vector2                _orbitAngles            = Vector2.zero;
        private Vector2                _orbitInput             = Vector2.zero;

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
            else
                VRCameraUpdate();
        }

        private void RegularCameraUpdate()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

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
            _aim.gameObject.SetActive(true);
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

        private void VRCameraUpdate()
        {
            var axis = _joystickMovementAction[SteamVR_Input_Sources.RightHand].axis;
            _rootParent.Rotate(Vector3.up, axis.x * _vrRotationSpeed * Time.deltaTime);
            _aim.gameObject.SetActive(false);
        }
    }
}