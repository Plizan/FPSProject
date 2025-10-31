using UnityEngine;

public class Managers : MonoBehaviour
{
	public static Managers Get => _instance;
	private static Managers _instance;
	
	public static GameManager Game { get; private set; }
	public static InputManager Input { get; private set; }
	public static UIManager UI { get; private set; }
	public static EventManager Event { get; private set; }

	private void Awake()
	{
		_instance = this;

		foreach (var i in GetComponents<ManagerBase>())
		{
			switch (i)
			{
				case GameManager manager:
					Game = manager;
					break;
				case InputManager manager:
					Input = manager;
					break;
				case UIManager manager:
					UI = manager;
					break;
				case EventManager manager:
					Event = manager;
					break;
			}

			i.Initialize();
		}
	}
}
