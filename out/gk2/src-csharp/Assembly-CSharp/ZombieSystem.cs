public class ZombieSystem : ICustomUpdatable
{
	public void CustomUpdate(float deltaTime)
	{
		foreach (SGuid zombieOnSceneWgoId in MainGame.ZombieSystemData.zombieOnSceneWgoIds)
		{
			MainGame.ZombieSystemData.GetZombie(zombieOnSceneWgoId).CustomUpdate(deltaTime);
		}
	}
}
