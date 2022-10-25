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
    [SerializeField] private GameObject _punchParticles;
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

        if (_enemyDetection.CurrentTarget() == null)
        {
            if (_enemyManager.AliveEnemyCount() == 0)
            {
                Attack(null, 0);
                return;
            } else
            {
                _lockedTarget = _enemyManager.RandomEnemy();
            }
        }

        if (_enemyDetection.InputMagnitude() > .2f)
            _lockedTarget = _enemyDetection.CurrentTarget();

        if (_lockedTarget == null)
            _lockedTarget = _enemyManager.RandomEnemy();

        Attack(_lockedTarget, TargetDistance(_lockedTarget));
    }

    public void Attack(EnemyController target, float distance)
    {
        _attacks = new string[] {"Attack1", "Attack2", "Attack3", "Attack4", "Attack5", "GroundPunch" };

        if (target == null)
        {
            AttackType("GroundPunch", .25f, null, 0);
            return;
        }

        if (distance < 15f)
        {
            _animationCount = (int)Mathf.Repeat((float)_animationCount + 1, (float)_attacks.Length);
            string attackString = isLastHit() ? _attacks[Random.Range(0, _attacks.Length)] : _attacks[_animationCount];
            AttackType(attackString, _attackCooldown, target, .75f);
        } else
        {
            _lockedTarget = null;
            AttackType("GroundPunch", .25f, null, 0);
        }
    }

    void AttackType(string attackTrigger, float cooldown, EnemyController target, float movementDuration)
    {
        _animator.SetTrigger(attackTrigger);

        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);

        _attackCoroutine = StartCoroutine(AttackCoroutine(isLastHit() ? 1.5f : cooldown));

        if (isLastHit())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
            return;

        target.StopMoving();
        MoveTowardsTarget(target, movementDuration);

        IEnumerator AttackCoroutine(float duration)
        {
            _movementInput.acceleration = 0;
            isAttackingEnemy = true;
            _movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            _movementInput.enabled = true;
            LerpCharacterAcceleration();
        }

        IEnumerator FinalBlowCoroutine()
        {
            Time.timeScale = .5f;
            yield return new WaitForSecondsRealtime(2);
            Time.timeScale = 1f;
        }
    }

    void MoveTowardsTarget(EnemyController target, float duration)
    {
        OnTrajectory.Invoke(target);
        transform.DOLookAt(target.transform.position, .25f);
        transform.DOMove(TargetOffset(target.transform), duration);
    }

    void CounterCheck()
    {
        if (isCountering || isAttackingEnemy || !_enemyManager.AnEnemyIsPreparingAttack())
            return;

        _lockedTarget = ClosestEnemyCounter();
        OnCounterAttack.Invoke(_lockedTarget);

        if (TargetDistance(_lockedTarget) > 2)
        {
            Attack(_lockedTarget, TargetDistance(_lockedTarget));
            return;
        }

        float duration = .2f;
        _animator.SetTrigger("Dodge");
        transform.DOLookAt(_lockedTarget.transform.position, .2f);
        transform.DOMove(transform.position + _lockedTarget.transform.forward, duration);

        if (_counterCoroutine != null)
            StopCoroutine(CounterCoroutine());

        _counterCoroutine = StartCoroutine(CounterCoroutine());

        IEnumerator CounterCoroutine()
        {
            isCountering = true;
            _movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(_lockedTarget, TargetDistance(_lockedTarget));
            isCountering = false;
        }
    }

    public void DamageEvent()
    {
        _animator.SetTrigger("Hit");

        if (_damageCoroutine != null)
            StopCoroutine(_damageCoroutine);

        _damageCoroutine = StartCoroutine(DamageCoroutine());

        IEnumerator DamageCoroutine()
        {
            _movementInput.enabled = false;
            yield return new WaitForSeconds(.5f);
            _movementInput.enabled = true;
            LerpCharacterAcceleration();
        }
    }

    public void HitEvent()
    {
        if (_lockedTarget == null || _enemyManager.AliveEnemyCount() == 0)
            return;

        OnHit.Invoke(_lockedTarget);
        _impulseSource.GenerateImpulse();

        _punchParticles.transform.position = _punchPosition.position;
        foreach (ParticleSystem particle in _punchParticles.GetComponentsInChildren<ParticleSystem>())
        {
            particle.Play();
        }
    }

    void LerpCharacterAcceleration()
    {
        _movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration) => _movementInput.acceleration = acceleration));
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    float TargetDistance(EnemyController target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    bool isLastHit()
    {
        if (_lockedTarget == null)
            return false;

        return _enemyManager.AliveEnemyCount() == 1 && _lockedTarget.health <= 1;
    }

    EnemyController ClosestEnemyCounter()
    {
        float minDistance = 100f;
        int finalIndex = 0;

        for (int i = 0; i < _enemyManager.allEnemies.Length; i++)
        {
            EnemyController enemy = _enemyManager.allEnemies[i].enemyController;

            if (enemy.IsPreparingAttack())
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return _enemyManager.allEnemies[finalIndex].enemyController;
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
