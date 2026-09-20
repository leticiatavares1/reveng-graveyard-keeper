public class MultipleAnswerVisualData : AnswerVisualData
{
	public override bool IsDetailed()
	{
		bool result = false;
		for (int i = 0; i < answer_visual_datas.Count; i++)
		{
			if (!string.IsNullOrEmpty(answer_visual_datas[i].icon_price) || !string.IsNullOrEmpty(answer_visual_datas[i].icon_lock) || !string.IsNullOrEmpty(answer_visual_datas[i].icon_reward))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
