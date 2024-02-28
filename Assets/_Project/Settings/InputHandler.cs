using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace GameDevTV.CharacterController
{
	public class InputHandler : MonoBehaviour
	{
		private static InputHandler instance;

		PlayerControls controls;

		public event Action<Vector2> OnLook;
		public event Action<Vector2> OnMove;
		public event Action OnSprintStart;
		public event Action OnSprintEnd;
		public event Action OnJump;

		[Tooltip("Inverts the y-axis controls, which is preferred by some players")]
		[SerializeField] private bool invertYAxis;
		public bool InvertYAxis { get { return invertYAxis; } set { invertYAxis = value; } }

		private void Awake () {
			if (instance != null) {
				Debug.LogErrorFormat("Cannot instantiate a second instance of {0}. Duplicate instance will be destroyed", GetType().Name);
				Destroy(this.gameObject);
			}
			else {
				instance = this;
			}

			controls = new PlayerControls();
		}

		private void OnEnable () {
			//Register for events.
			controls.Player.Look.performed += HandleLook;
			controls.Player.Move.performed += HandleMove;
			controls.Player.Move.canceled += HandleMove;
			controls.Player.Sprint.started += HandleSprintStart;
			controls.Player.Sprint.canceled += HandleSprintEnd;
			controls.Player.Jump.performed += HandleJump;
			controls.Player.Enable();
		}

		private void OnDisable () {
			//Deregister from events.
			controls.Player.Look.performed -= HandleLook;
			controls.Player.Move.performed -= HandleMove;
			controls.Player.Move.canceled -= HandleMove;
			controls.Player.Sprint.started -= HandleSprintStart;
			controls.Player.Sprint.canceled -= HandleSprintEnd;
			controls.Player.Jump.performed -= HandleJump;
			controls.Player.Disable();
		}

		private void HandleLook (CallbackContext context) {
			Vector2 value = context.ReadValue<Vector2>();
			if (invertYAxis) {
				value.y = -value.y;
			}
			OnLook?.Invoke(value);
		}

		private void HandleMove (CallbackContext context) {
			OnMove?.Invoke(context.ReadValue<Vector2>());
		}

		private void HandleSprintStart (CallbackContext context) {
			OnSprintStart?.Invoke();
		}

		private void HandleSprintEnd (CallbackContext context) {
			OnSprintEnd?.Invoke();
		}

		private void HandleJump (CallbackContext context) {
			OnJump?.Invoke();
		}
	}
}