using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalType;

public class CharacterController2D : MonoBehaviour
{   
    public float raycastDistance = 0.2f;
    public LayerMask layerMask;
    public float slopAngleLimit = 45f;

    // flags
    public bool below;
    public bool above;
    public bool right;
    public bool left;
    public GroundType groundType;
    public GroundType ceilingType;
    public WallType rightWallType;
    public WallType leftWallType;

    //나중에 private로 변경예정
    public Vector2 slopNormal;
    public float slopAngle;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    private bool _disableGroundCheck;

    private Vector2[] _raycastPosition = new Vector2[3];
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update() {
        _lastPosition = _rigidbody.position;

        if(slopAngle != 0 && below)
        {
            if(_moveAmount.x > 0f && slopAngle > 0f || (_moveAmount.x < 0f && slopAngle < 0f)){
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(slopAngle * Mathf.Deg2Rad) * _moveAmount.x);
            }
        }
        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPosition); 

        _moveAmount = Vector2.zero;

        if(!_disableGroundCheck) CheckGrounded();

        CheckOtherCollision();
        //Debug.Log(_disableGroundCheck);
    }

    public void Move(Vector2 movement){
        _moveAmount += movement;
    }

    private void CheckOtherCollision(){


        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size 
        * 0.7f, 0f, Vector2.left, raycastDistance, layerMask);

        if(leftHit.collider){
            leftWallType = DetermineWallType(leftHit.collider);
            left = true;
        }
        else{
            leftWallType = WallType.None;
            left = false;
        }

        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size 
        * 0.7f, 0f, Vector2.right, raycastDistance, layerMask);

        if(rightHit.collider){
            rightWallType = DetermineWallType(rightHit.collider);
            right = true;
        }
        else{
            rightWallType = WallType.None;
            right = false;
        }

        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, 
        CapsuleDirection2D.Vertical, 0f, Vector2.up, raycastDistance, layerMask);

        if(aboveHit.collider){
            ceilingType = DetermineGroundType(aboveHit.collider);
            above = true;
        }
        else{
            ceilingType = GroundType.none;
            above = false;
        }
    }

    private void CheckGrounded(){
        
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, 
        CapsuleDirection2D.Vertical, 0f, Vector2.down, raycastDistance, layerMask);

        if(hit.collider){
            groundType = DetermineGroundType(hit.collider);

            slopNormal = hit.normal;
            slopAngle = Vector2.SignedAngle(slopNormal, Vector2.up);

            if(slopAngle > slopAngleLimit || slopAngle < -slopAngleLimit){
                below = false;
            }
            else{
                below = true;
            }
        }
        else{
            below = false;
            groundType = GroundType.none;
        }
        Debug.Log("below" + below);
    }

    private GroundType DetermineGroundType(Collider2D collider){
        if(collider.GetComponent<GroundEffector>()){
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else return GroundType.LevelGeom;
    }

    private WallType DetermineWallType(Collider2D collider){
        if(collider.GetComponent<WallEffector>()){
            WallEffector WallEffector = collider.GetComponent<WallEffector>();
            return WallEffector.wallType;
        }
        else return WallType.Normal;
    }
    
    private void DrawDebugRays(Vector2 dir, Color color){
        for(int i = 0; i < _raycastPosition.Length; i++){
            Debug.DrawRay(_raycastPosition[i], dir * raycastDistance, color);
        }
    }

    public void DisableGroundCheck(float delayTime){
        below = false;
        _disableGroundCheck = true;
        StartCoroutine("EnableGroundCheck", delayTime);
    }

    IEnumerator EnableGroundCheck(float delayTime){
        yield return new WaitForSeconds(delayTime);
        _disableGroundCheck = false;
    }
}
