using UnityEngine;

public class SimplifiedWGO : SimplifiedObject
{
	public float floor_line;

	public int fine_tune_z;

	public SerializableWGO swgo;

	private bool _restored;

	[ContextMenu("Restore")]
	public override GameObject Restore()
	{
		if (_restored)
		{
			return null;
		}
		RoundAndSortComponent roundAndSortComponent = base.gameObject.AddComponent<RoundAndSortComponent>();
		base.gameObject.AddComponent<ChunkedGameObject>();
		roundAndSortComponent.floor_line = floor_line;
		roundAndSortComponent.fine_tune_z = fine_tune_z;
		WorldGameObject worldGameObject = base.gameObject.AddComponent<WorldGameObject>();
		worldGameObject.ForceDeinitComponents();
		worldGameObject.tf = worldGameObject.transform;
		CommonRestore(base.gameObject);
		worldGameObject.RestoreFromSerializedObject(swgo, change_hierarchy: false);
		if (Application.isPlaying)
		{
			Object.Destroy(this);
		}
		else
		{
			Object.DestroyImmediate(this);
		}
		_restored = true;
		return worldGameObject.gameObject;
	}

	public void Start()
	{
	}
}
