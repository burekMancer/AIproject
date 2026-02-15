using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _dashAction;
    private Animator _animator;
    private Vector3 _playerMovement;
    private bool _iFrames = false;


    private Camera _playerCamera;
    private float rotSpeed = 31f;
    private float dashPower = 20f;
    private float dashTime = 0.3f;
    private float dashCooldown = 0.75f;
    [SerializeField] private ParticleSystem ps;

    [Header("Stats")] //
    [SerializeField]
    private float playerHealth;

    [SerializeField] private float maxHealth = 100;

    [SerializeField] private Image hpBar;

    [SerializeField] private float iFrameTimer;
    [SerializeField] private float iFrameDuration;

    [SerializeField] private float moveSpeed = 4f;


    private float _dashCooldownTimer = 0f;
    private Vector3 _dashDirection;
    private float _yaw = 0f;
    private float _botch = 0f;
    private bool _isDashing = false;
    private float gravity = 9.81f;

    private float vertVelocity;

    //public Vector3 velocity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _playerCamera = Camera.main;
    }

    void Start()
    {
        Cursor.visible = false;
        playerHealth = maxHealth;
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _dashAction = _playerInput.actions["Dash"];
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        if (_dashCooldownTimer > 0)
            _dashCooldownTimer -= Time.deltaTime;

        Vector2 moveValue = _moveAction.ReadValue<Vector2>();

        _playerMovement =
            transform.forward * (moveValue.y * moveSpeed) + transform.right * (moveValue.x * moveSpeed);

        _playerMovement.y = VerticalForceCalc();

        _characterController.Move(_playerMovement * Time.deltaTime);
        //Anim();
        _animator.SetFloat("Speed", moveValue.magnitude);


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

    private float VerticalForceCalc()
    {
        if (_characterController.isGrounded)
        {
            vertVelocity = -1f;
        }
        else
        {
            vertVelocity -= gravity * Time.deltaTime;
        }

        return vertVelocity;
    }

    private void Anim()
    {
        while (_playerMovement.x > 0 || _playerMovement.z > 0)
        {
            _animator.SetFloat("Speed", 1f);
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

    public void TakeDamage(float dmg)
    {
        _animator.SetTrigger("Hit");
        playerHealth -= dmg;
        // iFrameTimer = iFrameDuration;
        // _iFrames = true;
        if (playerHealth <= 0)
        {
            Kill();
        }

        UpdateHPBar();
    }

    private void Kill()
    {
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.GameOver();
        }
    }

    void UpdateHPBar()
    {
        hpBar.fillAmount = playerHealth / maxHealth;
    }
}