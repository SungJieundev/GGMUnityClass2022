using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{   
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float creepSpeed = 5f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float doubleJumpSpeed = 10f;
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallRunSpeed = 8f;
    public float wallSlideAmount = 0.1f;

    // player ability toggle;
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canWallRun;
    public bool canWallSlide;

    // Player state
    [Header("Player States")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;

    // input flags
    
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private Vector2 _moveDir;
    private CharacterController2D _characterController;
    private CapsuleCollider2D _capsuleCollider;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _originalColliderSize;
    private bool _ableToWallRun;

    

    private void Awake() {
        _characterController = GetComponent<CharacterController2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _originalColliderSize = _capsuleCollider.size;
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
            isWallRunning = false;

            if(_startJump){
                _startJump = false;
                _moveDir.y = jumpSpeed;
                _ableToWallRun = true;
                isJumping = true;
                _characterController.DisableGroundCheck(0.1f);
            }

            //ducking, creeping
            if(_input.y < 0f){
                if(!isDucking && !isCreeping){
                    isDucking = true;

                    // 콜라이더 사이즈 반으로 줄이기
                    _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y / 2);
                    // position.y 변경
                    transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y / 4));
                    // Sprite 변경
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
                }
            }
            else{
                if(isDucking || isCreeping){
                    
                    RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                    CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask );

                    if(!hitCeiling.collider){

                        isDucking = false;
                        isCreeping = false;

                        _capsuleCollider.size = _originalColliderSize;
                        transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                        _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    }
                }
            }

            // creeping
            if(isDucking && _moveDir.x != 0){
                isCreeping = true;
            }
            else{
                isCreeping = false;
            }
        }
        else{ // 공중에

            if((isCreeping || isDucking) && _moveDir.y > 0){
                StartCoroutine(ClearDuckingState());
            }

            if(_releaseJump){
                _releaseJump = false;
                
                if(_moveDir.y > 0){
                    _moveDir.y *= 0.5f;
                }
            }

            // pressed jump button in air
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

            // wall running
            if(canWallRun && _characterController._left || _characterController._right){

                if(_input.y > 0f && _ableToWallRun){
                    _moveDir.y = wallRunSpeed;
                }
                //isWallRunning = true;

                StartCoroutine(WallRunWaiter());
            }

            GravityCalculation();
        }
        _characterController.Move(_moveDir * Time.deltaTime);
    }

    void GravityCalculation(){
        if(_moveDir.y > 0f && _characterController._above){
            _moveDir.y = 0f;
        }

        if(canWallSlide && _characterController._left || _characterController._right){
            if(_moveDir.y <= 0){
                _moveDir.y -= gravity * wallSlideAmount * Time.deltaTime;
            }
            else{
                _moveDir.y -= gravity * Time.deltaTime;
            }
        }
        else{
            _moveDir.y -= gravity * Time.deltaTime;
        }
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

    IEnumerator WallRunWaiter(){
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;
        _ableToWallRun = false;
    }

    IEnumerator ClearDuckingState(){
        yield return new WaitForSeconds(0.05f);

        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                    CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask );

                    if(!hitCeiling.collider){

                        isDucking = false;
                        isCreeping = false;

                        _capsuleCollider.size = _originalColliderSize;
                        transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                        _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                    }
    }
}
