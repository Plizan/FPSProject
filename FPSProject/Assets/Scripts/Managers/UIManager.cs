using TMPro;

public class UIManager : ManagerBase
{
	public static UIManager Get => Managers.UI;

	public TextMeshProUGUI txtHP;

	public void UpdateHP(int hp)
	{
		txtHP.text = hp.ToString();
	}
}