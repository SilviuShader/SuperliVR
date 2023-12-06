using UnityEngine;
using Utils;
using Valve.VR;

namespace SuperliVR.Picking.Controllers
{
    [RequireComponent(typeof(WandPicker))]
    public class VRWand : MonoBehaviour
    {
        private Vector3                ForwardDirection    => transform.forward;
        // TODO: Wrap this..
        [SerializeField]
        private SteamVR_Action_Boolean _pickObjectAction   = SteamVR_Input.GetBooleanAction("PickObject");
        [SerializeField]
        private Transform              _directionIndicator;

        private WandPicker             _picker;

        private void Awake() =>
            _picker = GetComponent<WandPicker>();

        private void LateUpdate()
        {
            if (!VRHelper.Instance.VRMode)
                return;

            var pickPressed = _pickObjectAction[SteamVR_Input_Sources.RightHand].state;

            if (!_picker.CurrentlyPicking)
                _picker.CheckPick(
                    pickPressed, 
                    transform.position, 
                    ForwardDirection);
            
            if (_picker.CurrentlyPicking)
                _picker.CurrentlyPickingUpdate(
                    pickPressed,
                    transform.position,
                    ForwardDirection);

            if (pickPressed && !_picker.CurrentlyPicking)
                ShowPickDirection();
            else
                HidePickDirection();
        }

        private void ShowPickDirection()
        {
            _directionIndicator.gameObject.SetActive(true);

            var targetScale = _picker.MaxPickingDist;

            if (Physics.Raycast(transform.position, ForwardDirection, out var hit, _picker.MaxPickingDist, _picker.RegularSceneLayers))
                targetScale = hit.distance;

            targetScale *= 0.5f;

            _directionIndicator.localScale = new Vector3(_directionIndicator.localScale.x, targetScale,
                _directionIndicator.localScale.z);

            _directionIndicator.localPosition = Vector3.forward * targetScale;
        }

        private void HidePickDirection()
        {
            _directionIndicator.gameObject.SetActive(false);
        }
    }
}