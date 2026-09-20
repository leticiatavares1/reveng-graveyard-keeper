using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;

namespace LazyBearTechnology;

public abstract class LazySingletonSerializedSO<T> : SerializedScriptableObject where T : SerializedScriptableObject
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Addressables.LoadAssetAsync<T>(typeof(T).Name).WaitForCompletion();
			}
			return instance;
		}
	}

	public static void SetReference(T reference)
	{
		instance = reference;
	}
}
