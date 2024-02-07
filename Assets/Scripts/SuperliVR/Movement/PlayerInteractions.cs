using UnityEngine;

namespace SuperliVR.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerInteractions : MonoBehaviour
    {
        [SerializeField] 
        private float     _movementSpeed           = 1.0f;
        [SerializeField]                    
        private float     _maxAcceleration         = 10.0f;
        [SerializeField]
        private float     _maxAirAcceleration      = 1f;
        [SerializeField]
        private float     _maxJumpHeight           = 3.0f;
        [SerializeField, Range(0, 5)]
        private int       _maxAirJumps             = 0;
        [SerializeField] 
        private Transform _playerInputSpace        = default;
         
        private Vector3   _movement                = Vector3.zero;
        private Vector3   _desiredVelocity         = Vector3.zero;

        private Vector3   _desiredVerticalVelocity = Vector3.zero;
        private bool      _onGround                = true;
        private int       _jumpPhase               = 0;

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

        public void Jump()
        {
            if (_onGround || _jumpPhase < _maxAirJumps)
            {
                _jumpPhase += 1;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * _maxJumpHeight * transform.lossyScale.y);
                if (_desiredVerticalVelocity.y > 0f)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - _desiredVerticalVelocity.y, 0f);
                }
                _desiredVerticalVelocity.y += jumpSpeed;
            }
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
            UpdateState();

            float acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, _desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, _desiredVelocity.z, maxSpeedChange);

            velocity.y += _desiredVerticalVelocity.y;

            _rigidbody.velocity = velocity;

            _desiredVerticalVelocity = Vector3.zero;
            _onGround = false;
        }

        private void UpdateState()
        {
            if (_onGround)
                _jumpPhase = 0;
        }        

        private void OnCollisionEnter(Collision collision) =>
            EvaluateCollision(collision);
        

        private void OnCollisionExit(Collision collision) =>
            EvaluateCollision(collision);
        

        private void OnCollisionStay(Collision collision) =>
            EvaluateCollision(collision);
        

        private void EvaluateCollision(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                _onGround |= normal.y >= 0.9f;
            }
        }
    }
}