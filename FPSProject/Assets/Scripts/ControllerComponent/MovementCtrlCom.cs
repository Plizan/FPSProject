using System;
using UnityEngine;

public class MovementCtrlCom : ControllerComponentBase, IEventHandler
{
	public bool isMoving { get; private set; }

	[SerializeField]
	private float _speed = 5f;
	[SerializeField]
	private float _rotSensitivity = 0.06f;
	[SerializeField]
	private float _jumpPower = 12f;
	[SerializeField]
	private float _startRecoil = .3f;
	[SerializeField]
	private float _maxRecoil = 1f;

	private const float minPitch = -85f;
	private const float maxPitch = 85f;

	[SerializeField]
	private Transform _traRotate;
	public CharacterController characterController;

	private float _yaw;
	private float _pitch;
	private bool _isCrouching;
	private float _verticalSpeed;
	private float _standHeight;
	private Vector3 _standCenter;
	private float _crouchHeight;
	private Vector3 _crouchCenter;
	private InputManager.ActionSnapshot _snapshot;

	private void Start()
	{
		if (_isLocal)
			EventManager.Get.AddHandler(EventType.LocalPlayerAttack, this);

		if (characterController == null)
			characterController = GetComponent<CharacterController>();

		_standHeight = characterController.height;
		_standCenter = characterController.center;
		_crouchHeight = _standHeight * 0.5f;
		_crouchCenter = new(_standCenter.x, _standCenter.y - (_standHeight - _crouchHeight) * 0.5f, _standCenter.z);
	}

	private void Update()
	{
		if (_isLocal == false)
			return;

		var dir = _snapshot.rotate;

		if (dir.sqrMagnitude > 1e-4f)
		{
			var dt = Time.deltaTime;
			_yaw += dir.x * _rotSensitivity * dt;
			var dPitch = -dir.y * _rotSensitivity * dt;
			_pitch = Mathf.Clamp(_pitch + dPitch, minPitch, maxPitch);
			transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
			_traRotate.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

			isMoving = true;
		}
	}

	private void FixedUpdate()
	{
		if (_isLocal == false)
			return;

		isMoving = false;

		var dt = Time.fixedDeltaTime;
		var travel = Vector3.zero;

		if (_snapshot.isMoving)
		{
			var dir = _snapshot.move;
			var moveLocal = new Vector3(dir.x, 0f, dir.y);
			if (moveLocal.sqrMagnitude > 1e-6f)
				moveLocal.Normalize();

			var moveWorld = transform.rotation * moveLocal;
			var speed = _speed;

			if (_isCrouching)
				speed *= 0.3333f;
			else if (_snapshot.walk)
				speed *= 0.5f;

			travel += moveWorld * (speed * dt);
			isMoving = true;
		}

		if (characterController.isGrounded && _verticalSpeed < 0f)
			_verticalSpeed = -2f;

		_verticalSpeed += Physics.gravity.y * dt;
		travel += Vector3.up * (_verticalSpeed * dt);

		characterController.Move(travel);
	}

	public override void BindSnapshot(InputManager.ActionSnapshot snapshot)
	{
		_snapshot = snapshot;

		base.BindSnapshot(snapshot);

		if (snapshot.jump)
			Jump();

		if (snapshot.crouch)
			Crouch();
	}

	private void Jump()
	{
		if (characterController.isGrounded == false)
			return;

		var power = _jumpPower;
		if (_isCrouching)
			power *= 0.5f;

		_verticalSpeed = power;
	}

	private void Crouch()
	{
		_isCrouching = !_isCrouching;
		transform.localScale = new(1f, _isCrouching ? 0.5f : 1f, 1f);

		if (_isCrouching)
		{
			characterController.height = _crouchHeight;
			characterController.center = _crouchCenter;
		}
		else
		{
			characterController.height = _standHeight;
			characterController.center = _standCenter;
		}
	}

	public void OnEvent(EventType eventType, EventTypesArgsBase args)
	{
		var attackIndex = args.Get<LocalPlayerAttackEventArgs>().attackIndex;
		var recoil = attackIndex <= 3 ? _startRecoil : Mathf.Min(_maxRecoil, _startRecoil + 0.1f * (attackIndex - 3));
		if (_isCrouching)
			recoil *= 0.5f;
		_pitch = Mathf.Clamp(_pitch - recoil, minPitch, maxPitch);
		_traRotate.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
	}
}