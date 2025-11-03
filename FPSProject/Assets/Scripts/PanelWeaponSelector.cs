using UnityEngine;
using UnityEngine.UI;

public class PanelWeaponSelector : MonoBehaviour
{
	[SerializeField]
	private Button btRifle;
	[SerializeField]
	private GameObject objRifle;
	[SerializeField]
	private Button btPistol;
	[SerializeField]
	private GameObject objPistol;

	private void Awake()
	{
		btRifle.SetOnClick(() => NetClient.Get.localPlayerCtrl.GetCtrlCom<AttackCtrlCom>().SetWeapon(objRifle));
		btPistol.SetOnClick(() => NetClient.Get.localPlayerCtrl.GetCtrlCom<AttackCtrlCom>().SetWeapon(objPistol));
	}
}