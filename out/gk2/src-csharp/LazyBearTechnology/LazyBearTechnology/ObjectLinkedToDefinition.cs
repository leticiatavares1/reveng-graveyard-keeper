using System;

namespace LazyBearTechnology;

[Serializable]
public abstract class ObjectLinkedToDefinition<T> where T : BalanceBaseObject
{
	public string id;

	private T definition;

	private string cachedId = "";

	public T Definition
	{
		get
		{
			if (cachedId != id)
			{
				definition = GameBalanceBase.Instance.GetData<T>(id);
				cachedId = id;
			}
			return definition;
		}
	}

	public ObjectLinkedToDefinition()
	{
	}

	public ObjectLinkedToDefinition(string id)
	{
		this.id = id;
	}

	public override string ToString()
	{
		return base.ToString() + " (" + id + ")";
	}
}
