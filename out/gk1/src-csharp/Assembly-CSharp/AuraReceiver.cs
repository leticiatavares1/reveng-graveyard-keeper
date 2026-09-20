using System;
using System.Collections.Generic;
using UnityEngine;

public class AuraReceiver : WorldGameObjectComponentBase
{
	private GameRes _auras_params = new GameRes();

	private GameRes _auras_temp_calc = new GameRes();

	private static List<AuraReceiver> _all = new List<AuraReceiver>();

	[SerializeField]
	private List<AuraReceiveSubcomponent> _aura_receivers = new List<AuraReceiveSubcomponent>();

	[SerializeField]
	private List<string> _aura_receiver_ids = new List<string>();

	[NonSerialized]
	public List<string> auras = new List<string>();

	public void Start()
	{
		_all.Add(this);
	}

	public void OnDestroy()
	{
		_all.Remove(this);
	}

	public void AddAuraReceiverSubcomponent(string aura_id)
	{
		if (!_aura_receiver_ids.Contains(aura_id))
		{
			_aura_receiver_ids.Add(aura_id);
			AuraReceiveSubcomponent item = new AuraReceiveSubcomponent(this, aura_id);
			_aura_receivers.Add(item);
		}
	}

	public static void OnAreaCalculationStarted()
	{
		foreach (AuraReceiver item in _all)
		{
			item._auras_temp_calc = new GameRes();
		}
	}

	public static void OnAreaCalculationFinished()
	{
		foreach (AuraReceiver item in _all)
		{
			item._auras_temp_calc.RemoveZeroValues();
			foreach (GameResAtom item2 in item._auras_params.ToAtomList())
			{
				if (item._auras_temp_calc.Get(item2.type).EqualsTo(0f))
				{
					item.OnAuraDisappeared(item2.type);
				}
			}
			foreach (GameResAtom item3 in item._auras_temp_calc.ToAtomList())
			{
				if (item._auras_params.Get(item3.type).EqualsTo(0f))
				{
					item.OnAuraAppeared(item3.type);
				}
			}
			item._auras_params = item._auras_temp_calc.Clone();
		}
	}

	private void ProcessAura(AuraEmitSubcomponent aura, float distance)
	{
		distance = Mathf.Abs(distance) / 96f;
		float value = Mathf.Min((float)((distance <= aura.aura.radius) ? 1 : 0) + _auras_temp_calc.Get(aura.aura_id), 1f);
		_auras_temp_calc.Set(aura.aura_id, value);
	}

	public static IEnumerable<int> ProcessAuraEmitCalculation(AuraEmitSubcomponent aura, Vector2 emitter_pos)
	{
		foreach (AuraReceiver item in _all)
		{
			float magnitude = ((Vector2)item.tf.position - emitter_pos).magnitude;
			item.ProcessAura(aura, magnitude);
		}
		yield break;
	}

	protected void OnAuraAppeared(string aura_id)
	{
		Debug.Log("AuraReceiver.OnAuraAppeared " + aura_id, base.wgo);
		auras.Add(aura_id);
		foreach (AuraReceiveSubcomponent aura_receiver in _aura_receivers)
		{
			if (aura_receiver.aura_id == aura_id)
			{
				aura_receiver.OnAuraAppeared();
			}
		}
	}

	protected void OnAuraDisappeared(string aura_id)
	{
		Debug.Log("AuraReceiver.OnAuraRemoved " + aura_id, base.wgo);
		auras.Remove(aura_id);
		foreach (AuraReceiveSubcomponent aura_receiver in _aura_receivers)
		{
			if (aura_receiver.aura_id == aura_id)
			{
				aura_receiver.OnAuraDisappeared();
			}
		}
	}

	public void Clear()
	{
		_aura_receivers.Clear();
		_aura_receiver_ids.Clear();
	}

	public void Update()
	{
		foreach (AuraReceiveSubcomponent aura_receiver in _aura_receivers)
		{
			aura_receiver.Update();
		}
	}

	public float GetAuraParam(string aura_id)
	{
		return _auras_params.Get(aura_id);
	}
}
