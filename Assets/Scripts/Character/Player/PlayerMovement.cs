using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Cursor = UnityEngine.Cursor;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public Image hpBar;
    public ParticleSystem ps;
    public Collider weaponCollider;
    public TMP_Text healText;

    private CharacterController _characterController;
    private PlayerInput _playerInput;
    [SerializeField] private BossMovement boss;


    private Animator _animator;

    //Vectors
    private Vector3 _playerMovement;
    private Vector3 _dashDirection;

    private Camera _playerCamera;

    //InputActions
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _dashAction;
    private InputAction _healAction;
    private InputAction _attackAction;

    private float gravity = 9.81f;

    private bool _isAttacking;
    private bool _isHealing = false;
    private bool _isDashing = false;
    private bool _iFrames;
    private float rotSpeed = 31f;
    private float dashPower = 20f;
    private float dashTime = 0.3f;
    private float dashCooldown = 0.75f;
    private float _healValue = 40f;
    private float _dashCooldownTimer = 0f;
    private float _yaw = 0f;
    private float _botch = 0f;
    private float vertVelocity;

    public static int healCount = 5;

    [Header("Stats")] [SerializeField] private float playerHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float moveSpeed; //7.67
    [SerializeField] private float playerDamage;


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
        _iFrames = false;
        playerHealth = maxHealth;
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _dashAction = _playerInput.actions["Dash"];
        _healAction = _playerInput.actions["Heal"];
        _attackAction = _playerInput.actions["Attack"];
        UpdateHPBar();
        InvokeRepeating(nameof(UpdateHealFlask), 0, 0.1f);
    }


    void Update()
    {
        Movement();
        if (playerHealth > maxHealth)
            playerHealth = maxHealth;
        if (_attackAction.WasPressedThisFrame())
        {
            if (!_isDashing && !_isHealing && !_isAttacking)
            {
                TryAttack();
            }
        }

        if (_healAction.WasPressedThisFrame())
        {
            if (!_isDashing && !_isHealing && !_isAttacking)
            {
                TryHeal();
            }
        }

        //Debug.Log($"Dash:{_isDashing} Attack:{_isAttacking} Heal:{_isHealing}");
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
        _animator.SetFloat("Speed", moveValue.magnitude, 0.1f, Time.deltaTime);


        Vector2 lookValue = _lookAction.ReadValue<Vector2>();

        _yaw += lookValue.x * rotSpeed * Time.deltaTime;
        _botch -= lookValue.y * rotSpeed * Time.deltaTime;
        _botch = Mathf.Clamp(_botch, -89f, 89f);

        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);


        if (_dashAction.WasPressedThisFrame() && !_isDashing && _dashCooldownTimer <= 0 && !_isAttacking)
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

    private void StartIFrame()
    {
        _iFrames = true;
    }

    private void EndIFrame()
    {
        _iFrames = false;
    }

    private void TryAttack()
    {
        if (_isAttacking) return;
        _isAttacking = true;
        _animator.SetTrigger("PlayerAttack");
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;

        Collider[] hits = Physics.OverlapBox(
            weaponCollider.bounds.center,
            weaponCollider.bounds.extents,
            weaponCollider.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Boss"))
            {
                BossMovement boss = hit.GetComponent<BossMovement>();
                if (boss != null)
                {
                    boss.TakeDamage(playerDamage);
                }
            }
        }
    }

    public void HealingFrame()
    {
        _isHealing = true;
        playerHealth += _healValue;
        UpdateHPBar();
        healCount--;
        boss.Rage();
    }

    public void EndHealing()
    {
        _isHealing = false;
    }


    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
        _isAttacking = false;
    }


    public void TakeDamage(float dmg)
    {
        if (_iFrames) return;
        _animator.SetTrigger("Hit");
        InterruptAttack();
        playerHealth -= dmg;
        if (playerHealth <= 0)
        {
            Kill();
        }

        UpdateHPBar();
    }

    public void InterruptAttack()
    {
        _isAttacking = false;
        _isHealing = false;
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

    void UpdateHealFlask()
    {
        healText.text = (healCount).ToString();
    }

    private void TryHeal()
    {
        if (_isHealing) return;
        if (healCount > 0 && playerHealth < 100)
        {
            _isHealing = true;
            _animator.SetTrigger("Heal");
        }
    }
}