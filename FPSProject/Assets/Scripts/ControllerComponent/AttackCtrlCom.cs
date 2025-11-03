using System.Linq;
using UnityEngine;

public class AttackCtrlCom : ControllerComponentBase
{
	private Weapon _targetWeapon;
	private WeaponFxVisual _targetWeaponVisual;
	[SerializeField]
	private Camera _targetCamera;
	[SerializeField]
	private Transform _traWeaponParent;
	[SerializeField]
	private WeaponFxVisual _defaultWeaponVisual;

	[Header("Debug")]
	public GameObject[] debugWeapons;

	private float? _lastAttackTime;
	private int _attackIndex;
	private bool _isAttacking;
	private bool _isMoving;
	private bool _weaponSelected;

	public override void Initialize(ControllerBase parentCtrl, int id, bool isLocal)
	{
		base.Initialize(parentCtrl, id, isLocal);

		SetWeapon(debugWeapons[0]);
	}

	public override void BindSnapshot(InputManager.ActionSnapshot snapshot)
	{
		base.BindSnapshot(snapshot);
		_isAttacking = snapshot.attack;
		_isMoving = snapshot.isMoving;

		if (snapshot.weapons.Any(i => i))
		{
			var weaponIndex = System.Array.FindIndex(snapshot.weapons, i => i);
			if (weaponIndex >= 0 && weaponIndex < debugWeapons.Length)
			{
				SetWeapon(debugWeapons[weaponIndex]);
			}
		}
	}

	protected override void UpdateLocal()
	{
		base.UpdateLocal();

		if (_weaponSelected == false)
			return;

		if (_isAttacking)
		{
			if (_lastAttackTime.HasValue == false || Time.time - _lastAttackTime >= _targetWeapon.cooldown)
			{
				Attack();
				_lastAttackTime = Time.time;
			}
		}
		else if (_lastAttackTime.HasValue && Time.time - _lastAttackTime >= _targetWeapon.cooldown)
		{
			_attackIndex = 0;
			_lastAttackTime = null;
		}
	}

	public void SetWeapon(GameObject objWeapon)
	{
		foreach (Transform tra in _traWeaponParent)
		{
			Destroy(tra.gameObject);
		}

		var weaponInstance = Instantiate(objWeapon, _traWeaponParent);
		_targetWeapon = weaponInstance.GetComponent<Weapon>();
		_targetWeaponVisual = weaponInstance.TryGetComponent<WeaponFxVisual>(out var visual) ? visual : _defaultWeaponVisual;
		_weaponSelected = _targetWeapon != null;
	}

	private void Attack()
	{
		_attackIndex += 1;

		var result = _targetWeapon.Shot(new(
			_isMoving,
			_parentCtrl.GetCtrlCom<MovementCtrlCom>().characterController.isGrounded == false,
			_attackIndex,
			_targetCamera
		));

		if (result.HasHit)
		{
			var hitCollider = result.Hit.collider;
			var target = hitCollider.GetComponentInParent<StateCtrlCom>();
			if (target)
			{
				var damage = _targetWeapon.damage;
				if (hitCollider.CompareTag("Head"))
					damage *= 4;
				target.TakeDamage(damage, _id);
			}
		}

		_targetWeaponVisual.OnShoot(result);

		EventManager.Get.DispatchEvent(EventType.LocalPlayerAttack, new LocalPlayerAttackEventArgs(_attackIndex));
	}
}