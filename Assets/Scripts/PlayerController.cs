using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeedFactor;
    public float sprintMaxTime;
    
    float _sprintTime;
    
    bool _isSprinting;
    
    float _horizontalMove;
    float _verticalMove;
    Vector3 _moveDirection;
    
    public Rigidbody rb;
    
    [Header("Mouse Look")]
    public float sensX = 150.0f;
    public float sensY = 150.0f;

    public Transform orientation;
    
    public float rotationX = 0;
    public float rotationY = 0;
    
    public Camera cam;
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        rb.freezeRotation = true;
        _sprintTime = sprintMaxTime;
    }

    // Update is called once per frame
    void Update()
    {
        rotationY += Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        rotationX -= Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
        
        rotationX = Mathf.Clamp(rotationX, -90, 90);
        
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
        
        _horizontalMove = Input.GetAxisRaw("Horizontal");
        _verticalMove = Input.GetAxisRaw("Vertical");
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isSprinting = true;
            
            _sprintTime -= Time.deltaTime;
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            _isSprinting = false;
            
            _sprintTime += Time.deltaTime;
        }
        
        _sprintTime = Mathf.Clamp(_sprintTime, 0, sprintMaxTime);
        if(_sprintTime == 0) _isSprinting = false;
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;
        
        speed = _isSprinting ? speed * sprintSpeedFactor : speed;
        
        _moveDirection = orientation.forward * _verticalMove + orientation.right * _horizontalMove;
        rb.AddForce(_moveDirection.normalized * speed, ForceMode.Acceleration);
    }
    
    public void IsSprinting()
    {
        _isSprinting = true;
    }
}
