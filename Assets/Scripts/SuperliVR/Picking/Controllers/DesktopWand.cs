using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Valve.VR;
using static UnityEngine.GraphicsBuffer;

namespace SuperliVR.Picking.Controllers
{
    [RequireComponent(typeof(WandPicker))]
    public class DesktopWand : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Camera _playerCamera        = null;
        [SerializeField, Min(0.1f)]                     
        private float              _distanceFromCamera  = 0.1f;
        [SerializeField]                                
        private Vector2            _offset              = Vector2.zero;
        [SerializeField]
        private Vector3            _defaultWandRotation = Vector3.zero;
        [SerializeField, Range(0.1f, 360.0f)]
        private float              _wandRotationSpeed   = 90.0f;

        private Quaternion         _desiredBaseRotation = Quaternion.identity;
        private Quaternion         _baseRotation        = Quaternion.identity;

        private WandPicker         _picker;
        private bool               _pickDown;

        public void OnPick(InputAction.CallbackContext context) =>
            _pickDown = context.ReadValueAsButton();

        private void Awake() =>
            _picker = GetComponent<WandPicker>();

        private void LateUpdate()
        {
            if (VRHelper.Instance.VRMode)
                return;

            if (!_picker.CurrentlyPicking)
                _picker.CheckPick(_pickDown,
                    _playerCamera.transform.position,
                    _playerCamera.transform.forward);

            if (_picker.CurrentlyPicking)
                _picker.CurrentlyPickingUpdate(_pickDown, _playerCamera.transform.position, _playerCamera.transform.forward);

            var cameraTransform = _playerCamera.transform;
            var baseWorldPos = _playerCamera.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, _distanceFromCamera));
            baseWorldPos += cameraTransform.right * _offset.x + cameraTransform.up * _offset.y;

            UpdateDesiredBaseRotation(baseWorldPos);

            _baseRotation =
            Quaternion.RotateTowards(_baseRotation, 
                _desiredBaseRotation, 
                _wandRotationSpeed * Time.deltaTime);
            
            var rotation = cameraTransform.rotation * _baseRotation;
            transform.SetPositionAndRotation(baseWorldPos, rotation);
        }

        private void UpdateDesiredBaseRotation(Vector3 baseWorldPos)
        {
            if (_pickDown)
            {
                var cameraTransform = _playerCamera.transform;

                var target = cameraTransform.position + cameraTransform.forward * 2.0f * _distanceFromCamera;
                var lookDirection = (target - baseWorldPos).normalized;

                _desiredBaseRotation = Quaternion.Inverse(cameraTransform.rotation) *
                                       Quaternion.LookRotation(lookDirection, cameraTransform.up);
            }
            else
            {
                _desiredBaseRotation = Quaternion.Euler(_defaultWandRotation);
            }
        }
    }
}