using UnityEngine;
using Utils;

namespace SuperliVR.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField]
        private float     _movementSpeed   = 1.0f;
        [SerializeField]
        private float     _maxAcceleration = 10.0f;
         
        private Vector2   _movement        = Vector2.zero;
        private Vector3   _desiredVelocity = Vector3.zero;

        private Rigidbody _rigidbody;

        public void Move(Vector2 direction) =>
            _movement = Vector2.ClampMagnitude(direction, 1.0f);

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();

        private void Update()
        {
            _desiredVelocity = (Vector3.right * _movement.x + Vector3.forward * _movement.y) * _movementSpeed;
        }

        private void FixedUpdate()
        {
            var velocity = _rigidbody.velocity;

            var maxSpeedChange = _maxAcceleration * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, _desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, _desiredVelocity.z, maxSpeedChange);

            _rigidbody.velocity = velocity;

            _desiredVelocity = Vector3.zero;
        }
    }
}