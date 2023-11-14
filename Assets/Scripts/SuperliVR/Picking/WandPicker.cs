using UnityEngine;

namespace SuperliVR.Picking
{
    public class WandPicker : MonoBehaviour
    {
        public  PickableObject     PickedObject
        {
            get => _pickedObject;
            set
            {
                if (_pickedObject != null)
                    _pickedObject.DropDown();
                
                _pickedObject = value;

                if (_pickedObject != null)
                    _pickedObject.PickUp(
                        Vector3.Distance(
                            _referenceCamera.transform.position, 
                            _pickedObject.transform.position));
            }
        }

        public  Vector3            Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        }

        public  bool               CurrentlyPicking  => PickedObject != null;
                                   
        [SerializeField]           
        private float              _distanceFromWand = 2.0f;
        [SerializeField]           
        private float              _maxPickingDist   = 10.0f;
        [SerializeField]           
        private LayerMask          _layerMask        = -1;
        [SerializeField]
        private UnityEngine.Camera _referenceCamera  = null;

        private PickableObject     _pickedObject;
        private Vector3            _direction        = Vector3.forward;

        public void CheckPick(bool pickDown, Vector3 raycastOrigin, Vector3 raycastDirection)
        {
            if (!pickDown)
                return;

            if (Physics.Raycast(raycastOrigin, raycastDirection, out var hit, _maxPickingDist, _layerMask))
            {
                var pickedObject = hit.transform.GetComponent<PickableObject>();
                if (pickedObject != null)
                    PickedObject = pickedObject;
            }

            Direction = raycastDirection;
        }

        public void CurrentlyPickingUpdate(bool pickDown, Vector3 pickDirection)
        {
            if (!pickDown)
            {
                PickedObject = null;
                return;
            }

            Direction = pickDirection;
        }

        private void LateUpdate()
        {
            if (PickedObject == null)
                return;
            
            PickedObject.SetDirection(_referenceCamera, transform.position, Direction);
            //PickedObject.SetPosition(transform.position + Direction * _distanceFromWand);
        }
    }
}