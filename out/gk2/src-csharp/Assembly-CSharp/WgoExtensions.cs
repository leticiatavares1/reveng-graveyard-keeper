using UnityEngine;

public static class WgoExtensions
{
	private const float COLLIDER_BOX_SEARCH_HALF_SIZE = 10f;

	private static readonly Collider[] overlapColliders = new Collider[30];

	public static bool TryGetNearestBuilderWorldZone(this Wgo builderWgo, out WorldZone worldZone)
	{
		worldZone = null;
		if (!builderWgo)
		{
			return false;
		}
		int num = Physics.OverlapBoxNonAlloc(builderWgo.transform.position, new Vector3(10f, 10f, 10f), overlapColliders, Quaternion.identity, 131072);
		if (num == 0)
		{
			return false;
		}
		float num2 = float.PositiveInfinity;
		WorldZone worldZone2 = null;
		Vector3 position = builderWgo.transform.position;
		for (int i = 0; i < num; i++)
		{
			Collider collider = overlapColliders[i];
			if ((bool)collider && collider.TryGetComponent<WorldZone>(out var component) && IsBuilderForWorldZone(builderWgo, component))
			{
				if (collider.bounds.Contains(position))
				{
					worldZone = component;
					return true;
				}
				float num3 = collider.bounds.SqrDistance(position);
				if (num2 > num3)
				{
					num2 = num3;
					worldZone2 = component;
				}
			}
		}
		if (!worldZone2)
		{
			return false;
		}
		worldZone = worldZone2;
		return true;
	}

	private static bool IsBuilderForWorldZone(Wgo builderWgo, WorldZone worldZone)
	{
		if (!builderWgo || worldZone == null)
		{
			return false;
		}
		WorldZoneDef dataOrNull = GameBalance.Me.GetDataOrNull<WorldZoneDef>(worldZone.Id);
		if (dataOrNull != null && !string.IsNullOrEmpty(dataOrNull.builderId))
		{
			return dataOrNull.builderId == builderWgo.Id;
		}
		return false;
	}
}
