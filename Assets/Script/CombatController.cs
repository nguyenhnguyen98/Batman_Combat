using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;

public class CombatController : MonoBehaviour
{
    private EnemyManager _enemyManager;
    private EnemyDetection _enemyDetection;
    private MovementInput _movementInput;
    private Animator _animator;
    private CinemachineImpulseSource _impulseSource;

    [Header("Target")]
    [SerializeField] private EnemyController _lockedTarget;

    [Space]
    [Header("Combat Settings")]
    [SerializeField] private float _attackCooldown;

    [Space]
    [Header("States")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;

    [Header("Public References")]
    [SerializeField] private Transform _punchPosition;
    //[SerializeField] private ParticleSystemScript _punchPosition;
    [SerializeField] private GameObject _lastHitCamera;
    [SerializeField] private Transform _lastHitFocusObject;

    private Coroutine _counterCoroutine;
    private Coroutine _attackCoroutine;
    private Coroutine _damageCoroutine;

    [Space]
    [Header("Events")]
    public UnityEvent<EnemyController> OnTrajectory;
    public UnityEvent<EnemyController> OnHit;
    public UnityEvent<EnemyController> OnCounterAttack;

    private int _animationCount = 0;
    private string[] _attacks;

    // Start is called before the first frame update
    void Start()
    {
        _enemyManager = FindObjectOfType<EnemyManager>();
        _animator = GetComponent<Animator>();
        _enemyDetection = GetComponent<EnemyDetection>();
        _movementInput = GetComponent<MovementInput>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;


    }

    void CounterCheck()
    {

    }

    void MoveTowardsTarget(EnemyController target, float duration)
    {
        //OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .2f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    #region Input

    private void OnCounter()
    {
        CounterCheck();
    }

    private void OnAttack()
    {
        AttackCheck();
    }

    #endregion
}
