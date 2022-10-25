using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private MovementInput _movementInput;

    public LayerMask layerMask;

    [SerializeField] private Vector3 _inputDirection;
    [SerializeField] private EnemyController _currentTarget;


    // Start is called before the first frame update
    void Start()
    {
        _movementInput = GetComponent<MovementInput>();
    }

    // Update is called once per frame
    void Update()
    {
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        _inputDirection = forward * _movementInput.moveAxis.y + right * _movementInput.moveAxis.x;
        _inputDirection = _inputDirection.normalized;

        RaycastHit hit;

        if (Physics.SphereCast(transform.position, 3f, _inputDirection, out hit, 10, layerMask))
        {
            if (hit.collider.transform.GetComponent<EnemyController>().IsAttackable())
            {
                _currentTarget = hit.collider.transform.GetComponent<EnemyController>();
            }
        }
    }

    public EnemyController CurrentTarget()
    {
        return _currentTarget;
    }

    public void SetCurrentTarget(EnemyController target)
    {
        _currentTarget = target;
    }

    public float InputMagnitude()
    {
        return _inputDirection.magnitude;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, .2f);
        Gizmos.DrawRay(transform.position, _inputDirection);
        if (CurrentTarget() != null)
            Gizmos.DrawSphere(CurrentTarget().transform.position, .25f);
    }
}
