using UnityEngine;
using Valve.VR;

namespace SuperliVR.Camera
{
    public class CrossInteractionCamera : MonoBehaviour
    {
        //[SerializeField]
        //private SteamVR_Action_Boolean _headsetOnHead    = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
        
        //[SerializeField]
        //private float                  _defaultEyeHeight = 2.0f;

        private void Update()
        {
            /*
            if (OpenVR.System.GetTrackedDeviceActivityLevel(OpenVR.k_unTrackedDeviceIndex_Hmd) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction ||
                OpenVR.System.GetTrackedDeviceActivityLevel(OpenVR.k_unTrackedDeviceIndex_Hmd) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction_Timeout)
            {
            }
            else
            {
                RegularCameraUpdate();
            }*/
        }

        /*
        private void RegularCameraUpdate() =>
            transform.SetLocalPositionAndRotation(Vector2.up * _defaultEyeHeight, Quaternion.identity);
        */
    }
}