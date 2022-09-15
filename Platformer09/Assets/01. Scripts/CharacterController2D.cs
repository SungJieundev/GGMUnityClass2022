using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate() {
        _lastPosition = _rigidbody.position;
        _currentPosition = _lastPosition + _moveAmount;

        _rigidbody.MovePosition(_currentPosition); 

        _moveAmount = Vector2.zero;
    }

    public void Move(Vector2 movement){
        _moveAmount += movement;
    }
}
