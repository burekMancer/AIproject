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
        rage
        
        
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

        switch (CurState)
        {
            case BossStates.idle:
                if (waypoint == null)
                {
                    Debug.Log("waypoint is null");
                    break;
                }
                if (!_agent.pathPending && _agent.remainingDistance > 5f )
                {
                    _agent.SetDestination(waypoint.position);
                }
                else
                {
                    _agent.ResetPath();
                    _agent.velocity = Vector3.zero;
                }
                break;
            case BossStates.chasing:
                _agent.SetDestination(player.position);
                break;
            case BossStates.attacking:
                break;
            case BossStates.rage :
                break;
        }

    }
}
