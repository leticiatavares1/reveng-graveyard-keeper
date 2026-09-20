using System;

[Serializable]
public class MovementSystem : ICustomUpdatable
{
	private static MovementSystemData MovementSystemData => MainGame.Instance.GameSave.movementSystemData;

	public void CustomUpdate(float deltaTime)
	{
		for (int i = 0; i < MovementSystemData.movingObjects.Count; i++)
		{
			MovementSystemData.movingObjects[i].Update(deltaTime);
		}
	}

	public void AddMovingObject(MovementComponent movingObject)
	{
		if (!MovementSystemData.movingObjects.Contains(movingObject))
		{
			MovementSystemData.movingObjects.Add(movingObject);
		}
	}

	public void RemoveMovingObject(MovementComponent movingObject)
	{
		MovementSystemData.movingObjects.Remove(movingObject);
	}
}
