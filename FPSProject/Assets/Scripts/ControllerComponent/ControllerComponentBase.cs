using UnityEngine;

public class ControllerComponentBase : MonoBehaviour
{
	protected int _id;
	protected bool _isLocal = false;
	protected ControllerBase _parentCtrl;

	public virtual void Initialize(ControllerBase parentCtrl, int id, bool isLocal)
	{
		_parentCtrl = parentCtrl;
		_id = id;
		_isLocal = isLocal;
	}
	
	public virtual void BindSnapshot(InputManager.ActionSnapshot snapshot)
	{
	}
}