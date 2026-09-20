using UnityEngine;

[CreateAssetMenu(menuName = "Player Customization", fileName = "CustomizablePart")]
public class CustomizablePart : ScriptableObject
{
	public int orderIndex;

	public CustomizablePartType type;

	public SkinPresetPartGK2 skinPart;
}
