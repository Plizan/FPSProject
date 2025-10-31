using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AttackCtrlCom : ControllerComponentBase
{
	[SerializeField]
	private int _cooldownFixedCount = 20;
	[SerializeField]
	private float _range = 100f;
	[SerializeField]
	private int _damage = 25;
	[SerializeField]
	private float _tightSpread = 0.3f;
	[SerializeField]
	private float _spreadStep = 0.7f;
	[SerializeField]
	private float _maxSpread = 8f;

	[SerializeField]
	private Transform _traTracerStartPoint;
	[SerializeField]
	private LineRenderer _tracerPrefab;
	[SerializeField]
	private float _tracerSpeed = 200f;
	[SerializeField]
	private float _tracerWidth = 0.02f;
	[SerializeField]
	private DecalProjector _urpDecalPrefab;
	[SerializeField]
	private float _decalLife = 15f;

	private Camera _targetCamera;
	private int _currentFixedCount;
	private bool _isAttacking;
	private bool _isMoving;

	public override void Initialize(ControllerBase parentCtrl, int id, bool isLocal)
	{
		base.Initialize(parentCtrl, id, isLocal);
		if (isLocal) _targetCamera = parentCtrl.GetComponent<PlayerCtrl>().targetCamera;
	}

	public override void BindSnapshot(InputManager.ActionSnapshot snapshot)
	{
		base.BindSnapshot(snapshot);
		_isAttacking = snapshot.attack;
		_isMoving = snapshot.isMoving;
	}

	private void FixedUpdate()
	{
		if (_isAttacking)
		{
			if (_currentFixedCount % _cooldownFixedCount == 0)
				Attack();
			++_currentFixedCount;
		}
		else if (_currentFixedCount != 0)
		{
			if (_currentFixedCount % _cooldownFixedCount == 0)
				_currentFixedCount = 0;
			else
				++_currentFixedCount;
		}
	}

	private void Attack()
	{
		var attackCount = _currentFixedCount / _cooldownFixedCount + 1;
		var isJumping = ((PlayerCtrl)_parentCtrl).movementCtrlCom.characterController.isGrounded == false;
		var step = _spreadStep;
		if (isJumping)
			step *= 2f;
		var spread = attackCount <= 3 && _isMoving == false && isJumping == false ? _tightSpread : Mathf.Min(_maxSpread, _tightSpread + step * (attackCount - 3) * (attackCount - 3));

		var camPos = _targetCamera.transform.position;
		var dir = GetSpreadDirection(_targetCamera.transform.forward, spread);

		var hasHit = Physics.Raycast(camPos, dir, out var hit, _range, ~0, QueryTriggerInteraction.Ignore);
		var end = hasHit ? hit.point : camPos + dir * _range;

		StartCoroutine(SpawnTracer(_traTracerStartPoint.transform.position, end));

		if (hasHit)
		{
			var dp = Instantiate(_urpDecalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(-hit.normal));
			dp.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
			if (hit.collider)
				dp.transform.SetParent(hit.collider.transform, true);
			Destroy(dp.gameObject, _decalLife);

			var target = hit.collider.GetComponentInParent<StateCtrlCom>();
			if (target != null)
			{
				var damage = _damage;
				if (hit.collider.CompareTag("Head"))
					damage *= 4;
				target.TakeDamage(damage, _id);
			}
		}

		EventManager.Get.DispatchEvent(EventType.LocalPlayerAttack, new LocalPlayerAttackEventArgs());
	}

	private Vector3 GetSpreadDirection(Vector3 forward, float angleDeg)
	{
		var o = Random.insideUnitCircle * angleDeg;
		var q = Quaternion.Euler(o.y, o.x, 0f);
		return (q * forward).normalized;
	}

	private IEnumerator SpawnTracer(Vector3 start, Vector3 end)
	{
		var lineRenderer = Instantiate(_tracerPrefab);
		lineRenderer.positionCount = 2;
		lineRenderer.startWidth = _tracerWidth;
		lineRenderer.endWidth = _tracerWidth;
		lineRenderer.SetPosition(0, start);
		lineRenderer.SetPosition(1, start);
		var dist = Vector3.Distance(start, end);
		var t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * (_tracerSpeed / Mathf.Max(0.001f, dist));
			var p = Vector3.Lerp(start, end, t);
			lineRenderer.SetPosition(1, p);
			yield return null;
		}
		lineRenderer.SetPosition(1, end);
		yield return null;
		Destroy(lineRenderer.gameObject);
	}
}