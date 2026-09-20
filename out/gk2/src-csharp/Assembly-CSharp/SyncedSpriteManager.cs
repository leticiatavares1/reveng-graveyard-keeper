using LazyBearTechnology;

public class SyncedSpriteManager : LazySingleton<SyncedSpriteManager>
{
	private void Update()
	{
		SyncedSprite.UpdateAllSyncedSprites();
	}
}
