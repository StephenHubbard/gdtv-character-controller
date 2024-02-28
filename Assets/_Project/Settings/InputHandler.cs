using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace GameDevTV.CharacterController
{
	public class InputHandler : MonoBehaviour
	{
		public static InputHandler instance;

		public event Action<Vector2> OnLook;
		public event Action<Vector2> OnMove;
		// public event Action OnSprintStart;
		// public event Action OnSprintEnd;
		public event Action OnJump;
		
		PlayerControls controls;

		void Awake () 
		{
			if (instance != null) {
				Destroy(this.gameObject);
			}
			else {
				instance = this;
			}

			controls = new PlayerControls();
		}

		void OnEnable () 
		{
			controls.Player.Look.performed += HandleLook;
			controls.Player.Move.performed += HandleMove;
			controls.Player.Move.canceled += HandleMove;
			// controls.Player.Sprint.started += HandleSprintStart;
			// controls.Player.Sprint.canceled += HandleSprintEnd;
			controls.Player.Jump.performed += HandleJump;
			controls.Player.Enable();
		}

		void OnDisable () 
		{
			controls.Player.Look.performed -= HandleLook;
			controls.Player.Move.performed -= HandleMove;
			controls.Player.Move.canceled -= HandleMove;
			// controls.Player.Sprint.started -= HandleSprintStart;
			// controls.Player.Sprint.canceled -= HandleSprintEnd;
			controls.Player.Jump.performed -= HandleJump;
			controls.Player.Disable();
		}

		void HandleLook (CallbackContext context) 
		{
			Vector2 value = context.ReadValue<Vector2>();
			
			OnLook?.Invoke(value);
		}

		void HandleMove (CallbackContext context) 
		{
			OnMove?.Invoke(context.ReadValue<Vector2>());
		}

		// void HandleSprintStart (CallbackContext context) 
		// {
		// 	OnSprintStart?.Invoke();
		// }
		//
		// void HandleSprintEnd (CallbackContext context) 
		// {
		// 	OnSprintEnd?.Invoke();
		// }

		void HandleJump (CallbackContext context) 
		{
			OnJump?.Invoke();
		}
	}
}