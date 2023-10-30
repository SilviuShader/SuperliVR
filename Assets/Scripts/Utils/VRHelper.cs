using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace Utils
{
    [CreateAssetMenu(fileName = "VRHelper", menuName = "ScriptableObjects/VRHelper")]
    public class VRHelper : ScriptableObject
    {
        public         bool                   VRMode  => XRSettings.enabled;
                                                             
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