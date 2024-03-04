using System;
using System.Collections.Generic;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevTV.Platformer
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        public Action Jump;
        
        [Header("Player")]
        [SerializeField] float _moveSpeed = 4.0f;
        [SerializeField] float _sprintSpeed = 6f;
        [Range(0.0f, 0.3f)] [SerializeField] float _rotationSmoothTime = 0.1f;
        [SerializeField] float _speedChangeRate = 10.0f;
        [SerializeField] float _minJumpHeight = 1f;
        [SerializeField] float _maxJumpHeight = 2f;
        [SerializeField] float _gravity = -15.0f;
        [SerializeField] float _terminalVelocity = 53.0f;

        // [SerializeField] float _jumpCooldown = 0f;

        [Header("Player Grounded")]
        [SerializeField] bool _grounded = true;
        [SerializeField] float _groundedOffset = -0.14f;
        [SerializeField] float _groundedRadius = 0.28f;
        [SerializeField] LayerMask _groundLayers;

        [Header("Cinemachine")]
        [SerializeField] GameObject _cameraFollowPoint;
        [SerializeField] float _topClamp = 70.0f;
        [SerializeField] float _bottomClamp = 10.0f;

        float _cinemachineTargetYaw;
        float _cinemachineTargetPitch;

        float _speed;
        float _targetRotation = 0.0f;
        float _rotationVelocity;
        float _minJumpVelocity, _maxJumpVelocity;
        float _verticalVelocity;
        bool _jumpToConsume; 
        
        PlayerInput _playerInput;
        CharacterController _controller;
        InputReader _inputReader;
        GameObject _mainCamera;

        const float _threshold = 0.01f;

        bool _isCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

        private void Awake()
        {
            if (Camera.main != null) _mainCamera = Camera.main.gameObject;
           
            _cinemachineTargetYaw = _cameraFollowPoint.transform.rotation.eulerAngles.y;
            
            _controller = GetComponent<CharacterController>();
            _inputReader = GetComponent<InputReader>();
            _playerInput = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            // The square root of height * 2 * gravity = how much velocity needed to reach desired height
            _minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (_gravity) * _minJumpHeight);
            _maxJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (_gravity) * _maxJumpHeight);
        }

        private void Update()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
            
            // Debug.Log(_inputReader.IsJumping);
        }

        private void LateUpdate()
        {
            CameraRotation();
        }
        
        private void GroundedCheck()
        {
            Vector3 position = transform.position;
            Vector3 spherePosition = new(position.x, position.y - _groundedOffset, position.z);
            _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);
        }
        
        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new(0f, 1f, 0f, .5f);
            Color transparentRed = new(1f, 0f, 0f, .5f);

            Gizmos.color = _grounded ? transparentGreen : transparentRed;

            Vector3 position = transform.position;
            Gizmos.DrawSphere(new Vector3(position.x, position.y - _groundedOffset, position.z), _groundedRadius);
        }

        private void CameraRotation()
        {
            if (_inputReader.Look.sqrMagnitude >= _threshold)
            {
                // Multiply controller input by Time.deltaTime but not mouse input;
                float deltaTimeMultiplier = _isCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _inputReader.Look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _inputReader.Look.y * deltaTimeMultiplier;
            }

            // Clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

            // Cinemachine will follow this target
            _cameraFollowPoint.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = _inputReader.IsSprinting ? _sprintSpeed : _moveSpeed;

            if (_inputReader.Move == Vector2.zero) targetSpeed = 0.0f;

            // Players current horizontal velocity
            var controllerVelocity = _controller.velocity;
            float currentHorizontalSpeed = new Vector3(controllerVelocity.x, 0.0f, controllerVelocity.z).magnitude;
            
            // Ensures buffer tolerance for controller velocity
            float speedOffset = 0.1f;
            float inputMagnitude = _inputReader.Move.magnitude;

            // Accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Creates curved result rather than a linear one giving a more organic speed change
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * _speedChangeRate);

                // Round speed to 3 decimal places for consistent behavior
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            // Normalize input direction
            Vector3 inputDirection = new Vector3(_inputReader.Move.x, 0.0f, _inputReader.Move.y).normalized;

            // Rotate player when the player is moving
            if (_inputReader.Move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _rotationSmoothTime);

                // Rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (_grounded)
            {
                // Stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_inputReader.IsJumping)
                {
                    _verticalVelocity = _maxJumpVelocity;
                }
            }
            // else
            // {
            //     _inputReader.IsJumping = false;
            // }
            
            if (!_inputReader.IsJumping && _verticalVelocity > _minJumpVelocity)
            {
                _verticalVelocity = _minJumpVelocity;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
    }
}