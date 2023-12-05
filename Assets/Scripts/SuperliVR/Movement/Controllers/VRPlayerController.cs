using UnityEngine;
using Valve.VR;

using Utils;

namespace SuperliVR.Movement.Controllers
{
    [RequireComponent(typeof(PlayerInteractions))]
    public class VRPlayerController : MonoBehaviour
    {
        // TODO: Wrap this
        [SerializeField]
        private SteamVR_Action_Vector2 _joystickMovementAction = SteamVR_Input.GetVector2Action("JoystickMovement");
        [SerializeField]
        private SteamVR_Action_Boolean _jumpAction             = SteamVR_Input.GetBooleanAction("Jump");

        private PlayerInteractions     _playerInteractions;

        private bool                   _previousJumpState;

        private void Awake() =>
            _playerInteractions = GetComponent<PlayerInteractions>();

        private void Update()
        {
            if (!VRHelper.Instance.VRMode)
                return;

            var axis = _joystickMovementAction[SteamVR_Input_Sources.LeftHand].axis;
            var jumpState = _jumpAction[SteamVR_Input_Sources.LeftHand].state;

            _playerInteractions.Move(axis);

            if (jumpState && !_previousJumpState)
                _playerInteractions.Jump();
            
            _previousJumpState = jumpState;
        }
    }
}