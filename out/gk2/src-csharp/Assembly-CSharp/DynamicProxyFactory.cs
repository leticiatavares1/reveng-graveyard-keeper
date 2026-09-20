using Castle.DynamicProxy;

public static class DynamicProxyFactory
{
	private static readonly ProxyGenerator proxyGenerator;

	private static readonly IInterceptor[] defaultInterceptors;

	static DynamicProxyFactory()
	{
		proxyGenerator = new ProxyGenerator();
		IInterceptor[] array = new NetworkInterceptor[1]
		{
			new NetworkInterceptor()
		};
		defaultInterceptors = array;
	}

	public static T CreateClass<T>(T targetClassCreateFrom, params IInterceptor[] interceptors) where T : class
	{
		return proxyGenerator.CreateClassProxyWithTarget(targetClassCreateFrom, (interceptors.Length != 0) ? interceptors : defaultInterceptors);
	}

	public static TProxy CreateClassTargeted<TClass, TProxy>(TClass targetClassCreateFrom, params IInterceptor[] interceptors) where TClass : class where TProxy : class, IProxyClassTargeted<TClass>, TClass
	{
		TProxy val = CreateClass(targetClassCreateFrom as TProxy, interceptors);
		val.Target = targetClassCreateFrom;
		return val;
	}
}
