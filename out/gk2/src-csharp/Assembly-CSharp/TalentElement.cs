using TMPro;
using UnityEngine;

public class TalentElement : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI talentValue;

	public void Draw(string fontIcon, int value = -1)
	{
		string text = ((value > -1) ? value.ToString() : "");
		talentValue.text = fontIcon + text;
	}
}
