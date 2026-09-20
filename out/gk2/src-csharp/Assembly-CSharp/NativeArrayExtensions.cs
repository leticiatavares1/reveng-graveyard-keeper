using Unity.Collections;

public static class NativeArrayExtensions
{
	public static NativeArray<T> CombineNativeArrays<T>(NativeArray<T>[] arrays, Allocator allocator) where T : struct
	{
		int num = 0;
		NativeArray<T>[] array = arrays;
		foreach (NativeArray<T> nativeArray in array)
		{
			num += nativeArray.Length;
		}
		NativeArray<T> nativeArray2 = new NativeArray<T>(num, allocator);
		int num2 = 0;
		array = arrays;
		for (int i = 0; i < array.Length; i++)
		{
			NativeArray<T> src = array[i];
			NativeArray<T>.Copy(src, 0, nativeArray2, num2, src.Length);
			num2 += src.Length;
		}
		return nativeArray2;
	}
}
