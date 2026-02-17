using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class BossMovement : MonoBehaviour
{
    //Drag n Drops
    private Animator _animator;
    private NavMeshAgent _agent;
    public ParticleSystem aoe;
    public ParticleSystem aoeWarn;
    public Collider rightHand;
    public Collider spinCollider;
    public Transform player;
    public Image hp;

    //attack related 

    //bools
    private bool _isTelegraphingSpin;

    private bool _isAttacking;

    //Base CDs
    private float _telegraphDuration = 1f;
    private float _meleeCooldown = 4f;
    private float _spinCooldown = 20f;
    private float _timeBetweenAttacks = 2.3f;

    //Timers
    private float _spinCooldownTimer = 0f;
    private float _meleeCooldownTimer = 0f;
    private float _hurtTimer;
    private float _telegraphTimer = 0f;
    private float _attackTimer = 0f;

    [Header("Stats")] // 
    public float health;

    public float bossHealth;
    public float meleeRange;
    public float spinRange;
    public float speed;
    public float hurtCooldown;
    public float dmg;
    public float spindmg;

    public enum BossStates
    {
        Idle,
        Chasing,
        Attacking,
        //rage
    }

    public BossStates CurState = BossStates.Idle;


    void Start()
    {
        _hurtTimer = 0;
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        UpdateHPBar();
    }


    void Update()
    {
        float speedPercent = 0f;

        if (!_agent.pathPending && _agent.remainingDistance > _agent.stoppingDistance)
        {
            speedPercent = _agent.velocity.magnitude;
        }

        if (speedPercent < 0.1f)
            speedPercent = 0f;


        _animator.SetFloat("speed", speedPercent);
        if (_hurtTimer > 0)
            _hurtTimer -= Time.deltaTime;

        HandleTransitions();
        HandleState();
    }

    private void HandleTransitions()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (_meleeCooldownTimer > 0)
            _meleeCooldownTimer -= Time.deltaTime;
        if (_spinCooldownTimer > 0)
            _spinCooldownTimer -= Time.deltaTime;
        if (_attackTimer > 0)
            _attackTimer -= Time.deltaTime;

        switch (CurState)
        {
            case BossStates.Idle:
                if (dist < 40f)
                    CurState = BossStates.Chasing;
                break;

            case BossStates.Chasing:
                if (dist <= meleeRange || dist > meleeRange && dist <= spinRange)
                    CurState = BossStates.Attacking;

                break;


            case BossStates.Attacking:
                if (dist > _agent.stoppingDistance + 1f)
                    CurState = BossStates.Chasing;
                break;
            // case BossStates.SpinAttack:
            //     if (dist > spinRange)
            //         CurState = BossStates.Chasing;
            //     break;
        }
    }

    private void HandleState()
    {
        switch (CurState)
        {
            case BossStates.Idle:
                _agent.isStopped = true;
                Idle();
                break;

            case BossStates.Chasing:
                _agent.isStopped = false;
                Chase();
                break;

            case BossStates.Attacking:
                _agent.isStopped = true;
                Attack();
                break;
        }
    }


    private void Idle()
    {
        _agent.isStopped = true;


        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 3f * Time.deltaTime);
        }
    }


    private void Chase()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPosition = player.position + direction * _agent.stoppingDistance;

        _agent.SetDestination(targetPosition);
    }


    private void Attack()
    {
        _agent.isStopped = true;

        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (!_isAttacking)
        {
            if (dist <= meleeRange && _meleeCooldownTimer <= 0 && _attackTimer <= 0 && !_isTelegraphingSpin)
                PerformAttack();
            else if (!_isAttacking && _spinCooldownTimer <= 0 && _attackTimer <= 0 && !_isTelegraphingSpin)
            {
                _agent.isStopped = true;
                StartCoroutine(TelegraphSpin());
            }
        }
    }

    private void PerformAttack()
    {
        _isAttacking = true;
        //Range();
        _animator.SetTrigger("Attack");
    }

    private IEnumerator TelegraphSpin()
    {
        _isTelegraphingSpin = true;

        _agent.isStopped = true;
        // glow mb?
        aoeWarn.Clear();
        aoeWarn.Play();


        print("Telegraph spin!");

        yield return new WaitForSeconds(_telegraphDuration);


        PerformSpinAttack();

        _isTelegraphingSpin = false;
    }

    private void PerformSpinAttack()
    {
        _isAttacking = true;
        print("spin attack");
        _animator.SetTrigger("Spin");
    }

    public void StartAttackF()
    {
        rightHand.enabled = true;
    }

    public void EndAttackF()
    {
        rightHand.enabled = false;
        _isAttacking = false;
        _meleeCooldownTimer = _meleeCooldown;
        _attackTimer = _timeBetweenAttacks;
    }

    private void StartSpinF()
    {
        _agent.updateRotation = false;
        // _agent.isStopped = true;
        spinCollider.enabled = true;
        aoe.Clear();
        aoe.Play();
    }

    private void EndSpinF()
    {
        _agent.isStopped = false;
        _agent.updateRotation = true;
        spinCollider.enabled = false;
        aoe.Stop();
        _attackTimer = _timeBetweenAttacks;
        _spinCooldownTimer = _spinCooldown;
        _isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!rightHand.enabled && !spinCollider.enabled) return;

        if (rightHand.enabled && other.CompareTag("Player") && _hurtTimer <= 0)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            player.TakeDamage(dmg);
            _hurtTimer = hurtCooldown;
        }
        else if (spinCollider.enabled && other.CompareTag("Player") && _hurtTimer <= 0)
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            player.TakeDamage(spindmg);
            _hurtTimer = hurtCooldown;
        }
    }

    public void TakeDamage(float dmg)
    {
        print("took dmg");
        health -= dmg;
        if (health <= 0)
        {
            Die();
        }

        UpdateHPBar();
    }

    public void Die()
    {
        _animator.SetTrigger("BossDeath");
        if (!GameManager.instance.isGameOver)
        {
            GameManager.instance.Victory();
        }
    }

    private void BossDeathAnimation()
    {
        Destroy(gameObject);
    }

    void UpdateHPBar()
    {
        hp.fillAmount = health / bossHealth;
    }
}