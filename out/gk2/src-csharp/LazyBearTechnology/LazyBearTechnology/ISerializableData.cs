namespace LazyBearTechnology;

public interface ISerializableData
{
	void OnBeforeSerialize();

	void OnAfterSerialize();
}
