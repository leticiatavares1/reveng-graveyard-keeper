using UnityEngine;

public class TriggerColliderComponentLinker : MonoBehaviour
{
	[SerializeField]
	private MonoBehaviour component;

	public MonoBehaviour Component
	{
		get
		{
			return component;
		}
		set
		{
			component = value;
		}
	}
}
