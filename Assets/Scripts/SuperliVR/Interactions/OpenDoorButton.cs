using UnityEngine;

namespace SuperliVR.Interactions
{
    public class OpenDoorButton : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _door;

        [SerializeField] 
        private Material   _transparentMaterial;
        
        [SerializeField]   
        private Material   _opaqueMaterial;
        private int        _objectsOnButton;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
            {
                _door.GetComponent<MeshRenderer>().material = _transparentMaterial;
                _door.GetComponent<BoxCollider>().enabled = false;
                _objectsOnButton++;
            }

        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
            {
                _objectsOnButton--;
                if (_objectsOnButton == 0)
                {
                    _door.GetComponent<MeshRenderer>().material = _opaqueMaterial;
                    _door.GetComponent<BoxCollider>().enabled = true;
                }
            }

        }
    }
}