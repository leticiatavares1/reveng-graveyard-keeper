using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UISimpleTextNotification : UIBaseNotification
{
	[SerializeField]
	private TextMeshProUGUI label;

	public string Text { get; set; }

	public string LocalizationKey { get; set; }

	public override void Draw()
	{
		label.text = Text;
	}

	public override void ReleaseToPool()
	{
		LazyPooler.ReleaseObject(this);
	}
}
