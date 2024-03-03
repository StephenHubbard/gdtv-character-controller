using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace GameDevTV
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader; 
        [SerializeField] float _zoomSpeed = 1f; 
        [SerializeField] float _minDistance = 0f; 
        [SerializeField] float _maxDistance = 10f; 
        [SerializeField] float _smoothTime = 0.1f; 

        CinemachineVirtualCamera _cinemachineCamera;
        Cinemachine3rdPersonFollow _cinemachine3RdPersonFollow;
        
        float _targetDistance;
        float _zoomVelocity;

        private void Awake()
        {
            _cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
            _cinemachine3RdPersonFollow = _cinemachineCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            _targetDistance = _cinemachine3RdPersonFollow.CameraDistance;
        }

        private void Update()
        {
            Vector2 zoomValue = _inputReader.Zoom;

            if (zoomValue.y != 0f)
            {
                AdjustCameraDistance(zoomValue.y);
            }

            SmoothCameraDistance();
        }

        void AdjustCameraDistance(float zoomInput)
        {
            _targetDistance -= zoomInput * _zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        }

        void SmoothCameraDistance()
        {
            float smoothedDistance = Mathf.SmoothDamp(_cinemachine3RdPersonFollow.CameraDistance, _targetDistance, ref _zoomVelocity, _smoothTime);
            _cinemachine3RdPersonFollow.CameraDistance = smoothedDistance;
        }
    }
}
