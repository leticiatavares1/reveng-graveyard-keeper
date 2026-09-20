using UnityEngine;

public interface IFightDecal
{
	int SortingGroup => 0;

	float GroupYOffset => 0f;

	IFightDecal SpawnDecal(Vector3 position, Direction orientation, string customDeathEffectId = "");
}
