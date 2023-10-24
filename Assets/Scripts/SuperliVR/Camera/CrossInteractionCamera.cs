using UnityEngine;

using Utils;

namespace SuperliVR.Camera
{
    public class CrossInteractionCamera : MonoBehaviour
    {
        [SerializeField]
        private float _defaultEyeHeight = 2.0f;

        private void Update()
        {
            //if (!VRHelper.Instance.HeadsetOnHead)
                RegularCameraUpdate();
        }

        private void RegularCameraUpdate() =>
            transform.SetLocalPositionAndRotation(Vector2.up * _defaultEyeHeight, Quaternion.identity);
    }
}