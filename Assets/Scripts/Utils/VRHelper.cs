using UnityEngine;
using Valve.VR;

namespace Utils
{
    [CreateAssetMenu(fileName = "VRHelper", menuName = "ScriptableObjects/VRHelper")]
    public class VRHelper : ScriptableObject
    {
        public         bool                   HeadsetOnHead  => _headsetOnHead.GetStateDown(SteamVR_Input_Sources.Head);
                                                             
        public  static VRHelper               Instance 
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<VRHelper>("VRHelper");

                if (_instance == null)
                    _instance = new VRHelper();

                return _instance;
            }
        }

        [SerializeField]
        private        SteamVR_Action_Boolean _headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");

        private static VRHelper               _instance;
    }
}