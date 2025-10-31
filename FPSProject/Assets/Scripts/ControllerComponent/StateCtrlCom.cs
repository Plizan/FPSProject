using UnityEngine;

public class StateCtrlCom : ControllerComponentBase
{
	[SerializeField]
	private int _hp = 100;
	public int HP => _hp;

	public void SetHP(int hp)
	{
		_hp = hp;
		if (_hp <= 0)
		{
			_hp = 0;
			Destroy(gameObject);
			Debug.Log($"Player {_id} is dead.");
		}
		UIManager.Get.UpdateHP(_hp);
	}

	public void TakeDamage(int damage, int attackerID)
	{
		NetClient.Get.TakeDamage(new()
		{
			Damage = damage,
			TargetID = _id,
			AttackerID = attackerID
		});
	}
}