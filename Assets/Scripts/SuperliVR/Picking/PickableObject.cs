using UnityEngine;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Rigidbody), typeof(MeshRenderer))]
    public class PickableObject : MonoBehaviour
    {
        private Rigidbody    _rigidbody;
        private MeshRenderer _meshRenderer;

        public void PickUp()
        {
            _rigidbody.isKinematic = true;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void DropDown()
        {
            _rigidbody.isKinematic = false;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        public void SetPosition(Vector3 position)
        {
            _rigidbody.position = position;
            transform.position  = position;
        }

        private void Awake()
        {
            _rigidbody    = GetComponent<Rigidbody>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }
    }
}