using UnityEngine;

public class Weapon : MonoBehaviour
{
	public readonly struct FireContext
	{
		public bool IsMoving { get; }
		public bool IsJumping { get; }
		public int AttackIndex { get; }

		public bool IsTight(int tightSpreadThreshold) => IsJumping == false && IsMoving == false && AttackIndex <= tightSpreadThreshold;

		public FireContext(bool isMoving, bool isJumping, int attackIndex)
		{
			IsMoving = isMoving;
			IsJumping = isJumping;
			AttackIndex = attackIndex;
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
	private const float spreadIncreaseExponent = 1.5f;

	public FireResult Shot(FireContext fireContext)
	{
		var step = _spreadStep;
		if (fireContext.IsJumping)
			step *= 2f;
		var spread = fireContext.IsTight(_tightSpreadThreshold)
			? _tightSpread
			: Mathf.Min(_maxSpread, _tightSpread + step * Mathf.Pow(fireContext.AttackIndex - _tightSpreadThreshold, spreadIncreaseExponent));

		var startPos = _traStartPoint.position;
		var dir = GetSpreadDirection(_traStartPoint.forward, spread);
		var hasHit = Physics.Raycast(startPos, dir, out var hit, _distance, ~0, QueryTriggerInteraction.Ignore);
		var end = hasHit ? hit.point : startPos + dir * _distance;
		
		return new(hasHit, startPos, end, hit, spread);
	}

	private Vector3 GetSpreadDirection(Vector3 forward, float angleDeg)
	{
		var o = Random.insideUnitCircle * angleDeg;
		var q = Quaternion.Euler(o.y, o.x, 0f);
		return (q * forward).normalized;
	}
}