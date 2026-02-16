using UnityEngine;
using UnityEngine.AI;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.Serialization;

public class BossMovement : MonoBehaviour
{
    private bool _isTelegraphingSpin = false;
    private float _telegraphTimer = 0f;
    private float _telegraphDuration = 0.5f;
    private Animator _animator;
    private NavMeshAgent _agent;
    private bool _isAttacking = false;
    private float _attackCooldown = 4f;
    private float _attackCooldownTimer = 0f;
    private float _spinCooldownTimer = 0f;
    private float _spinCooldown = 10f;
    private float _hurtTimer;
    private float _timeBetweenAttacks = 1.2f;
    private float _attackTimer = 0f;


    public float meleeRange;
    public ParticleSystem aoe;
    public ParticleSystem aoeWarn;
    public float spinRange;
    public Collider rightHand;
    public Collider spinCollider;
    public Transform waypoint;
    public Transform player;
    public float speed;
    public float hurtCooldown;
    public float dmg;
    public float spindmg;

    public enum BossStates
    {
        Idle,
        Chasing,
        Attacking,
        //SpinAttack,
        //rage
    }

    public BossStates CurState = BossStates.Idle;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _hurtTimer = 0;
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
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

        if (_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;
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
            // case BossStates.SpinAttack:
            //     Spin();
            //     break;
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
            if (dist <= meleeRange && _attackCooldownTimer <= 0 && _attackTimer <= 0)
                PerformAttack();
            else if (!_isAttacking && _spinCooldownTimer <= 0 && !_isTelegraphingSpin)
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
        _attackCooldownTimer = _attackCooldown;
        _attackTimer = _timeBetweenAttacks;
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
        _attackTimer = _timeBetweenAttacks;
        _spinCooldownTimer = _spinCooldown;
    }

    public void StartAttackF()
    {
        rightHand.enabled = true;
    }

    public void EndAttackF()
    {
        rightHand.enabled = false;
        _isAttacking = false;
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
}