using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private CombatController _playerCombat;
    private EnemyManager _enemyManager;
    private EnemyDetection _enemyDetection;
    private CharacterController _characterController;

    [Header("Stats")]
    public int health = 3;
    private Vector3 _moveDirection;
    private float _moveSpeed = 1;

    [Space]
    [Header("States")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private bool _isLockedTarget;
    [SerializeField] private bool _isStunned;
    [SerializeField] private bool _isPreparingAttack;
    [SerializeField] private bool _isWaiting = true;
    [SerializeField] private bool _isRetreating;

    [Header("Polish")]
    [SerializeField] private ParticleSystem _counterParticle;

    [Space]
    private Coroutine _movementCoroutine;
    private Coroutine _damageCoroutine;
    private Coroutine _retreatCoroutine;
    private Coroutine _prepareAttackCoroutine;

    [Space]
    // Events
    public UnityEvent<EnemyController> OnDamage;
    public UnityEvent<EnemyController> OnStopMoving;
    public UnityEvent<EnemyController> OnRetreat;

    // Start is called before the first frame update
    void Start()
    {
        _enemyManager = GetComponentInParent<EnemyManager>();

        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();

        _playerCombat = FindObjectOfType<CombatController>();
        _enemyDetection = _playerCombat.GetComponentInChildren<EnemyDetection>();

        _playerCombat.OnHit.AddListener((x) => OnPlayerHit(x));
        _playerCombat.OnCounterAttack.AddListener((x) => OnPlayerCounter(x));
        _playerCombat.OnTrajectory.AddListener((x) => OnPlayerTrajectory(x));

        _movementCoroutine = StartCoroutine(EnemyMovement());
    }

    IEnumerator EnemyMovement()
    {
        yield return new WaitUntil(() => _isWaiting == true);

        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            int randomDir = Random.Range(0, 2);
            _moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            _isMoving = true;
        } else
        {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        _movementCoroutine = StartCoroutine(EnemyMovement());
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(_playerCombat.transform.position.x, transform.position.y, _playerCombat.transform.position.z));

        MoveEnemy(_moveDirection);
    }

    void OnPlayerHit(EnemyController target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            _damageCoroutine = StartCoroutine(HitCoroutine());

            _enemyDetection.SetCurrentTarget(null);
            _isLockedTarget = false;
            OnDamage.Invoke(this);
            
            health--;

            if (health <= 0)
            {
                Death();
                return;
            }

            _animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward / 2), .3f).SetDelay(.1f);

            StopMoving();
        }

        IEnumerator HitCoroutine()
        {
            _isStunned = true;
            yield return new WaitForSeconds(.5f);
            _isStunned = false;
        }
    }

    void OnPlayerCounter(EnemyController target)
    {
        if (target == this)
            PrepareAttack(false);
    }

    void OnPlayerTrajectory(EnemyController target)
    {
        if (target == this)
        {
            StopEnemyCoroutines();
            _isLockedTarget = true;
            PrepareAttack(false);
            StopMoving();
        }
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();

        _retreatCoroutine = StartCoroutine(PrepRetreat());

        IEnumerator PrepRetreat()
        {
            yield return new WaitForSeconds(1.4f);
            OnRetreat.Invoke(this);
            _isRetreating = true;
            _animator.SetBool("Retreating", true);
            _moveDirection = -Vector3.forward;
            _isMoving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, _playerCombat.transform.position) > 4);
            _isRetreating = false;
            _animator.SetBool("Retreating", false);
            StopMoving();

            _isWaiting = true;
            _movementCoroutine = StartCoroutine(EnemyMovement());
        }
    }

    public void SetAttack()
    {
        _isWaiting = false;

        _prepareAttackCoroutine = StartCoroutine(PrepAttack());

        IEnumerator PrepAttack()
        {
            PrepareAttack(true);
            yield return new WaitForSeconds(.2f);
            _moveDirection = Vector3.forward;
            _isMoving = true;
        }
    }

    void PrepareAttack(bool active)
    {
        _isPreparingAttack = active;

        if (active)
        {
            _counterParticle.Play();
        } else
        {
            StopMoving();
            _counterParticle.Clear();
            _counterParticle.Stop();
        }
    }

    void MoveEnemy(Vector3 direction)
    {
        _moveSpeed = 1;

        if (direction == Vector3.forward)
            _moveSpeed = 3;
        if (direction == -Vector3.forward)
            _moveSpeed = 2;

        _animator.SetFloat("InputMagnitude", Mathf.Abs((_characterController.velocity.normalized.magnitude * direction.z) / (5 / _moveSpeed)), .2f, Time.deltaTime);
        _animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        _animator.SetFloat("StrafeDirection", direction.normalized.x, .2f, Time.deltaTime);

        if (!_isMoving)
            return;

        Vector3 dir = (_playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir;
        Vector3 moveDir = Vector3.zero;

        Vector3 finalDirection = Vector3.zero;

        if (direction == Vector3.forward)
            finalDirection = dir;
        if (direction == Vector3.right || direction == Vector3.left)
            finalDirection = (pDir * direction.normalized.x);
        if (direction == -Vector3.forward)
            finalDirection = -transform.forward;

        if (direction == Vector3.right || direction == Vector3.left)
            _moveSpeed /= 1.5f;

        moveDir += finalDirection * _moveSpeed * Time.deltaTime;

        _characterController.Move(moveDir);


        if (!_isPreparingAttack)
            return;

        if (Vector3.Distance(transform.position, _playerCombat.transform.position) < 1)
        {
            StopMoving();
            
            if (!_playerCombat.isCountering && !_playerCombat.isAttackingEnemy)
                Attack();
            else 
                PrepareAttack(false);
        }
    }

    private void Attack()
    {
        transform.DOMove(transform.position + (transform.position / 1), .5f);
        _animator.SetTrigger("AirPunch");
    }

    public void HitEvent()
    {
        if (!_playerCombat.isCountering && !_playerCombat.isAttackingEnemy)
            _playerCombat.DamageEvent();

        PrepareAttack(false);
    }

    void Death()
    {
        StopEnemyCoroutines();

        this.enabled = false;
        _characterController.enabled = false;
        _animator.SetTrigger("Death");
        _enemyManager.SetEnemyAvailiability(this, false);
    }

    public void StopMoving()
    {
        _isMoving = false;
        _moveDirection = Vector3.zero;
        if (_characterController.enabled)
            _characterController.Move(_moveDirection);
    }

    void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (_isRetreating)
        {
            if (_retreatCoroutine != null)
            {
                StopCoroutine(_retreatCoroutine);
            }

            if (_prepareAttackCoroutine != null)
            {
                StopCoroutine(_prepareAttackCoroutine);
            }

            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }

            if (_movementCoroutine != null)
            {
                StopCoroutine(_movementCoroutine);
            }
        }
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return health > 0;
    }

    public bool IsPreparingAttack()
    {
        return _isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return _isRetreating;
    }

    public bool IsLockedTarget()
    {
        return _isLockedTarget;
    }

    public bool IsStunned()
    {
        return _isStunned;
    }

    #endregion
}
