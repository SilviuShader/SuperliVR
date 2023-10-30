using UnityEngine;

namespace SuperliVR.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField]
        private float     _movementSpeed    = 1.0f;
        [SerializeField]                    
        private float     _maxAcceleration  = 10.0f;
        [SerializeField] 
        private Transform _playerInputSpace = default;
         
        private Vector3   _movement         = Vector3.zero;
        private Vector3   _desiredVelocity  = Vector3.zero;

        private Rigidbody _rigidbody;

        public void Move(Vector2 direction)
        {
            var input = Vector2.ClampMagnitude(direction, 1.0f);

            var forward = _playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = _playerInputSpace.right;
            right.y = 0f;
            right.Normalize();

            _movement = forward * input.y + right * input.x;
        }

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();

        private void Update()
        {
            _desiredVelocity = _movement * _movementSpeed;
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