using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Valve.VR;

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
        [SerializeField]
        private float              _maxPickingDist      = 10.0f;
        [SerializeField]                                
        private LayerMask          _layerMask           = -1;

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

            var cameraTransform = _playerCamera.transform;
            var baseWorldPos = _playerCamera.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, _distanceFromCamera));
            baseWorldPos += cameraTransform.right * _offset.x + cameraTransform.up * _offset.y;

            if (!_picker.CurrentlyPicking)
                CheckPick();
            if (_picker.CurrentlyPicking)
                CurrentlyPickingUpdate(baseWorldPos);

            _baseRotation =
            Quaternion.RotateTowards(_baseRotation, 
                _desiredBaseRotation, 
                _wandRotationSpeed * Time.deltaTime);
            
            var rotation = cameraTransform.rotation * _baseRotation;
            transform.SetPositionAndRotation(baseWorldPos, rotation);
        }

        private void CheckPick()
        {
            _desiredBaseRotation =  Quaternion.Euler(_defaultWandRotation);

            if (!_pickDown)
                return;

            var cameraTransform = _playerCamera.transform;

            var dir = cameraTransform.forward;
            if (Physics.Raycast(cameraTransform.position, dir, out var hit, _maxPickingDist, _layerMask))
            {
                Debug.Log(hit.transform.name);

                // TODO: create a script for the pickable objects that require the rigidbody component
                var rigidbody = hit.transform.GetComponent<Rigidbody>();
                if (rigidbody != null)
                    _picker.PickedObject = rigidbody;
            }

            _picker.Direction = dir;
        }

        private void CurrentlyPickingUpdate(Vector3 baseWorldPod)
        {
            if (!_pickDown)
            {
                _picker.PickedObject = null;
                return;
            }

            var cameraTransform = _playerCamera.transform;

            _picker.Direction = cameraTransform.forward;

            var target = cameraTransform.position + cameraTransform.forward * 2.0f * _distanceFromCamera;
            var lookDirection = (target - baseWorldPod).normalized;

            _desiredBaseRotation = Quaternion.Inverse(cameraTransform.rotation) * 
                                   Quaternion.LookRotation(lookDirection, cameraTransform.up);
        }
    }
}