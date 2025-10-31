using UnityEngine;

public class ControllerBase : MonoBehaviour
{
	protected ControllerComponentBase[] _components;
	public int id;
	protected bool _isLocal;

	public virtual void Initialize(int id, bool isLocal)
	{
		this.id = id;
		_isLocal = isLocal;
		
		_components = GetComponents<ControllerComponentBase>();
		
		foreach (var component in _components)
		{
			component.Initialize(this, id, isLocal);
		}
	}
}
