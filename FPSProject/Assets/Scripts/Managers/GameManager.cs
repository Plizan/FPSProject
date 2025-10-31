using UnityEngine;

public class GameManager : ManagerBase
{
	public static GameManager Get => Managers.Game;

	public enum GameState
	{
		Waiting,
		Starting,
		Playing,
		Result
	}

	private GameState _currentState;
	public Transform[] spawnPoints;
	public GameState currentState => _currentState;

	private void Awake()
	{
		_currentState = GameState.Starting;
	}

	public void StartGame()
	{
		_currentState = GameState.Playing;
	}

	public void Dead()
	{
		_currentState = GameState.Result;

		if (NetClient.Get && NetClient.Get.gameObject)
			Destroy(NetClient.Get.gameObject);
	}
}