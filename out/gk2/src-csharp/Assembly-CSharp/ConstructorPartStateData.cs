using System;
using UnityEngine;

[Serializable]
public class ConstructorPartStateData
{
	[Tooltip("Original (broken) model name — stable key for repair mapping, reset, and path lookup")]
	[SerializeField]
	private string originalModelId;

	[Tooltip("Local position relative to the stage/Wso")]
	[SerializeField]
	private Vector3 localPosition;

	[Tooltip("Rotation")]
	[SerializeField]
	private Quaternion localRotation = Quaternion.identity;

	[Tooltip("Scale of current constructor part)")]
	[SerializeField]
	private float localXScale;

	[Tooltip("LUT name (loaded via config path + name at runtime)")]
	[SerializeField]
	private string lutName;

	[Tooltip("Whether this part has been repaired")]
	[SerializeField]
	private bool isRepaired;

	[Tooltip("Whether this part should be deleted (not replaced)")]
	[SerializeField]
	private bool isDeleted;

	public string OriginalModelId => originalModelId;

	public Vector3 LocalPosition => localPosition;

	public float LocalXScale => localXScale;

	public string LutName => lutName;

	public bool IsRepaired => isRepaired;

	public bool IsDeleted => isDeleted;

	public ConstructorPartStateData()
	{
	}

	public ConstructorPartStateData(string originalModelId, Vector3 localPosition, float localXScale, string lutName = null)
	{
		this.originalModelId = originalModelId;
		this.localPosition = localPosition;
		this.localXScale = localXScale;
		this.lutName = lutName;
		isRepaired = false;
	}

	public string GetCurrentModelId(ConstructorPartReplacementConfig config)
	{
		if (isDeleted || string.IsNullOrEmpty(originalModelId))
		{
			return null;
		}
		if (isRepaired && config != null && config.TryGetRepairedModel(originalModelId, out var repairedModelId) && !string.IsNullOrEmpty(repairedModelId))
		{
			return repairedModelId;
		}
		return originalModelId;
	}

	public string GetCurrentAssetPath(ConstructorPartReplacementConfig config)
	{
		if (isDeleted || string.IsNullOrEmpty(originalModelId) || config == null)
		{
			return null;
		}
		if (isRepaired && config.TryGetRepairedModelWithPath(originalModelId, out var _, out var repairedAssetPath) && !string.IsNullOrEmpty(repairedAssetPath))
		{
			return repairedAssetPath;
		}
		if (!config.TryGetBrokenAssetPath(originalModelId, out var brokenAssetPath))
		{
			return null;
		}
		return brokenAssetPath;
	}

	public void SetRepaired()
	{
		isRepaired = true;
		isDeleted = false;
	}

	public void SetDeleted()
	{
		isDeleted = true;
		isRepaired = false;
	}

	public void Reset()
	{
		isRepaired = false;
		isDeleted = false;
	}
}
