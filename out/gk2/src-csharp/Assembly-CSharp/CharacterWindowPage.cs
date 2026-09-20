using LazyBearTechnology;

public abstract class CharacterWindowPage : LazyWidget<CharacterWindowPageData>
{
	protected override void TestDraw()
	{
	}

	protected T GetData<T>() where T : CharacterWindowPageData
	{
		return data as T;
	}
}
