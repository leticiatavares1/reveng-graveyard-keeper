using UnityEngine;

public class FlowGridCell : MonoBehaviour
{
	public enum CellType
	{
		None,
		UnderObject,
		TotemArea
	}

	private CellType _cell_type;

	public const float COLLIDER_RADIUS = 0.9f;

	public CellType cell_type => _cell_type;

	public bool IsPlaceAvailable(string build_zone_id)
	{
		Vector3 position = base.transform.position;
		Collider2D[] array = Physics2D.OverlapBoxAll(position, Vector2.one * 0.9f * 96f, 0f, 1);
		bool flag = array.Length == 0;
		if (string.IsNullOrEmpty(build_zone_id))
		{
			return flag;
		}
		if (!flag)
		{
			return false;
		}
		array = Physics2D.OverlapBoxAll(position, Vector2.one * 0.9f * 96f, 0f, 524288);
		if (array.Length == 0)
		{
			return false;
		}
		Collider2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			WorldZone component = array2[i].GetComponent<WorldZone>();
			if (component != null && component.id == build_zone_id)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInsideWorldZone(string zone_id, string sub_zone_id)
	{
		Collider2D[] array = Physics2D.OverlapBoxAll(base.transform.position, BuildGrid.GRID_CHECK_BOX_SIZE, 0f, 524288);
		foreach (Collider2D collider2D in array)
		{
			if (string.IsNullOrEmpty(sub_zone_id))
			{
				WorldZone component = collider2D.GetComponent<WorldZone>();
				if (!(component == null) && component.id == zone_id)
				{
					return true;
				}
			}
			else
			{
				WorldSubZone component2 = collider2D.GetComponent<WorldSubZone>();
				if (component2 != null && component2.sub_zone_id == sub_zone_id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static FlowGridCell Create(Transform parent_tr, Vector2 pos, int grid_scale_divider, CellType cell_type)
	{
		GameObject obj = Object.Instantiate(MainGame.me.build_grid_cell);
		obj.transform.SetParent(parent_tr, worldPositionStays: false);
		obj.transform.localPosition = new Vector3(pos.x, pos.y, 1f);
		obj.transform.localScale = Vector3.one / grid_scale_divider;
		FlowGridCell component = obj.GetComponent<FlowGridCell>();
		component._cell_type = cell_type;
		component.SetRedColorState(is_red_color: false);
		if (BuildGrid.current_sub_zone_configuration != null)
		{
			SpriteRenderer component2 = component.GetComponent<SpriteRenderer>();
			component2.sortingLayerName = BuildGrid.current_sub_zone_configuration.build_cell_sorting_layer;
			if (!BuildGrid.current_sub_zone_configuration.is_cell_ground_object)
			{
				Object.Destroy(component.GetComponent<GroundObject>());
			}
			if (BuildGrid.current_sub_zone_configuration.override_grid_cell_sorting_order)
			{
				component2.sortingOrder = BuildGrid.current_sub_zone_configuration.grid_cell_sorting_order;
			}
		}
		return component;
	}

	public void SetRedColorState(bool is_red_color)
	{
		SpriteRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = ((_cell_type == CellType.TotemArea) ? Color.blue : (is_red_color ? Color.red : Color.white));
		}
	}
}
