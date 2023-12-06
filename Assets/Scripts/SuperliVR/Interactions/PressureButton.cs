using UnityEngine;
using UnityEngine.Events;

namespace SuperliVR.Interactions
{
    [RequireComponent(typeof(Collider))]
    public class PressureButton : MonoBehaviour
    {
        private enum ButtonState
        {
            Released,
            Pressed
        }

        [SerializeField]
        private LayerMask       _interactionLayers    = -1;
        [SerializeField]                              
        private float           _buttonSpeed          = 1.0f;
        [SerializeField] 
        private int             _framesToRelease      = 20;
        [SerializeField]
        private UnityEvent      _onButtonPressed;
        [SerializeField]
        private UnityEvent      _onButtonReleased;
                                                    
        private bool            _touchingContact;   
        private bool            _realTouchState;

        private ButtonState     _currentState         = ButtonState.Released;
                                
        private Vector3         _releasedPosition;
        private Vector3         _pressedPosition;
        private bool            _reachedDestination   = true;
                                
        private int             _framesSinceLastTouch;

        private void Awake()
        {
            var buttonCollider = GetComponent<Collider>();
            _releasedPosition = transform.position;
            _pressedPosition  = _releasedPosition - Vector3.up * buttonCollider.bounds.extents.y;
        }

        private void OnCollisionEnter(Collision collision) =>
            EvaluateCollision(collision);

        private void OnCollisionExit(Collision collision) =>
            EvaluateCollision(collision);
  
        private void OnCollisionStay(Collision collision) =>
            EvaluateCollision(collision);


        private void Update()
        {
            var targetPosition = _releasedPosition;
            if (_currentState == ButtonState.Pressed)
                targetPosition = _pressedPosition;

            var movement = _buttonSpeed * Time.deltaTime;

            var newPosition = Vector3.MoveTowards(transform.position, targetPosition, movement);
            _reachedDestination = Vector3.Distance(newPosition, transform.position) < Mathf.Epsilon;

            if (!_reachedDestination)
                transform.position = newPosition;
        }

        private void FixedUpdate()
        {
            if (_realTouchState && _currentState == ButtonState.Released && _reachedDestination)
                ButtonPressed();

            if (!_realTouchState && _currentState == ButtonState.Pressed && _reachedDestination)
                ButtonReleased();

            if (!_touchingContact)
            {
                if (_reachedDestination)
                {
                    if (_framesSinceLastTouch >= _framesToRelease)
                        SetRealTouchState();
                    _framesSinceLastTouch++;
                }
            }
            else
            {
                SetRealTouchState();
            }
        }

        private void EvaluateCollision(Collision collision)
        {
            _touchingContact = false;
            
            for (var contactIndex = 0; contactIndex < collision.contactCount; contactIndex++)
            {
                var contact = collision.GetContact(contactIndex);
                if ((_interactionLayers & (1 << contact.otherCollider.gameObject.layer)) != 0)
                    _touchingContact = true;
            }

            if (_touchingContact) 
                _framesSinceLastTouch = 0;
        }

        private void ButtonPressed()
        {
            _currentState = ButtonState.Pressed;
            _onButtonPressed.Invoke();
        }

        private void ButtonReleased()
        {
            _currentState = ButtonState.Released;
            _onButtonReleased.Invoke();
        }

        private void SetRealTouchState()
        {

            _realTouchState = _touchingContact;
            _framesSinceLastTouch = 0;
        }
    }
}