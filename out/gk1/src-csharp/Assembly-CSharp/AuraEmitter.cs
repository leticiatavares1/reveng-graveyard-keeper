using System.Collections.Generic;
using UnityEngine;

public class AuraEmitter : WorldGameObjectComponentBase
{
	[SerializeField]
	private List<AuraEmitSubcomponent> _auras = new List<AuraEmitSubcomponent>();

	[SerializeField]
	private List<string> _aura_ids = new List<string>();

	private static List<AuraEmitter> _all = new List<AuraEmitter>();

	public void Start()
	{
		_all.Add(this);
	}

	public void OnDestroy()
	{
		_all.Remove(this);
	}

	protected IEnumerable<int> DoEmitCalculations()
	{
		Vector2 pos = base.tf.position;
		foreach (AuraEmitSubcomponent aura in _auras)
		{
			foreach (int item in AuraReceiver.ProcessAuraEmitCalculation(aura, pos))
			{
				_ = item;
			}
			yield return 0;
		}
	}

	public void AddAura(string aura_id)
	{
		if (!_aura_ids.Contains(aura_id))
		{
			_aura_ids.Add(aura_id);
			_auras.Add(new AuraEmitSubcomponent
			{
				aura_id = aura_id
			});
		}
	}

	public static void ProcessAurasCalculation()
	{
		AuraReceiver.OnAreaCalculationStarted();
		foreach (AuraEmitter item in _all)
		{
			foreach (int item2 in item.DoEmitCalculations())
			{
				_ = item2;
			}
		}
		AuraReceiver.OnAreaCalculationFinished();
	}

	public void Clear()
	{
		_auras.Clear();
		_aura_ids.Clear();
	}
}
