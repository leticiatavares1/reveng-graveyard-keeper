using LazyBearTechnology;

public class UIAlchemyWindowData : LazyWidgetDataBase
{
	public Wgo Wgo { get; private set; }

	public UIAlchemyWindowData(Wgo wgo)
	{
		Wgo = wgo;
		Wgo.Data.TrySetWorker(MainGame.PlayerController);
	}
}
