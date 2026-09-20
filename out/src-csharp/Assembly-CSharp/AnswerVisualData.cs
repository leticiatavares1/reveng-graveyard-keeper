using System.Collections.Generic;

public class AnswerVisualData
{
	public string id = "";

	public string icon_price;

	public string icon_lock;

	public string icon_reward;

	public string translation;

	public bool can_be_picked = true;

	public bool inside_price_is_red;

	public string price_txt = "";

	public string icon_price_quality;

	public string icon_reward_quality;

	public string icon_lock_quality;

	public int n_price;

	public int n_reward;

	public int n_lock;

	public AnswerData link_to_answer_data;

	public List<AnswerVisualData> answer_visual_datas = new List<AnswerVisualData>();

	public virtual bool IsDetailed()
	{
		if (string.IsNullOrEmpty(icon_price) && string.IsNullOrEmpty(icon_lock))
		{
			return !string.IsNullOrEmpty(icon_reward);
		}
		return true;
	}
}
