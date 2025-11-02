using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : ManagerBase
{
	public struct ActionSnapshot
	{
		public Vector2 move;
		public bool isMoving;
		public Vector2 rotate;
		public bool jump;
		public bool crouch;
		public bool walk;
		public bool attack;
		public bool[] skills;
	}
	
	public static InputManager Get => Managers.Input;

	[SerializeField]
	private InputActionAsset _inputActionAsset;

	private InputActionMap _player;
	private InputAction _move;
	private InputAction _look;
	private InputAction _jump;
	private InputAction _attack;
	private InputAction _crouch;
	private InputAction _walk;
	private List<InputAction> _skills = new();

	public override void Initialize()
	{
		base.Initialize();

		_player = _inputActionAsset.FindActionMap("Player", true);
		_move = _player.FindAction("Move", true);
		_look = _player.FindAction("Look", true);
		_attack = _player.FindAction("Attack", true);
		_crouch = _player.FindAction("Crouch", true);
		_jump = _player.FindAction("Jump", true);
		_walk = _player.FindAction("Walk", true);

		_skills.Clear();
		foreach (var a in _player.actions)
			if (a.name.StartsWith("Skill"))
				_skills.Add(a);
		
		_inputActionAsset.Enable();
	}

	private bool _isFocused = true;
	
	private void OnApplicationFocus(bool hasFocus)
	{
		_isFocused = hasFocus;
		Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = hasFocus == false;
	}

	public ActionSnapshot GetPlayerInputSnapshot()
	{
		var moveValue = _move.ReadValue<Vector2>();
		return new()
		{
			move = moveValue,
			isMoving = moveValue.sqrMagnitude > 1e-4f,
			rotate = _isFocused ? _look.ReadValue<Vector2>() : Vector2.zero,
			jump = _jump.WasPressedThisFrame(),
			crouch = _crouch.WasPressedThisFrame(),
			walk = _walk.IsPressed(),
			skills = _skills.ConvertAll(a => a.WasPressedThisFrame()).ToArray(),
			attack = _attack.IsPressed(),
		};
	}
}