using UnityEngine;
using Utils;
using Valve.VR;

namespace SuperliVR.Picking.Controllers
{
    [RequireComponent(typeof(WandPicker))]
    public class VRWand : MonoBehaviour
    {
        private Vector3                ForwardDirection => transform.forward;
        // TODO: Wrap this..
        [SerializeField]
        private SteamVR_Action_Boolean _pickObjectAction = SteamVR_Input.GetBooleanAction("PickObject");

        private WandPicker             _picker;

        private void Awake() =>
            _picker = GetComponent<WandPicker>();

        private void Update()
        {
            if (!VRHelper.Instance.VRMode)
                return;

            if (!_picker.CurrentlyPicking)
                _picker.CheckPick(
                    _pickObjectAction[SteamVR_Input_Sources.RightHand].state, 
                    transform.position, 
                    ForwardDirection);
            
            if (_picker.CurrentlyPicking)
                _picker.CurrentlyPickingUpdate(_pickObjectAction[SteamVR_Input_Sources.RightHand].state,
                    ForwardDirection);
        }
    }
}