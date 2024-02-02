using SuperliVR.Picking;
using UnityEngine;
using Utils;

namespace SuperliVR.Portals
{
    [RequireComponent(typeof(Collider))]
    public class PortalBehaviour : MonoBehaviour
    {
        private Portal   _portal;
        private Collider _collider;

        private void Awake()
        {
            _portal   = transform.parent.GetComponent<Portal>();
            _collider = GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherForward = other.transform.forward;
            var cam = other.GetComponentInChildren<UnityEngine.Camera>();
            if (cam != null)
                otherForward = cam.transform.forward;

            if (other.CompareTag("Player") &&
                Vector3.Dot(transform.up, other.transform.position - transform.position) >= Mathf.Epsilon &&
                Vector3.Dot(transform.up, otherForward) <= -Mathf.Epsilon)
            {
                var wandPicker = other.GetComponentInChildren<WandPicker>();

                var currentlyPicking = false;
                if (wandPicker != null)
                    currentlyPicking = wandPicker.CurrentlyPicking;

                if (!currentlyPicking)
                    if (ScaleHelper.ObjectBoundingRadius(other) < ScaleHelper.ObjectBoundingRadius(_collider))
                        _portal.Teleport(other);
            }
        }
    }
}