using UnityEngine;

namespace SuperliVR.Picking
{
    [RequireComponent(typeof(Rigidbody))]
    public class PickableObject : MonoBehaviour
    {
        private Rigidbody _rigidbody;

        public void PickUp() =>
            _rigidbody.isKinematic = true;
        
        public void DropDown() =>
            _rigidbody.isKinematic = false;

        public void SetPosition(Vector3 position)
        {
            _rigidbody.position = position;
            transform.position = position;
        }

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();
    }
}