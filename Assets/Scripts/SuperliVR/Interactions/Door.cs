using UnityEngine;

namespace SuperliVR.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class Door : MonoBehaviour
    {
        private enum DoorState
        {
            Closed,
            Open
        }

        [SerializeField]
        private float     _doorSpeed = 10.0f;

        private DoorState _state     = DoorState.Closed;

        private Vector3   _targetClosedPosition;
        private Vector3   _targetOpenPosition;

        public void Open() =>
            _state = DoorState.Open;

        public void Close() =>
            _state = DoorState.Closed;

        private void Awake()
        {
            var collider = GetComponent<Collider>();
            _targetClosedPosition = transform.position;
            _targetOpenPosition = _targetClosedPosition + transform.up * collider.bounds.extents.y * 2.0f;
        }

        private void Update()
        {
            var targetPosition = _state == DoorState.Open ? _targetOpenPosition : _targetClosedPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _doorSpeed * Time.deltaTime);
        }
    }
}