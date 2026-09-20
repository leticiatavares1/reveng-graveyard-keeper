using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentIcon : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	public void Draw(string talentId, bool isActive = true)
	{
		label.text = (isActive ? talentId.FontIcon() : (talentId + "-inactive").FontIcon());
	}
}
