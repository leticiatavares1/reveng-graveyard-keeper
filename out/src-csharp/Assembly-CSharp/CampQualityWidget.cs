using DLCRefugees;
using UnityEngine;

public class CampQualityWidget : CustomInventoryWidget
{
	public UILabel header_bottom_label;

	public const string YELLOW_COLOR = "eacf16";

	public const string RED_COLOR = "76b900";

	public const string GREEN_COLOR = "76b900";

	public const string ARROW_UP = "(up)";

	public const string ARROW_DOWN = "(down)";

	public UILabel full_slots_count;

	public UILabel empty_slots_count;

	public CampHappinessProgressWidget progress_widget;

	public CampHappinessProgressWidget progress_widget_right;

	public UIGrid progress_widgets_grid;

	public override void Init()
	{
		base.Init();
	}

	public override void Redraw()
	{
		base.Redraw();
		float campHappinessProgress = RefugeesCampEngine.instance.GetCampHappinessProgress();
		int totalHappiness = RefugeesCampEngine.instance.GetTotalHappiness();
		int num = Mathf.FloorToInt(RefugeesCampEngine.instance.camp_zone.GetTotalQuality()) - totalHappiness;
		float num2 = RefugeesCampEngine.instance.PredictRefugeeHappinessChange(1f);
		if (totalHappiness <= 0 && campHappinessProgress + num2 < 0f)
		{
			num2 = 0f - campHappinessProgress;
		}
		if (header_label != null)
		{
			header_label.text = inventory.name + " " + totalHappiness + "/" + (totalHappiness + num);
		}
		if (header_bottom_label != null)
		{
			string text = "[c][eacf16]" + Mathf.RoundToInt(campHappinessProgress * 100f) + "%[-][/c] ";
			bool flag = false;
			if (num2 > 0f)
			{
				text += "[c][76b900](up) ";
			}
			else if (num2 < 0f)
			{
				text += "[c][76b900](down) ";
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				text = text + Mathf.RoundToInt(num2 * 100f) + "%[-][/c]";
			}
			header_bottom_label.text = text;
		}
		if (full_slots_count != null)
		{
			full_slots_count.text = totalHappiness.ToString();
		}
		if (empty_slots_count != null)
		{
			empty_slots_count.text = num.ToString();
		}
		UpdateProgressWidget(campHappinessProgress, num2);
	}

	public void UpdateProgressWidget(float cur_pos, float prediction)
	{
		float num = cur_pos + prediction;
		bool flag = num > 1f;
		bool num2 = num < 0f;
		Debug.Log($"#CAMP# Camp quality: cur_pos={cur_pos}, prediction={prediction}, predicted_pos={num}");
		if (num2)
		{
			progress_widget.SetActive(active: true);
			float num3 = Mathf.Abs(num);
			progress_widget.green_filler.fillAmount = 0f;
			progress_widget.yellow_filler.invert = true;
			progress_widget.yellow_filler.fillAmount = 1f - num3;
			progress_widget.red_filler.invert = false;
			progress_widget.red_filler.fillAmount = num3;
			progress_widget_right.SetActive(active: true);
			progress_widget_right.yellow_filler.fillAmount = 0f;
			progress_widget_right.green_filler.fillAmount = 0f;
			progress_widget_right.red_filler.invert = true;
			progress_widget_right.red_filler.fillAmount = cur_pos;
		}
		else
		{
			if (prediction > 0f)
			{
				progress_widget.red_filler.fillAmount = 0f;
				progress_widget.green_filler.invert = true;
				progress_widget.green_filler.fillAmount = Mathf.Min(num, 1f);
				progress_widget.yellow_filler.invert = true;
				progress_widget.yellow_filler.fillAmount = cur_pos;
			}
			else
			{
				progress_widget.green_filler.fillAmount = 0f;
				progress_widget.red_filler.invert = true;
				progress_widget.red_filler.fillAmount = cur_pos;
				progress_widget.yellow_filler.invert = true;
				progress_widget.yellow_filler.fillAmount = num;
			}
			if (!flag)
			{
				progress_widget_right.SetActive(active: false);
			}
			else
			{
				progress_widget.SetActive(active: true);
				progress_widget_right.red_filler.fillAmount = 0f;
				progress_widget_right.yellow_filler.fillAmount = 0f;
				progress_widget_right.green_filler.invert = true;
				progress_widget_right.green_filler.fillAmount = Mathf.Min(num - 1f, 1f);
			}
		}
		progress_widgets_grid.repositionNow = true;
		progress_widgets_grid.Reposition();
	}

	public override GamepadNavigationItem GetFirstNavigationItem(Direction dir)
	{
		return GetComponent<GamepadNavigationItem>();
	}
}
