using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementInput : MonoBehaviour
{
    [Header("Public Variables")]
    public float desiredRotationSpeed = 0.1f;
    public float speed;
    public float allowPlayerRotation = 0.1f;
    public float rotationSpeed = 0.1f;
    public bool blockRotationPlayer;
    public float fallSpeed = 0.2f;
    public float acceleration = 1f;
    public Vector2 moveAxis;

    [Space]
    [Header("States")]
    [SerializeField]
    private bool _isGrounded;

    [Space]
    [Header("Animation Smoothing")]
    [Range(0f, 1f)]
    public float startAnimTime = 0.3f;
    [Range(0f, 1f)]
    public float stopAnimTime = 0.15f;

    private Vector3 _desiredMoveDirection;
    private float _verticalVel;
    private Vector3 _moveVector;

    private Animator _animator;
    private Camera _camera;
    private CharacterController _characterController;


    void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main;
        _characterController = GetComponent<CharacterController>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        InputMagnitude();

        _isGrounded = _characterController.isGrounded;
        if (_isGrounded)
        {
            _verticalVel -= 0;
        }
        else
        {
            _verticalVel -= 1;
        }
        _moveVector = new Vector3(0, _verticalVel * fallSpeed * Time.deltaTime, 0);
        _characterController.Move(_moveVector);
    }

    void PlayerMoveAndRotation()
    {
        var forward = _camera.transform.forward;
        var right = _camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        _desiredMoveDirection = forward * moveAxis.y + right * moveAxis.x;

        if (blockRotationPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_desiredMoveDirection), rotationSpeed * acceleration);
            _characterController.Move(_desiredMoveDirection * Time.deltaTime * (speed * acceleration));
        } else
        {
            _characterController.Move((transform.forward * moveAxis.y + transform.right * moveAxis.y) * Time.deltaTime * (speed * acceleration));
        }
    }

    public void LookAt(Vector3 pos)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), rotationSpeed);
    }

    public void RotateToCamera(Transform t)
    {
        var forward = _camera.transform.forward;
        var right = _camera.transform.right;

        _desiredMoveDirection = forward;
        Quaternion lookAtRotation = Quaternion.LookRotation(_desiredMoveDirection);
        Quaternion lookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, lookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        t.rotation = Quaternion.Slerp(transform.rotation, lookAtRotationOnly_Y, rotationSpeed);
    }

    void InputMagnitude()
    {
        float inputMagnitude = new Vector2(moveAxis.x, moveAxis.y).sqrMagnitude;

        if (inputMagnitude > allowPlayerRotation)
        {
            _animator.SetFloat("InputMagnitude", inputMagnitude * acceleration, startAnimTime, Time.deltaTime);
            PlayerMoveAndRotation();
        }
        else
        {
            _animator.SetFloat("InputMagnitude", inputMagnitude * acceleration, stopAnimTime, Time.deltaTime);
        }
    }

    #region Input

    public void OnMove(InputValue value)
    {
        Debug.Log(value);
        moveAxis.x = value.Get<Vector2>().x;
        moveAxis.y = value.Get<Vector2>().y;
    }

    #endregion

    private void OnDisable()
    {
        _animator.SetFloat("InputMagnitude", 0);
    }
}
