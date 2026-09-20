using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WsoRepairableStage : MonoBehaviour
{
	[SerializeField]
	private List<ConstructorPart> constructorParts = new List<ConstructorPart>();

	public IReadOnlyList<ConstructorPart> ConstructorParts => constructorParts;

	public void CollectParts()
	{
		constructorParts.ForEach(delegate(ConstructorPart part)
		{
			if ((bool)part)
			{
				part.constructorPartChildData.canNotBeBaked = false;
			}
		});
		constructorParts.Clear();
		GetComponentsInChildren(includeInactive: true, constructorParts);
		constructorParts.ForEach(delegate(ConstructorPart part)
		{
			part.constructorPartChildData.canNotBeBaked = true;
		});
	}

	public WsoStageData CreateStageData(int stageIndex)
	{
		CollectParts();
		WsoStageData wsoStageData = new WsoStageData(stageIndex);
		foreach (ConstructorPart constructorPart in constructorParts)
		{
			if ((bool)constructorPart)
			{
				ConstructorPartStateData constructorPartStateData = CreatePartStateData(constructorPart);
				if (constructorPartStateData != null)
				{
					wsoStageData.AddPartData(constructorPartStateData);
				}
			}
		}
		return wsoStageData;
	}

	private ConstructorPartStateData CreatePartStateData(ConstructorPart part)
	{
		string text = null;
		string text2 = null;
		string text3 = part.constructorPartChildData?.pathToObject;
		if (!string.IsNullOrEmpty(text3))
		{
			int num = text3.LastIndexOf('/');
			int num2 = text3.LastIndexOf('.');
			if (num >= 0 && num2 > num)
			{
				text = text3.Substring(num + 1, num2 - num - 1);
			}
		}
		text2 = ((part.constructorPartChildData?.lut != null) ? part.constructorPartChildData.lut.name : null);
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogWarning("[WsoRepairableStage] ConstructorPart '" + part.name + "' has no model ID, skipping");
			return null;
		}
		Vector3 localPosition = base.transform.InverseTransformPoint(part.transform.position);
		Vector3 lossyScale = part.transform.lossyScale;
		Vector3 lossyScale2 = base.transform.lossyScale;
		Vector3 vector = part.transform.localScale;
		if (lossyScale2.x != 0f && lossyScale2.y != 0f && lossyScale2.z != 0f)
		{
			vector = new Vector3(lossyScale.x / lossyScale2.x, lossyScale.y / lossyScale2.y, lossyScale.z / lossyScale2.z);
		}
		return new ConstructorPartStateData(text, localPosition, vector.x, text2);
	}
}
