using System;
using System.Collections.Generic;

[Serializable]
public class AnswerData
{
	public SmartRes d_lock;

	public SmartRes d_price;

	public SmartRes d_reward;

	public List<AnswerData> datas;

	public virtual void FillVisualData(ref AnswerVisualData vis_data, WorldGameObject linked_wgo = null)
	{
		if (d_price != null)
		{
			d_price.FillVisualData(out vis_data.icon_price, out vis_data.n_price, out vis_data.icon_price_quality, linked_wgo);
		}
		if (d_lock != null)
		{
			d_lock.FillVisualData(out vis_data.icon_lock, out vis_data.n_lock, out vis_data.icon_lock_quality, linked_wgo);
		}
		if (d_reward != null)
		{
			d_reward.FillVisualData(out vis_data.icon_reward, out vis_data.n_reward, out vis_data.icon_reward_quality, linked_wgo);
		}
		_ = d_price;
		if (!MainGame.me.player.IsEnough(d_price) || !MainGame.me.player.IsEnough(d_lock))
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

	public virtual bool HasAnyReward()
	{
		if (d_reward == null)
		{
			return false;
		}
		if (d_reward.res_type == SmartRes.ResType.Empty)
		{
			return false;
		}
		return true;
	}
}
