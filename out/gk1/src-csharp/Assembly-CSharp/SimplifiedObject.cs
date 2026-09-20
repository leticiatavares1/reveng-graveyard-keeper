using System;
using UnityEngine;

public abstract class SimplifiedObject : MonoBehaviour
{
	[SerializeField]
	protected int sg_grid_divider;

	[SerializeField]
	protected int sg_fine_tune_z;

	[SerializeField]
	protected int go_layer;

	[SerializeField]
	protected int spr_sorting_layer_id;

	[SerializeField]
	protected int spr_sorting_order;

	[NonSerialized]
	public ChunkedGameObject chunk;

	public virtual GameObject Restore()
	{
		return null;
	}

	protected void CommonRestore(GameObject o)
	{
		o.layer = go_layer;
		o.name = base.name;
		SnapToGridComponent component = o.GetComponent<SnapToGridComponent>();
		if (component != null)
		{
			component.grid_divider = sg_grid_divider;
			component.fine_tune_z = sg_fine_tune_z;
		}
		SpriteRenderer component2 = o.GetComponent<SpriteRenderer>();
		if (component2 != null)
		{
			component2.sortingLayerID = spr_sorting_layer_id;
			component2.sortingOrder = spr_sorting_order;
		}
		chunk = o.GetComponent<ChunkedGameObject>();
		if (chunk != null)
		{
			chunk.Start();
		}
	}

	public static void RestoreAll(Transform root)
	{
		SimplifiedObject[] componentsInChildren = root.GetComponentsInChildren<SimplifiedObject>(includeInactive: true);
		Debug.Log("Restoring objects on scene: " + componentsInChildren.Length);
		SimplifiedObject[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Restore();
		}
	}
}
