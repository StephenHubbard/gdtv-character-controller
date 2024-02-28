using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevTV.CharacterController
{
    public class CameraController : MonoBehaviour
	{
		[SerializeField] InputHandler inputHandler;

		[Header ("Target")]
		[Tooltip("The follow target that is set in the Cinemachine Virtual Camera")]
		[SerializeField] private GameObject cameraTarget;

		[Header("Constraints")]
		[Tooltip("Increase this value to increase the look sensitivity")]
		[SerializeField] private float lookSpeed = 1f;
		[Tooltip("How far the camera can be rotated up in degrees")]
		[SerializeField] private float TopClamp = 70f;
		[Tooltip("How far the camera can be rotated down in degrees")]
		[SerializeField] private float BottomClamp = -30f;
		[Tooltip("Minimum distance required to move the camera")]
		[SerializeField] private float lookThreshold = 0.1f;

		[Header("Fixed Controls")]
		[Tooltip("Locks the camera on all axes")]
		[SerializeField] private bool lockPosition = false;
		[Tooltip("Additional override controls for fine tuning")]
		[SerializeField] private float CameraAngleOverride = 0f;

		private float targetYaw;
		private float targetPitch;
		private Vector2 lookDirection;

		private void OnValidate () {
			lookSpeed = lookSpeed < 1 ? 1 : lookSpeed;
			TopClamp = TopClamp < 0 ? 0 : TopClamp;
			TopClamp = TopClamp > 90 ? 90 : TopClamp;
			BottomClamp = BottomClamp < -90 ? -90 : BottomClamp;
			BottomClamp = BottomClamp > 0f ? 0f : BottomClamp;
			lookThreshold = lookThreshold < 0 ? 0 : lookThreshold;
		}

		private void OnEnable () {
			if (inputHandler == null) {
				inputHandler = FindAnyObjectByType(typeof(InputHandler)) as InputHandler;
			}

			inputHandler.OnLook += OnLook;
		}

		private void OnDisable () {
			inputHandler.OnLook -= OnLook;
		}

		private void Start () {
			targetYaw = cameraTarget.transform.rotation.eulerAngles.y;
		}

		private void LateUpdate () {
			MoveCamera();
		}

		private void MoveCamera () {
			if (lookDirection.sqrMagnitude >= lookThreshold && !lockPosition) {
				targetYaw += lookDirection.x * lookSpeed * Time.deltaTime;
				targetPitch += lookDirection.y * lookSpeed * Time.deltaTime;
			}
			targetYaw = ClampAngle(targetYaw);
			targetPitch = ClampAngle(targetPitch, BottomClamp, TopClamp);

			cameraTarget.transform.rotation = Quaternion.Euler(targetPitch + CameraAngleOverride, targetYaw, 0f);
			lookDirection = Vector2.zero;
		}

		float ClampAngle (float angle, float min = float.MinValue, float max = float.MaxValue) {
			if (angle < -360f) { angle += 360f; }
			if (angle > 360f) { angle -= 360f; }
			return Mathf.Clamp(angle, min, max);
		}

		void OnLook (Vector2 direction) {
			lookDirection = direction;
		}
	}
}
