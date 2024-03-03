using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevTV
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField] float _moveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        [SerializeField] float _sprintSpeed = 6;
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)] [SerializeField] float _rotationSmoothTime = 0.1f;
        [Tooltip("Acceleration and deceleration")]
        [SerializeField] float _speedChangeRate = 10.0f;
        [Tooltip("The height the player can jump")]
        [SerializeField] float _jumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        [SerializeField] float _gravity = -15.0f;
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        [SerializeField] float _jumpCooldown = 0f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        [SerializeField] bool _grounded = true;
        [Tooltip("Useful for rough ground or matching player radius")]
        [SerializeField] float _groundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        [SerializeField] float _groundedRadius = 0.28f;
        [Tooltip("What layers the character uses as ground")]
        [SerializeField] LayerMask _groundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        [SerializeField] GameObject _cameraFollowPoint;
        [Tooltip("How far in degrees can you move the camera up")]
        [SerializeField] float _topClamp = 70.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        [SerializeField] float _bottomClamp = 10.0f;

        // Cinemachine
        float _cinemachineTargetYaw;
        float _cinemachineTargetPitch;

        // Player
        float _speed;
        float _targetRotation = 0.0f;
        float _rotationVelocity;
        float _verticalVelocity;
        float _terminalVelocity = 53.0f;

        // Timeout deltatime
        float _jumpTimeoutDelta;

        PlayerInput _playerInput;
        CharacterController _controller;
        InputReader _inputReader;
        GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

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
            // Reset our timeouts on start
            _jumpTimeoutDelta = _jumpCooldown;
        }

        private void Update()
        {
            GroundedCheck();
            JumpAndGravity();
            Move();
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
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

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

                // Jump
                if (_inputReader.Jumped && _jumpTimeoutDelta <= 0.0f)
                {
                    // The square root of height * -2 * gravity (H x -2 x G) = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                }

                // Jump cooldown after landing
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // Reset the jump timeout timer
                _jumpTimeoutDelta = _jumpCooldown;

                // if we are not grounded, do not jump
                _inputReader.Jumped = false;
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