using UnityEngine;
using UnityEngine.AI;
using NUnit.Framework;
using System.Collections.Generic;

public class BossMovement : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;


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
        _animator.SetFloat("speed", _agent.velocity.magnitude);

        HandleTransitions();
        HandleState();
    }

    private void HandleTransitions()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < 40f)
            CurState = BossStates.chasing;

        if (dist < 2f)
            CurState = BossStates.attacking;
    }

    private void HandleState()
    {
        switch (CurState)
        {
            case BossStates.idle:
                Idle();
                break;

            case BossStates.chasing:
                Chase();
                break;

            case BossStates.attacking:
                Attack();
                break;
        }
    }

    private void Idle()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < 40f)
        {
            CurState = BossStates.chasing;
            return;
        }
    }

    private void Chase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < 2f)
        {
            CurState = BossStates.attacking;
            return;
        }

        _agent.SetDestination(player.position);
    }

    private void Attack()
    {
        return;
    }
}