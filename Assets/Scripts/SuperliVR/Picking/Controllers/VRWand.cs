using UnityEngine;
using Valve.VR;

namespace SuperliVR.Picking.Controllers
{
    [RequireComponent(typeof(WandPicker))]
    public class VRWand : MonoBehaviour
    {
        private Vector3                ForwardDirection => transform.up;
                                       
        [SerializeField]               
        private float                  _maxPickingDist  = 10.0f;
        [SerializeField]
        private LayerMask              _layerMask       = -1;
        // TODO: Wrap this..
        [SerializeField]
        private SteamVR_Action_Boolean _pickObjectAction = SteamVR_Input.GetBooleanAction("PickObject");

        private WandPicker             _picker;

        private void Awake() =>
            _picker = GetComponent<WandPicker>();

        private void Update()
        {
            if (!_picker.CurrentlyPicking)
                CheckPick();
            if (_picker.CurrentlyPicking)
                CurrentlyPickingUpdate();
        }

        private void CheckPick()
        {
            if (!_pickObjectAction[SteamVR_Input_Sources.RightHand].state)
                return;
            
            var dir = ForwardDirection;

            if (Physics.Raycast(transform.position, dir, out var hit, _maxPickingDist, _layerMask))
            {
                // TODO: create a script for the pickable objects that require the rigidbody component
                var rigidbody = hit.transform.GetComponent<Rigidbody>();
                if (rigidbody != null)
                    _picker.PickedObject = rigidbody;
            }

            _picker.Direction = dir;
        }

        private void CurrentlyPickingUpdate()
        {
            if (!_pickObjectAction[SteamVR_Input_Sources.RightHand].state)
            {
                _picker.PickedObject = null;
                return;
            }

            _picker.Direction = transform.up;
        }
    }
}