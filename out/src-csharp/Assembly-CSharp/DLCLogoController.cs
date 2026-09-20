using System.Collections.Generic;
using UnityEngine;

public class DLCLogoController : MonoBehaviour
{
	[SerializeField]
	private UIWidget _ui_widget;

	[SerializeField]
	private UITable _ui_table;

	[SerializeField]
	private List<DLCLogoElement> dlc_logos = new List<DLCLogoElement>();

	public void Show()
	{
		int num = DLCEngine.DLCAvailableCount();
		int num2 = 0;
		DLCLogoElement dLCLogoElement = null;
		for (int i = 0; i < dlc_logos.Count; i++)
		{
			if (DLCEngine.IsDLCAvailable(dlc_logos[i].dlc_version))
			{
				num2++;
				dlc_logos[i].Show(num2 != num);
				if (num2 == num)
				{
					dLCLogoElement = dlc_logos[i];
				}
			}
			else
			{
				dlc_logos[i].Hide();
			}
		}
		_ui_table.repositionNow = true;
		_ui_table.Reposition();
		if (dLCLogoElement != null)
		{
			float y = dLCLogoElement.gameObject.transform.localPosition.y;
			_ui_widget.height = Mathf.RoundToInt(Mathf.Abs(y));
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
