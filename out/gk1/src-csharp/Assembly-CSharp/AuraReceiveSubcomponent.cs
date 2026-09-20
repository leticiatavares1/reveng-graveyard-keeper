using System;
using UnityEngine;

[Serializable]
public class AuraReceiveSubcomponent : AuraSubcomponentBase
{
	protected AuraReceiver aura_receiver;

	protected AuraDefinition aura_definition;

	public EventDelegate on_aura_appeared;

	public EventDelegate on_aura_disappeared;

	private float _aura_time;

	private float _last_update_time;

	public AuraReceiveSubcomponent(AuraReceiver aura_receiver, string aura_id)
	{
		this.aura_receiver = aura_receiver;
		base.aura_id = aura_id;
		aura_definition = GameBalance.me.GetData<AuraDefinition>(aura_id);
	}

	public virtual void OnAuraAppeared()
	{
		if (on_aura_appeared != null)
		{
			on_aura_appeared.Execute();
		}
		MainGame.me.gui_elements.buffs.Redraw();
		_aura_time = 0f;
		_last_update_time = Time.time;
		Debug.Log("OnAuraAppeared " + aura_id);
	}

	public virtual void OnAuraDisappeared()
	{
		if (on_aura_disappeared != null)
		{
			on_aura_disappeared.Execute();
		}
		MainGame.me.gui_elements.buffs.Redraw();
		Debug.Log("OnAuraDisappeared " + aura_id);
	}

	public override void Update()
	{
		base.Update();
		_aura_time += Time.time - _last_update_time;
		_last_update_time = Time.time;
		if (base.aura != null && _aura_time > base.aura.time_tick)
		{
			_aura_time -= base.aura.time_tick;
			DoAuraTick();
		}
	}

	protected void DoAuraTick()
	{
		if (aura_receiver == null)
		{
			Debug.LogError("AuraReceiveSubcomponent has no receiver");
			return;
		}
		float auraParam = aura_receiver.GetAuraParam(aura_id);
		if (auraParam.Equals(0f))
		{
			return;
		}
		WorldGameObject worldGameObject = null;
		Debug.LogException(new Exception("Auras not implemented"));
		if (worldGameObject == null)
		{
			Debug.LogError("AuraReceiveSubcomponent receiver has no WorldGameObject");
			return;
		}
		float hp = worldGameObject.hp;
		worldGameObject.AddToParams(base.aura.res * auraParam);
		float num = worldGameObject.hp - hp;
		if (!num.EqualsTo(0f))
		{
			EffectBubblesManager.ShowStackedHP(worldGameObject, num);
		}
	}
}
