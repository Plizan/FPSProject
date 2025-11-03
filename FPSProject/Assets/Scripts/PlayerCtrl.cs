using UnityEngine;

public class PlayerCtrl : ControllerBase
{
	public Camera targetCamera;

	public override void Initialize(int id, bool isLocal)
	{
		base.Initialize(id, isLocal);
		targetCamera.SetActive(_isLocal);

		if (_isLocal)
		{
			var mineLayer = LayerMask.NameToLayer("Mine");
			foreach (var tra in GetComponentsInChildren<Transform>(true))
			{
				tra.gameObject.layer = mineLayer;
			}
			
			GameManager.Get.StartGame();
		}
	}
	
	private void Update()
	{
		if (_isLocal == false)
			return;

		var snapshot = InputManager.Get.GetPlayerInputSnapshot();
		foreach (var component in _components)
		{
			component.BindSnapshot(snapshot);
		}
	}

	private void OnDestroy()
	{
		if (_isLocal)
			GameManager.Get.Dead();
	}
}