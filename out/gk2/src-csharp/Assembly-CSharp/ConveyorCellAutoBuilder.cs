using UnityEngine;

public class ConveyorCellAutoBuilder : MonoBehaviour
{
	[SerializeField]
	private int rotationIndex;

	public Wgo AutoBuildCell()
	{
		ConveyorWgoData conveyorWgoData = new ConveyorWgoData(ConveyorElementType.Cell, "conveyor_cell", base.transform.position, MainGame.PlayerController.CurrentGameScene.Id);
		conveyorWgoData.MainWgoPartData.variationId = "start";
		conveyorWgoData.MainWgoPartData.rotationIndex = rotationIndex;
		Wgo wgo = MainGame.PlayerController.CurrentGameScene.AddWgoData(conveyorWgoData);
		if (wgo == null)
		{
			Debug.LogError("Failed to build conveyor cell");
			return null;
		}
		return wgo;
	}
}
