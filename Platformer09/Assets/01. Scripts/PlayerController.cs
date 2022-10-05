using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{   
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 5f;
    public float yWallJumpSpeed = 5f;

    // player ability toggle;
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;

    // Player state
    [Header("Player States")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;

    // input flags
    
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

        if(_moveDir.x > 0f){
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if(_moveDir.x < 0f){
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if(_characterController.below){ // on the ground

            _moveDir.y = 0f;
            isJumping = false;
            isDoubleJumping = false;
            isTripleJumping = false;
            isWallJumping = false;

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

            if(_startJump){

                // triple jumping
                if(canTripleJump && (!_characterController._left && !_characterController._right)){
                    if(isDoubleJumping && !isTripleJumping){
                        _moveDir.y = doubleJumpSpeed;
                        isTripleJumping = true;
                    }
                }

                // double jumping
                if(canDoubleJump && (!_characterController._left && !_characterController._right)){
                    if(!isDoubleJumping){
                        _moveDir.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }

                if(canWallJump && (_characterController._left || _characterController._right)){
                    if(_characterController._left){
                        _moveDir.x = xWallJumpSpeed;
                        _moveDir.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (_characterController._right){
                        _moveDir.x = -xWallJumpSpeed;
                        _moveDir.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    isWallJumping = true;
                    StartCoroutine(WallJumpWaiter());
                }

                _startJump = false;
            }

            GravityCalculation();
        }
        _characterController.Move(_moveDir * Time.deltaTime);
    }

    void GravityCalculation(){
        if(_moveDir.y > 0f && _characterController._above){
            _moveDir.y = 0f;
        }
        _moveDir.y -= gravity * Time.deltaTime;
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

    IEnumerator WallJumpWaiter(){
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }
}
