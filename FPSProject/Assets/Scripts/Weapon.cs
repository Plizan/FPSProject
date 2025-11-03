using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
	public readonly struct FireContext
	{
		public bool IsMoving { get; }
		public bool IsJumping { get; }
		public int AttackIndex { get; }
		public Camera TargetCamera { get; }
		public bool IsTight(int tightSpreadThreshold) => IsJumping == false && IsMoving == false && AttackIndex <= tightSpreadThreshold;

		public FireContext(bool isMoving, bool isJumping, int attackIndex, Camera targetCamera)
		{
			IsMoving = isMoving;
			IsJumping = isJumping;
			AttackIndex = attackIndex;
			TargetCamera = targetCamera;
		}
	}

	public readonly struct FireResult
	{
		public bool HasHit { get; }
		public Vector3 Start { get; }
		public Vector3 End { get; }
		public RaycastHit Hit { get; }
		public float Spread { get; }

		public FireResult(bool hasHit, Vector3 start, Vector3 end, RaycastHit hit, float spread)
		{
			HasHit = hasHit;
			Start = start;
			End = end;
			Hit = hit;
			Spread = spread;
		}
	}

	public float cooldown => _cooldown;
	public int damage => _damage;

	[SerializeField]
	private float _distance = 100f;
	[SerializeField]
	private int _damage = 25;
	[SerializeField]
	private float _tightSpread = 0.3f;
	[SerializeField]
	private float _spreadStep = 0.7f;
	[SerializeField]
	private float _maxSpread = 8f;
	[SerializeField]
	private float _cooldown = 0.4f;
	[SerializeField]
	private int _tightSpreadThreshold = 3;
	[SerializeField]
	private Transform _traStartPoint;

	private const float _spreadIncreaseExponent = 1.5f;
	private const float _muzzleSafety = 0.2f;
	private int _layerMask;

	private void Awake()
	{
		_layerMask = ~(1 << LayerMask.NameToLayer("Mine"));
	}

	public FireResult Shot(FireContext ctx)
	{
		var step = ctx.IsJumping ? _spreadStep * 2f : _spreadStep;
		var baseIdx = Mathf.Max(0, ctx.AttackIndex - _tightSpreadThreshold);
		var spread = ctx.IsTight(_tightSpreadThreshold) ? _tightSpread : Mathf.Min(_maxSpread, _tightSpread + step * Mathf.Pow(baseIdx, _spreadIncreaseExponent));
		spread = Mathf.Clamp(spread, 0f, _maxSpread);

		var cam = ctx.TargetCamera;
		var camOrigin = cam.transform.position;
		var camDir = cam.transform.forward;
		var hasCamHit = Physics.Raycast(camOrigin, camDir, out var camHit, _distance, _layerMask, QueryTriggerInteraction.Ignore);
		var aimPoint = hasCamHit ? camHit.point : camOrigin + camDir * _distance;

		var startPos = _traStartPoint.position;
		var dirToAim = (aimPoint - startPos).sqrMagnitude > 1e-12f ? (aimPoint - startPos).normalized : _traStartPoint.forward;
		var dir = GetSpreadToward(dirToAim, spread);

		var blockedNearMuzzle = Physics.Raycast(startPos, dir, out var nearHit, _muzzleSafety, _layerMask, QueryTriggerInteraction.Ignore);
		if (blockedNearMuzzle)
		{
			return new(true, startPos, nearHit.point, nearHit, spread);
		}

		var hasHit = Physics.Raycast(startPos, dir, out var hit, _distance, _layerMask, QueryTriggerInteraction.Ignore);
		var end = hasHit ? hit.point : startPos + dir * _distance;
		return new(hasHit, startPos, end, hit, spread);
	}

	private Vector3 GetSpreadToward(Vector3 dir, float angleDeg)
	{
		if (dir.sqrMagnitude < 1e-12f) dir = Vector3.forward;
		dir.Normalize();
		angleDeg = Mathf.Clamp(angleDeg, 0f, _maxSpread);

		var u = Random.value;
		var v = Random.value;

		var theta = 2f * Mathf.PI * u;
		var cosMax = Mathf.Cos(angleDeg * Mathf.Deg2Rad);
		var cosPhi = Mathf.Lerp(1f, cosMax, v);
		var sinPhi = Mathf.Sqrt(Mathf.Max(0f, 1f - cosPhi * cosPhi));

		var local = new Vector3(Mathf.Cos(theta) * sinPhi, Mathf.Sin(theta) * sinPhi, cosPhi);
		var basis = Quaternion.FromToRotation(Vector3.forward, dir);
		return (basis * local).normalized;
	}
}