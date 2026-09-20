using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class LinkedEntityWidgetWithText : LinkedEntityWidget
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TextStyle prefixStyle;

	[SerializeField]
	private TextStyle headerStyle;

	public override void Redraw()
	{
		base.Redraw();
		headerStyle.ApplyStyle(this.text);
		string text = prefixStyle.ApplyStyleToString(data.GetLinkedEntityPrefix()) + ": ";
		this.text.text = LLBase.L(text + data.GetLinkedEntityHeader());
		background.enabled = true;
	}
}
