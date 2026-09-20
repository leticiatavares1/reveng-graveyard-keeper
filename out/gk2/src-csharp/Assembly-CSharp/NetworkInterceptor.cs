using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using LinqTools;

public class NetworkInterceptor : IInterceptor
{
	private List<object> contextData = new List<object>();

	public void SetContextData(params object[] contextData)
	{
		this.contextData = contextData.ToList();
	}

	public void Intercept(IInvocation invocation)
	{
		InterceptorInfo interceptorInfo = InterceptorCache.GetInterceptorInfo(invocation.MethodInvocationTarget ?? invocation.Method);
		if (interceptorInfo != null)
		{
			object obj = ((interceptorInfo.BaseGenericArgumentType != null) ? Activator.CreateInstance(interceptorInfo.TargetType, invocation.InvocationTarget) : Activator.CreateInstance(interceptorInfo.TargetType));
			int num = interceptorInfo.TargetMethod.GetParameters().Length - invocation.Arguments.Length;
			object[] array = new object[invocation.Arguments.Length + num];
			Array.Copy(invocation.Arguments, 0, array, 0, invocation.Arguments.Length);
			for (int i = 0; i < contextData.Count; i++)
			{
				int num2 = invocation.Arguments.Length + i;
				array[num2] = contextData[i];
			}
			interceptorInfo.TargetMethod.Invoke(obj, array);
		}
		else
		{
			invocation.Proceed();
		}
	}
}
