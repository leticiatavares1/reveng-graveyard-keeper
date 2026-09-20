using UnityEngine;

[DefaultExecutionOrder(-9000)]
public class GlobalSingletonObject : MonoBehaviour
{
	private static GlobalSingletonObject _me;

	public void Awake()
	{
		if (_me == null)
		{
			_me = this;
		}
		else
		{
			NGUITools.DestroyImmediate(base.gameObject);
		}
	}
}
