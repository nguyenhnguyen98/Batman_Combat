using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementInput : MonoBehaviour
{
	private Animator _animator;
	private Camera _camera;
	private CharacterController _controller;

	private Vector3 _desiredMoveDirection;
	private Vector3 _moveVector;

	public Vector2 moveAxis;
	private float _verticalVel;

	[Header("Settings")]
	[SerializeField] float _movementSpeed;
	[SerializeField] float _rotationSpeed = 0.1f;
	[SerializeField] float _fallSpeed = .2f;
	public float acceleration = 1;

	[Header("Booleans")]
	[SerializeField] bool _blockRotationPlayer;
	private bool _isGrounded;


	void Start()
	{
		_animator = this.GetComponent<Animator>();
		_camera = Camera.main;
		_controller = this.GetComponent<CharacterController>();
	}

	void Update()
	{
		InputMagnitude();

		_isGrounded = _controller.isGrounded;

		if (_isGrounded)
			_verticalVel -= 0;
		else
			_verticalVel -= 1;

		_moveVector = new Vector3(0, _verticalVel * _fallSpeed * Time.deltaTime, 0);
		_controller.Move(_moveVector);
	}

	void PlayerMoveAndRotation()
	{
		var camera = Camera.main;
		var forward = _camera.transform.forward;
		var right = _camera.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		_desiredMoveDirection = forward * moveAxis.y + right * moveAxis.x;

		if (_blockRotationPlayer == false)
		{
			//Camera
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_desiredMoveDirection), _rotationSpeed * acceleration);
			_controller.Move(_desiredMoveDirection * Time.deltaTime * (_movementSpeed * acceleration));
		}
		else
		{
			//Strafe
			_controller.Move((transform.forward * moveAxis.y + transform.right * moveAxis.y) * Time.deltaTime * (_movementSpeed * acceleration));
		}
	}

	public void LookAt(Vector3 pos)
	{
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), _rotationSpeed);
	}

	public void RotateToCamera(Transform t)
	{
		var forward = _camera.transform.forward;

		_desiredMoveDirection = forward;
		Quaternion lookAtRotation = Quaternion.LookRotation(_desiredMoveDirection);
		Quaternion lookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, lookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

		t.rotation = Quaternion.Slerp(transform.rotation, lookAtRotationOnly_Y, _rotationSpeed);
	}

	void InputMagnitude()
	{
		//Calculate the Input Magnitude
		float inputMagnitude = new Vector2(moveAxis.x, moveAxis.y).sqrMagnitude;

		//Physically move player
		if (inputMagnitude > 0.1f)
		{
			_animator.SetFloat("InputMagnitude", inputMagnitude * acceleration, .1f, Time.deltaTime);
			PlayerMoveAndRotation();
		}
		else
		{
			_animator.SetFloat("InputMagnitude", inputMagnitude * acceleration, .1f,Time.deltaTime);
		}
	}

	#region Input

	public void OnMove(InputValue value)
	{
		moveAxis.x = value.Get<Vector2>().x;
		moveAxis.y = value.Get<Vector2>().y;
	}

	#endregion

	private void OnDisable()
	{
		_animator.SetFloat("InputMagnitude", 0);
	}
}
