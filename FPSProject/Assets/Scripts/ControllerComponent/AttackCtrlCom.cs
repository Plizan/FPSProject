using UnityEngine;

public class AttackCtrlCom : ControllerComponentBase
{
	private Weapon _targetWeapon;
	private WeaponFxVisual _targetWeaponVisual;
	[SerializeField]
	private Transform _traCamera;
	[SerializeField]
	private Transform _traWeapon;
	[SerializeField]
	private WeaponFxVisual _defaultWeaponVisual;

	private float? _lastAttackTime;
	private int _attackIndex;
	private bool _isAttacking;
	private bool _isMoving;

	public override void BindSnapshot(InputManager.ActionSnapshot snapshot)
	{
		base.BindSnapshot(snapshot);
		_isAttacking = snapshot.attack;
		_isMoving = snapshot.isMoving;
	}

	protected override void UpdateLocal()
	{
		base.UpdateLocal();

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
		
	}

	private void Attack()
	{
		_attackIndex += 1;

		var result = _targetWeapon.Shot(new(
			_isMoving,
			_parentCtrl.GetCtrlCom<MovementCtrlCom>().characterController.isGrounded == false,
			_attackIndex
		));

		if (result.HasHit)
		{
			var hitCollider = result.Hit.collider;
			var target = hitCollider.GetComponentInParent<StateCtrlCom>();
			if (target != null)
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