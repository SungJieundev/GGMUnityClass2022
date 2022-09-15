using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gravity = 20f;

    private Vector2 _input;
    private Vector2 _moveDir;
    private CharacterController2D _characterController;

    private void Awake() {
        _characterController = GetComponent<CharacterController2D>();
    }

    private void Update() {
        _moveDir.x = _input.x * walkSpeed;
        //_moveDir.y -= gravity * Time.deltaTime;
        _characterController.Move(_moveDir * Time.deltaTime);
    }

    public void OnMovement(InputAction.CallbackContext context){
        _input = context.ReadValue<Vector2>();        
    }
}
