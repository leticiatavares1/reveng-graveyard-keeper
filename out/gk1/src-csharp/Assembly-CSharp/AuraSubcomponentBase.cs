using System;

public abstract class AuraSubcomponentBase
{
	public string aura_id;

	[NonSerialized]
	private AuraDefinition _aura;

	public AuraDefinition aura => _aura ?? (_aura = GameBalance.me.GetData<AuraDefinition>(aura_id));

	public virtual void Update()
	{
	}
}
