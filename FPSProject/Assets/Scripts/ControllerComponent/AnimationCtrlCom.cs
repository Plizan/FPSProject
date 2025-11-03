using UnityEngine;

public class AnimationCtrlCom : ControllerComponentBase
{
	[SerializeField]
	private GameObject _objRemoteTarget;
	[SerializeField]
	private GameObject _objLocalTarget;

	public override void Initialize(ControllerBase parentCtrl, int id, bool isLocal)
	{
		base.Initialize(parentCtrl, id, isLocal);
		_objLocalTarget.SetActive(isLocal);
		_objRemoteTarget.SetActive(isLocal == false);
	}
}