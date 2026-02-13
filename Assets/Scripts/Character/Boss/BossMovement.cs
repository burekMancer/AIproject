using UnityEngine;
using UnityEngine.AI;
using NUnit.Framework;
using System.Collections.Generic;

public class BossMovement : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private bool _isAttacking = false;
    private float _attackCooldown = 5f;
    private float _attackCooldownTimer = 0f;


    public Transform waypoint;
    public Transform player;
    public float speed;

    public enum BossStates
    {
        idle,
        chasing,
        attacking,
        //rage
    }

    public BossStates CurState = BossStates.idle;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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


        HandleTransitions();
        HandleState();
    }

    private void HandleTransitions()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (_attackCooldownTimer > 0)
            _attackCooldownTimer -= Time.deltaTime;

        switch (CurState)
        {
            case BossStates.idle:
                if (dist < 40f)
                    CurState = BossStates.chasing;
                break;

            case BossStates.chasing:
                if (dist <= _agent.stoppingDistance)
                    CurState = BossStates.attacking;
                break;

            case BossStates.attacking:
                if (dist > _agent.stoppingDistance + 1f)
                    CurState = BossStates.chasing;
                break;
        }
    }

    private void HandleState()
    {
        switch (CurState)
        {
            case BossStates.idle:
                _agent.isStopped = true;
                Idle();
                break;

            case BossStates.chasing:
                _agent.isStopped = false;
                Chase();
                break;

            case BossStates.attacking:
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

        if (!_isAttacking && _attackCooldownTimer <= 0)
        {
            PerformAttack();
        }

        _agent.isStopped = false;
    }


    private void PerformAttack()
    {
        _isAttacking = true;
        _animator.SetTrigger("Attack");
        _attackCooldownTimer = _attackCooldown;
        _isAttacking = false;
    }
}