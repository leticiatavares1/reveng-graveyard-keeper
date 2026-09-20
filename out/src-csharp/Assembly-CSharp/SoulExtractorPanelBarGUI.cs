using UnityEngine;

public class SoulExtractorPanelBarGUI : MonoBehaviour
{
	public const string MIN_CORRUPTION_ID = "min_corruption_chance";

	public const string MAX_CORRUPTION_ID = "max_corruption_chance";

	public const string CORRUPTION_CHANCE = "corruption_chance";

	private const int HORIZONTAL_BAR_SIZE = 108;

	[Range(0f, 1f)]
	public float durability;

	[SerializeField]
	private float min_corruption_chance;

	[SerializeField]
	private float max_corruption_chance;

	[SerializeField]
	private float corruption_chance;

	[SerializeField]
	private UIProgressBar progressBar;

	[SerializeField]
	private UIProgressBar min_corruption_widget;

	[SerializeField]
	private UIProgressBar range_corruption_widget;

	[SerializeField]
	private GameObject range_corruption_widget_thumb;

	[SerializeField]
	private UILabel corruption_chance_label;

	[SerializeField]
	private GameObject hints_container;

	public void SetData(float durability, float min_corruption_chance = 0f, float max_corruption_chance = 0f, float corruption_chance = 0f)
	{
		this.durability = durability;
		this.min_corruption_chance = min_corruption_chance;
		this.max_corruption_chance = max_corruption_chance;
		this.corruption_chance = corruption_chance;
	}

	public void Redraw()
	{
		Mathf.CeilToInt(durability * 100f);
		_ = 90;
		range_corruption_widget_thumb.SetActive(value: true);
		if (min_corruption_chance.EqualsTo(0f))
		{
			min_corruption_widget.SetActive(active: false);
		}
		else
		{
			if (durability <= min_corruption_chance)
			{
				min_corruption_chance = durability;
			}
			min_corruption_widget.value = min_corruption_chance;
			min_corruption_widget.ForceUpdate();
			min_corruption_widget.SetActive(active: true);
		}
		if (max_corruption_chance.EqualsTo(0f))
		{
			range_corruption_widget.gameObject.SetActive(value: false);
		}
		else
		{
			if (durability <= max_corruption_chance)
			{
				max_corruption_chance = durability;
				range_corruption_widget_thumb.SetActive(value: false);
			}
			range_corruption_widget.value = max_corruption_chance;
			range_corruption_widget.ForceUpdate();
			corruption_chance_label.text = $"{Mathf.CeilToInt(corruption_chance * 100f)}%";
			range_corruption_widget.gameObject.SetActive(value: true);
		}
		hints_container.SetActive(value: false);
		progressBar.value = durability;
		progressBar.ForceUpdate();
	}
}
