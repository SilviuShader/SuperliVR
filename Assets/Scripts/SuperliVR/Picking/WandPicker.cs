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
                    _pickedObject.PickUp(_referenceCamera);
            }
        }

        public  Vector3            Direction
        {
            get => _direction;
            set => _direction = value.normalized;
        }

        public Vector3             PickOrigin { get; set; }

        public  bool               CurrentlyPicking   => PickedObject != null;

        public  float              MaxPickingDist     = 300.0f;

        public  LayerMask          RegularSceneLayers = -1;

        [SerializeField]           
        private float              _distanceFromWand  = 2.0f;
        [SerializeField]           
        private LayerMask          _layerMask         = -1;
        [SerializeField]                              
        private UnityEngine.Camera _referenceCamera   = null;
                                                      
        private PickableObject     _pickedObject;     
        private Vector3            _direction         = Vector3.forward;

        public void CheckPick(bool pickDown, Vector3 raycastOrigin, Vector3 raycastDirection)
        {
            if (!pickDown)
                return;

            if (Physics.Raycast(raycastOrigin, raycastDirection, out var regularHit, MaxPickingDist, RegularSceneLayers))
            {
                if (Physics.Raycast(raycastOrigin, raycastDirection, out var hit, MaxPickingDist, _layerMask))
                {
                    if (hit.distance < regularHit.distance + Mathf.Epsilon || regularHit.transform == hit.transform)
                    {
                        var pickedObject = hit.transform.GetComponent<PickableObject>();
                        if (pickedObject != null)
                            PickedObject = pickedObject;
                    }
                }
            }

            PickOrigin = raycastOrigin;
            Direction = raycastDirection;
        }

        public void CurrentlyPickingUpdate(bool pickDown, Vector3 pickOrigin, Vector3 pickDirection)
        {
            if (!pickDown)
            {
                PickedObject = null;
                return;
            }

            PickOrigin = pickOrigin;
            Direction = pickDirection;
        }

        private void LateUpdate()
        {
            if (PickedObject == null)
                return;
            
            PickedObject.SetDirection(_referenceCamera, PickOrigin, Direction);
        }
    }
}