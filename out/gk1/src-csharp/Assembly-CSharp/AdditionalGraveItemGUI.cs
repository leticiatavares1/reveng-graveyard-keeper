using UnityEngine;

public class AdditionalGraveItemGUI : MonoBehaviour
{
	public UILabel skulls_counter;

	private bool _initialized;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
		}
	}

	public void Clear()
	{
		if (!_initialized)
		{
			Init();
		}
		skulls_counter.text = "-";
	}

	public void Draw(Item item)
	{
	}
}
