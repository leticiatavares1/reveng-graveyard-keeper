using System.Collections.Generic;
using UnityEngine;

public class SimplifiedNonWGO : SimplifiedObject
{
	public Object prefab;

	private static Dictionary<string, bool> _prefab_has_chunck = new Dictionary<string, bool>();

	[ContextMenu("Restore")]
	public override GameObject Restore()
	{
		GameObject gameObject = (GameObject)prefab;
		string key = gameObject.name;
		if (gameObject.activeSelf)
		{
			if (!_prefab_has_chunck.TryGetValue(key, out var value))
			{
				value = gameObject.GetComponent<ChunkedGameObject>() != null;
				_prefab_has_chunck.Add(key, value);
			}
			if (value)
			{
				gameObject.SetActive(value: false);
			}
		}
		GameObject gameObject2 = Object.Instantiate(gameObject, base.transform.parent);
		gameObject2.transform.localPosition = base.transform.localPosition;
		gameObject2.transform.localScale = base.transform.localScale;
		gameObject2.transform.localRotation = base.transform.localRotation;
		CommonRestore(gameObject2);
		if (Application.isPlaying && chunk != null)
		{
			chunk.ResetAtTheBeginning();
			chunk.Init();
		}
		if (Application.isPlaying)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
		_ = Application.isPlaying;
		return gameObject2.gameObject;
	}

	public void Start()
	{
	}
}
