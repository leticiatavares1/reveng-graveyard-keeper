using UnityEngine;

public abstract class SmartCondition : MonoBehaviour
{
	public abstract bool CheckCondition();

	public virtual string GetName()
	{
		return GetType().Name.Replace("SmartCondition", "").Trim('_');
	}
}
