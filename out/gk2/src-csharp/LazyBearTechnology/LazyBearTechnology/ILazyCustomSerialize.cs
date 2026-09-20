namespace LazyBearTechnology;

public interface ILazyCustomSerialize
{
	void OnLazyPreSerialize();

	void OnLazyPostDeserialize();
}
