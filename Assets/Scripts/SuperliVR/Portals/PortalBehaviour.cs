using UnityEngine;

namespace SuperliVR.Portals
{
    [RequireComponent(typeof(Collider))]
    public class PortalBehaviour : MonoBehaviour
    {
        private Portal _portal;

        private void Awake() =>
            _portal = transform.parent.GetComponent<Portal>();

        private void OnTriggerEnter(Collider other)
        {
            var otherForward = other.transform.forward;
            var cam = other.GetComponentInChildren<UnityEngine.Camera>();
            if (cam != null)
                otherForward = cam.transform.forward;

            if (other.CompareTag("Player") && 
                Vector3.Dot(transform.up, other.transform.position - transform.position) >= Mathf.Epsilon &&
                Vector3.Dot(transform.up, otherForward) <= -Mathf.Epsilon)
                _portal.Teleport(other);
        }
    }
}