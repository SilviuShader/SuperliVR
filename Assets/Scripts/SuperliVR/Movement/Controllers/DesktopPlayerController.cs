using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace SuperliVR.Movement.Controllers
{
    [RequireComponent(typeof(PlayerInteractions), typeof(PlayerInput))]
    public class DesktopPlayerController : MonoBehaviour
    {
        private PlayerInteractions _playerInteractions;
        private Vector2            _movement;

        public void Move(InputAction.CallbackContext context) =>
            _movement = context.ReadValue<Vector2>();

        private void Awake() =>
            _playerInteractions = GetComponent<PlayerInteractions>();

        private void Update()
        {
            if (VRHelper.Instance.VRMode)
                return;

            _playerInteractions.Move(_movement);
        }
    }
}