using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;

    // Player state
    public bool isJumping;
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDir;
    private CharacterController2D _characterController;

    

    private void Awake() {
        _characterController = GetComponent<CharacterController2D>();
    }

    private void Update() {
        _moveDir.x = _input.x * walkSpeed;

        if(_characterController.below){
            if(_startJump){
                _startJump = false;
                _moveDir.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck(0.1f);
            }
        }
        else{

            if(_releaseJump){
                _releaseJump = false;
                
                if(_moveDir.y > 0){
                    _moveDir.y *= 0.5f;
                }
            }
            _moveDir.y -= gravity * Time.deltaTime;
        }
        _characterController.Move(_moveDir * Time.deltaTime);
    }

    public void OnMovement(InputAction.CallbackContext context){
        _input = context.ReadValue<Vector2>();        
    }

    public void OnJump(InputAction.CallbackContext context){
        if(context.started){
            _startJump = true;
            _releaseJump = false;
        }
        else if(context.canceled){
            _startJump = false;
            _releaseJump = true;
        }
    }
}
