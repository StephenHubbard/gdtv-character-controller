using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevTV
{
	public class InputReader : MonoBehaviour
	{
		public Vector2 Move { get; private set; }
		public Vector2 Look { get; private set; }
		public Vector2 Zoom { get; private set; }
		public bool IsSprinting { get; private set; }
		[field: NonSerialized]	public bool IsJumping { get; set; }

		bool _cursorLocked = true;
		
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			LookInput(value.Get<Vector2>());
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnZoom(InputValue value)
		{
			ZoomInput(value.Get<Vector2>());
		}
		
		void MoveInput(Vector2 newMoveDirection)
		{
			Move = newMoveDirection;
		} 

		void LookInput(Vector2 newLookDirection)
		{
			Look = newLookDirection;
		}

		void JumpInput(bool newJumpState)
		{
			IsJumping = newJumpState;
		}

		void SprintInput(bool newSprintState)
		{
			IsSprinting = newSprintState;
		}
		
		void ZoomInput (Vector2 zoomValue)
		{
			Zoom = zoomValue;
		}

		void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(_cursorLocked);
		}

		void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}