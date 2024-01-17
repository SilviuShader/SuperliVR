using UnityEngine;

namespace SuperliVR.Portals
{
    [RequireComponent(typeof(Collider))]
    public class PortalBehaviour : MonoBehaviour
    {
        public  Collider BannedCollider { get; set; }
        
        private Portal   _portal;

        private void Awake() =>
            _portal = transform.parent.GetComponent<Portal>();

        private void OnTriggerEnter(Collider other)
        {
            if (other == BannedCollider)
            {
                Debug.Log("OKK???");
                return;
            }

            if (other.CompareTag("Player") && 
                Vector3.Dot(transform.up, other.transform.position - transform.position) >= Mathf.Epsilon)
                _portal.Teleport(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == BannedCollider)
                BannedCollider = null;
        }
    }
}