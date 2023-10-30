using UnityEngine;

namespace SuperliVR.Picking.Controllers
{
    [RequireComponent(typeof(WandPicker))]
    public class VRWand : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody  _testPickedObject;

        private WandPicker _picker;

        private void Awake() =>
            _picker = GetComponent<WandPicker>();

        private void Update()
        {
            _picker.PickedObject = _testPickedObject;
            _picker.Direction    = transform.up;
        }
    }
}