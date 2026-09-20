using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ParadoxNotion;

namespace FlowCanvas.Nodes;

public class JitConstructorNode : BaseReflectedConstructorNode
{
	private static readonly Dictionary<string, UniversalDelegate> Delegates = new Dictionary<string, UniversalDelegate>(StringComparer.Ordinal);

	private static readonly Type[] DynParamTypes = new Type[1] { typeof(UniversalDelegateParam[]) };

	private readonly Type[] tmpTypes = new Type[1];

	private readonly Type[] tmpTypes2 = new Type[2];

	private UniversalDelegate delegat;

	private UniversalDelegateParam[] delegateParams;

	private Action actionCall;

	private void CreateDelegat()
	{
		string generatedKey = ReflectedNodesHelper.GetGeneratedKey(constructorInfo);
		if (Delegates.TryGetValue(generatedKey, out delegat) && delegat != null)
		{
			return;
		}
		DynamicMethod dynamicMethod = new DynamicMethod("Constructor_Dynamic", null, DynParamTypes, typeof(JitMethodNode));
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		int num = -1;
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			ParamDef paramDef = universalDelegateParam.paramDef;
			iLGenerator.DeclareLocal(universalDelegateParam.GetCurrentType());
			if (paramDef.paramMode == ParamMode.Result)
			{
				num = i;
			}
		}
		for (int j = 0; j <= delegateParams.Length - 1; j++)
		{
			UniversalDelegateParam universalDelegateParam2 = delegateParams[j];
			iLGenerator.Emit(OpCodes.Ldarg, 0);
			iLGenerator.Emit(OpCodes.Ldc_I4, j);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Ldfld, universalDelegateParam2.ValueField);
			iLGenerator.Emit(OpCodes.Stloc, j);
		}
		for (int k = 0; k <= delegateParams.Length - 1; k++)
		{
			ParamDef paramDef2 = delegateParams[k].paramDef;
			if (paramDef2.paramMode != ParamMode.Instance && paramDef2.paramMode != ParamMode.Result)
			{
				if (paramDef2.paramMode == ParamMode.In)
				{
					iLGenerator.Emit(OpCodes.Ldloc, k);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldloca, k);
				}
			}
		}
		iLGenerator.Emit(OpCodes.Newobj, constructorInfo);
		if (num >= 0)
		{
			iLGenerator.Emit(OpCodes.Stloc, num);
		}
		for (int l = 0; l <= delegateParams.Length - 1; l++)
		{
			UniversalDelegateParam universalDelegateParam3 = delegateParams[l];
			iLGenerator.Emit(OpCodes.Ldarg, 0);
			iLGenerator.Emit(OpCodes.Ldc_I4, l);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Ldloc, l);
			iLGenerator.Emit(OpCodes.Stfld, universalDelegateParam3.ValueField);
		}
		iLGenerator.Emit(OpCodes.Ret);
		delegat = (UniversalDelegate)dynamicMethod.CreateDelegate(typeof(UniversalDelegate));
		Delegates[generatedKey] = delegat;
	}

	private void Call()
	{
		if (delegat != null)
		{
			for (int i = 0; i <= delegateParams.Length - 1; i++)
			{
				delegateParams[i].SetFromInput();
			}
			delegat(delegateParams);
		}
	}

	protected override bool InitInternal(ConstructorInfo constructor)
	{
		delegat = null;
		int num = paramDefinitions.Count;
		if (resultDef.paramMode != 0)
		{
			num++;
		}
		delegateParams = new UniversalDelegateParam[num];
		int num2 = 0;
		if (resultDef.paramMode != 0)
		{
			tmpTypes[0] = resultDef.paramType;
			delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			delegateParams[num2].paramDef = resultDef;
			num2++;
		}
		for (int i = 0; i <= paramDefinitions.Count - 1; i++)
		{
			if (options.exposeParams && paramDefinitions[i].isParamsArray)
			{
				tmpTypes2[0] = paramDefinitions[i].paramType;
				tmpTypes2[1] = paramDefinitions[i].arrayType;
				delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<, >).RTMakeGenericType(tmpTypes2).CreateObject();
				delegateParams[num2].paramsArrayNeeded = options.exposeParams;
				delegateParams[num2].paramsArrayCount = options.exposedParamsCount;
			}
			else
			{
				tmpTypes[0] = paramDefinitions[i].paramType;
				delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			}
			delegateParams[num2].paramDef = paramDefinitions[i];
			num2++;
		}
		try
		{
			CreateDelegat();
		}
		catch
		{
			return false;
		}
		return true;
	}

	public override void RegisterPorts(FlowNode node, ReflectedMethodRegistrationOptions options)
	{
		if (actionCall == null)
		{
			actionCall = Call;
		}
		if (options.callable)
		{
			FlowOutput output = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", delegate(Flow flow)
			{
				Call();
				output.Call(flow);
			});
		}
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			ParamDef paramDef = universalDelegateParam.paramDef;
			if (paramDef.paramMode == ParamMode.Result)
			{
				universalDelegateParam.RegisterAsOutput(node, options.callable ? null : actionCall);
			}
			else if (paramDef.paramMode == ParamMode.Ref)
			{
				universalDelegateParam.RegisterAsInput(node);
				universalDelegateParam.RegisterAsOutput(node, options.callable ? null : actionCall);
			}
			else if (paramDef.paramMode == ParamMode.In)
			{
				universalDelegateParam.RegisterAsInput(node);
			}
			else if (paramDef.paramMode == ParamMode.Out)
			{
				universalDelegateParam.RegisterAsOutput(node, options.callable ? null : actionCall);
			}
		}
	}
}
