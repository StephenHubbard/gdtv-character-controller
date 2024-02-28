using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevTV.CharacterController
{
    public class CameraController : MonoBehaviour
	{
		[SerializeField] InputHandler inputHandler;
		
		[Header("Movement")]
		[Tooltip("Inverts the y-axis controls")]
		[SerializeField] bool invertYAxis;
		[Tooltip("Increase this value to increase the look sensitivity")]
		[SerializeField] float lookSpeed = 1f;

		[Header("Framing")]
		[Tooltip("How far the camera can be rotated up in degrees")]
		[SerializeField] float TopClamp = 70f;
		[Tooltip("How far the camera can be rotated down in degrees")]
		[SerializeField] float BottomClamp = -30f;
		[Tooltip("Minimum distance required to move the camera")]
		[SerializeField] float lookThreshold = 0.1f;

		[Header("Fixed Controls")]
		[Tooltip("Locks the camera on all axes")]
		[SerializeField] bool lockPosition = false;
		[Tooltip("Additional override controls for fine tuning")]
		[SerializeField] float CameraAngleOverride = 0f;

		float targetYaw;
		float targetPitch;
		Vector2 lookDirection;

		void OnValidate () {
			lookSpeed = lookSpeed < 1 ? 1 : lookSpeed;
			TopClamp = TopClamp < 0 ? 0 : TopClamp;
			TopClamp = TopClamp > 90 ? 90 : TopClamp;
			BottomClamp = BottomClamp < -90 ? -90 : BottomClamp;
			BottomClamp = BottomClamp > 0f ? 0f : BottomClamp;
			lookThreshold = lookThreshold < 0 ? 0 : lookThreshold;
		}
		
		void OnEnable ()
		{
			if (inputHandler != null) inputHandler.OnLook += OnLook;
		}

		void OnDisable () 
		{
			inputHandler.OnLook -= OnLook;
		}

		void Start () 
		{
			targetYaw = transform.rotation.eulerAngles.y;

			Cursor.lockState = CursorLockMode.Locked;
		}

		void Update()
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		void FixedUpdate () 
		{
			MoveCamera();
		}

		void MoveCamera () 
		{
			if (lookDirection.sqrMagnitude >= lookThreshold && !lockPosition) {
				targetYaw += lookDirection.x * lookSpeed * Time.deltaTime;
				targetPitch += lookDirection.y * lookSpeed * Time.deltaTime;
			}
			
			targetYaw = ClampAngle(targetYaw);
			targetPitch = ClampAngle(targetPitch, BottomClamp, TopClamp);

			transform.rotation = Quaternion.Euler(targetPitch + CameraAngleOverride, targetYaw, 0f);
			lookDirection = Vector2.zero;
		}

		float ClampAngle (float angle, float min = float.MinValue, float max = float.MaxValue) 
		{
			if (angle < -360f) { angle += 360f; }
			if (angle > 360f) { angle -= 360f; }
			
			return Mathf.Clamp(angle, min, max);
		}

		void OnLook (Vector2 direction)
		{
			Vector2 newDirection = direction;
			newDirection.y = invertYAxis ? newDirection.y : -newDirection.y;
			
			lookDirection = newDirection;
		}
	}
}
