using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class playerInputController : MonoBehaviour
{
    private CharacterController _characterController;
    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _dashAction;
    private Animator _animator;
    
    private Camera _playerCamera;
    [SerializeField] private float moveSpeed = 4f; //change this shi to 5 dont forget
    [SerializeField] private float rotSpeed = 31f;
    [SerializeField] private float dashPower = 20f;
    [SerializeField] private float dashTime = 0.3f;
    [SerializeField] private float dashCooldown = 0.75f;
    [SerializeField] private ParticleSystem ps;


    
    
    private float _dashCooldownTimer = 0f;
    private Vector3 _dashDirection;
    private float _yaw = 0f;
    private float _botch = 0f;
    private bool _isDashing = false;

    public Vector3 velocity;

   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent < PlayerInput>();
        _playerCamera = Camera.main;
    }

    void Start()
    {
        Cursor.visible=false;
        
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _dashAction = _playerInput.actions["Dash"];
        
     
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_dashCooldownTimer > 0)
            _dashCooldownTimer -= Time.deltaTime;

        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        
        Vector3 playerMovement =
            transform.forward * (moveValue.y * moveSpeed) + transform.right * (moveValue.x * moveSpeed);

        _characterController.Move(playerMovement * Time.deltaTime);
        
        _animator.SetFloat("Speed", playerMovement.magnitude);


        Vector2 lookValue = _lookAction.ReadValue<Vector2>();

         _yaw += lookValue.x * rotSpeed * Time.deltaTime;
         _botch -= lookValue.y * rotSpeed * Time.deltaTime;
         _botch = Mathf.Clamp(_botch, -89f, 89f);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);


        if (_dashAction.WasPressedThisFrame() && !_isDashing && _dashCooldownTimer <= 0)
        {
            StartCoroutine(Dash());

        }
    }

    IEnumerator Dash()
        {
            
            _isDashing = true;
            
            ps.Clear();
            ps.Play();
            
            _animator.SetTrigger("DashTrigger");
            _dashCooldownTimer = dashCooldown;
            

            // Get dash direction from input
            Vector2 moveValue = _moveAction.ReadValue<Vector2>();

            if (moveValue.magnitude > 0.1f)
            {
                _dashDirection = transform.forward * moveValue.y + transform.right * moveValue.x;
            }
            else
            {
              
                _dashDirection = transform.forward;
            }

            _dashDirection.Normalize();

            float timer = 0f;

            while (timer < dashTime)
            {
                _characterController.Move(_dashDirection * dashPower * Time.deltaTime);

                timer += Time.deltaTime;
                yield return null;
            }

            ps.Stop();
            _isDashing = false;
            
        }
    
}