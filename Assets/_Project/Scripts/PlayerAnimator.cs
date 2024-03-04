using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevTV.Platformer
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader; 
        [SerializeField] float _leanAmount = 8f; 
        [SerializeField] float _leanSpeed = 5f; 

        private void Update()
        {
            LeanCharacter();
        }

        void LeanCharacter()
        {
            float inputMagnitude = _inputReader.Move.magnitude; 
            float targetLean = _leanAmount * inputMagnitude; 
            Quaternion targetRotation = Quaternion.Euler(targetLean, 0f, 0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * _leanSpeed);
        }
    }
}
