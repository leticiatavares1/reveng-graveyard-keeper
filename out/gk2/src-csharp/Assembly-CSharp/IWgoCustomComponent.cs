public interface IWgoCustomComponent<T>
{
	T OnSave();

	void OnLoad(T data);

	void OnUnload();
}
