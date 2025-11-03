using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponFxVisual : MonoBehaviour
{
	[SerializeField]
	private LineRenderer _tracer;
	[SerializeField]
	private float _tracerSpeed = 200f;
	[SerializeField]
	private float _tracerWidth = 0.02f;
	[SerializeField]
	private DecalProjector _urpDecalPrefab;
	[SerializeField]
	private float _decalLife = 15f;

	public void OnShoot(Weapon.FireResult result)
	{
		StartCoroutine(SpawnTracer(result.Start, result.End));

		if (result.HasHit)
		{
			var hit = result.Hit;
			var dp = Instantiate(_urpDecalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(-hit.normal));
			dp.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
			if (hit.collider)
				dp.transform.SetParent(hit.collider.transform, true);
			Destroy(dp.gameObject, _decalLife);
		}
	}

	private IEnumerator SpawnTracer(Vector3 start, Vector3 end)
	{
		var lineRenderer = Instantiate(_tracer);
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