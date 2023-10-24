using UnityEngine;
using Valve.VR;

namespace Utils
{
    [CreateAssetMenu(fileName = "VRHelper", menuName = "ScriptableObjects/VRHelper")]
    public class VRHelper : ScriptableObject
    {
        // TODO: Make this work properly
        //public         bool                   HeadsetOnHead  => SteamVR_Actions.default_HeadsetOnHead.GetStateDown(SteamVR_Input_Sources.Head);
                                                             
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

        private static VRHelper               _instance;
    }
}