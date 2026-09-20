using System;
using System.Collections.Generic;

[Serializable]
public class MultipleAnswerData : AnswerData
{
	public MultipleAnswerData()
	{
	}

	public MultipleAnswerData(SmartRes reward, List<AnswerData> datas)
	{
		d_reward = reward;
		base.datas = datas;
	}

	public void FillVisualData(ref MultipleAnswerVisualData vis_data, WorldGameObject linked_wgo = null)
	{
		bool flag = true;
		bool flag2 = true;
		if (datas != null)
		{
			for (int i = 0; i < datas.Count; i++)
			{
				AnswerVisualData answerVisualData = new AnswerVisualData();
				vis_data.answer_visual_datas.Add(answerVisualData);
				datas[i].d_lock.FillVisualData(out answerVisualData.icon_lock, out answerVisualData.n_lock, out answerVisualData.icon_lock_quality, linked_wgo);
				datas[i].d_price.FillVisualData(out answerVisualData.icon_price, out answerVisualData.n_price, out answerVisualData.icon_price_quality, linked_wgo);
				answerVisualData.link_to_answer_data = datas[i];
				if (!MainGame.me.player.IsEnough(datas[i].d_lock))
				{
					flag2 = false;
				}
				else if (!MainGame.me.player.IsEnough(datas[i].d_price))
				{
					flag = false;
				}
			}
		}
		if (d_reward != null)
		{
			d_reward.FillVisualData(out vis_data.icon_reward, out vis_data.n_reward, out vis_data.icon_reward_quality, linked_wgo);
		}
		if (!flag2 || !flag)
		{
			vis_data.inside_price_is_red = true;
			vis_data.can_be_picked = false;
			if (d_price != null && d_price.res_type == SmartRes.ResType.Item)
			{
				vis_data.price_txt = MainGame.me.player.data.GetTotalCount(d_price.item.id) + "/" + vis_data.n_price;
			}
		}
		vis_data.link_to_answer_data = this;
	}
}
