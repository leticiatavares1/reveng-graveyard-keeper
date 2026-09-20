using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICraftStatusInfoWidget : LazyWidget<UICraftStatusInfoWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI label;

	public override void Redraw()
	{
		base.Redraw();
		string iconName = data.StatusIcon.name.Replace("(Clone)", "");
		label.text = iconName.FontIcon() + ": " + data.Text;
		label.alignment = data.TextAlignmentOptions;
		if (data.TextStyle != null)
		{
			data.TextStyle.ApplyStyle(label);
		}
	}

	protected override void TestDraw()
	{
	}
}
