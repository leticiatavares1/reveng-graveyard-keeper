using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "WgoPartBakedDataAsset")]
public class WgoPartBakedDataAsset : SerializedScriptableObject
{
	[OdinSerialize]
	private WgoPartBakedData bakedData;

	public WgoPartBakedData Data => bakedData;

	public string Id => bakedData?.id;

	public void SetData(WgoPartBakedData data)
	{
		bakedData = data;
	}
}
