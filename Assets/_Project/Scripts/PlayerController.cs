using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevTV.CharacterController
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] Transform _rigTransform;
        
        [SerializeField] InputHandler _inputHandler;
        [SerializeField] LayerMask _jumpLayers;
        [SerializeField] float _moveSpeed = 5;
        // [SerializeField] float _rotationSpeed = .1f;

        Rigidbody _rigidbody;
        Transform _mainCamTransform;
        
        private Vector2 _moveDirection;
        float _targetRotation;
        float _rotationVelocity;

        void Awake()
        {
            if (Camera.main != null) _mainCamTransform = Camera.main.transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        void OnEnable()
        {
            _inputHandler.OnJump += OnJump;
            _inputHandler.OnMove += OnMove;
        }

        void OnDisable()
        {
            _inputHandler.OnJump -= OnJump;
            _inputHandler.OnMove -= OnMove;
        }

        void Update()
        {
        }

        void FixedUpdate()
        {
            _rigTransform.rotation = Quaternion.Euler(0f, _mainCamTransform.eulerAngles.y, 0f);
            
            // Calculate the directions relative to the camera's orientation
            Vector3 forward = _mainCamTransform.forward;
            Vector3 right = _mainCamTransform.right;

            // Ignore the y component to keep movement horizontal
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Calculate the desired move direction based on camera orientation
            Vector3 desiredMoveDirection = (forward * _moveDirection.y + right * _moveDirection.x).normalized;

            // Apply movement
            _rigidbody.velocity = new Vector3(desiredMoveDirection.x * _moveSpeed, _rigidbody.velocity.y, desiredMoveDirection.z * _moveSpeed);
        }

        private bool IsGrounded () {
            bool isGrounded = Physics.CheckSphere(transform.position, 1f, _jumpLayers, QueryTriggerInteraction.Ignore);

            return isGrounded;
        }
        
        private void OnMove (Vector2 Direction) {
            _moveDirection = Direction;
        }
        
        private void OnJump ()	{
            if (IsGrounded())
            {   
                Debug.Log("Jumping!");
                _rigidbody.AddForce(Vector3.up * 5, ForceMode.Impulse);
            }
            else
            {
                Debug.Log("Not grounded!");
            }
        }
    }
}
