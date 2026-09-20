using TMPro;
using UnityEngine;

public class UICreditsWindowElement : MonoBehaviour
{
	public TextMeshProUGUI leftLabel;

	public TextMeshProUGUI rightLabel;

	public TextMeshProUGUI centerLabel;

	public void DrawCenter(string text)
	{
		TextMeshProUGUI label = ((centerLabel != null) ? centerLabel : leftLabel);
		SetLabel(leftLabel, string.Empty, isActive: false);
		SetRightLabel(string.Empty, isActive: false);
		SetLabel(centerLabel, string.Empty, isActive: false);
		SetLabel(label, text, isActive: true);
	}

	public void DrawRow(string leftText, string rightText)
	{
		SetLabel(leftLabel, leftText, isActive: true);
		SetRightLabel(rightText, isActive: true);
		SetLabel(centerLabel, string.Empty, isActive: false);
	}

	private void SetRightLabel(string text, bool isActive)
	{
		if (!(rightLabel == null))
		{
			((rightLabel.transform.parent != null) ? rightLabel.transform.parent.gameObject : rightLabel.gameObject).SetActive(isActive);
			rightLabel.text = text;
		}
	}

	private static void SetLabel(TextMeshProUGUI label, string text, bool isActive)
	{
		if (!(label == null))
		{
			label.gameObject.SetActive(isActive);
			label.text = text;
		}
	}
}
