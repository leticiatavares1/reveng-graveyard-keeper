using JetBrains.Annotations;
using UnityEngine;

public class ModuleSlotArea : MonoBehaviour
{
	[SerializeField]
	[CanBeNull]
	private MeshBoundsUvToShader meshBoundsUvToShader;

	[SerializeField]
	private Texture2D iconTexture;

	[SerializeField]
	private Texture2D unavailableIconTexture;

	[SerializeField]
	private Texture2D gridTexture;

	[SerializeField]
	private Texture2D unavailableGridTexture;

	[SerializeField]
	[CanBeNull]
	private BuildArea buildArea;

	[SerializeField]
	private string fontIconId;

	[SerializeField]
	private Vector2Int cellsCount = Vector2Int.one;

	private bool IsAvailable { get; set; }

	public BuildArea BuildArea => buildArea;

	public string FontIconId => fontIconId;

	public void ApplyVisibility(bool isAvailable)
	{
		if (!(meshBoundsUvToShader == null) && !(iconTexture == null) && !(gridTexture == null) && !(unavailableIconTexture == null) && !(unavailableGridTexture == null))
		{
			IsAvailable = isAvailable;
			Texture2D texture2D = (isAvailable ? iconTexture : unavailableIconTexture);
			Texture2D texture2D2 = (isAvailable ? gridTexture : unavailableGridTexture);
			meshBoundsUvToShader.SetTextures(texture2D, texture2D2);
			meshBoundsUvToShader.SetCellsCount(cellsCount);
			meshBoundsUvToShader.Apply();
		}
	}

	private void Awake()
	{
		ApplyVisibility(IsAvailable);
	}
}
