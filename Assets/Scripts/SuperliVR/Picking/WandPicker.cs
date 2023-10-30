using UnityEngine;

namespace SuperliVR.Picking
{
    public class WandPicker : MonoBehaviour
    {
        public  Rigidbody PickedObject
        {
            get => _pickedObject;
            set
            {
                if (_pickedObject != null)
                {
                    _pickedObject.isKinematic = false;
                }

                _pickedObject = value;
                _pickedObject.isKinematic = true;
            }
        }

        public  Vector3   Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        }

        [SerializeField]
        private float     _distanceFromWand = 2.0f;
        
        private Rigidbody _pickedObject;
        private Vector3   _direction        = Vector3.forward;

        private void Update()
        {
            if (PickedObject == null)
                return;

            PickedObject.position = transform.position + Direction * _distanceFromWand;
        }
    }
}