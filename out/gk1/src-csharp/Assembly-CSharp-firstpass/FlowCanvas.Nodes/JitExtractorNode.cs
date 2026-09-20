using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ParadoxNotion;

namespace FlowCanvas.Nodes;

public class JitExtractorNode : BaseReflectedExtractorNode
{
	private static readonly Type[] DynParamTypes = new Type[1] { typeof(UniversalDelegateParam[]) };

	private readonly Type[] tmpTypes = new Type[1];

	private UniversalDelegateParam[] delegateParams;

	private void CreateDelegates()
	{
		int num = -1;
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			if (universalDelegateParam != null && universalDelegateParam.paramDef.paramMode == ParamMode.Instance)
			{
				num = i;
			}
		}
		for (int j = 0; j <= delegateParams.Length - 1; j++)
		{
			UniversalDelegateParam universalDelegateParam2 = delegateParams[j];
			if (universalDelegateParam2 == null)
			{
				continue;
			}
			ParamDef paramDef = universalDelegateParam2.paramDef;
			FieldInfo fieldInfo = paramDef.presentedInfo as FieldInfo;
			MethodInfo methodInfo = paramDef.presentedInfo as MethodInfo;
			if (paramDef.paramMode == ParamMode.Instance || (fieldInfo != null && fieldInfo.IsStatic && fieldInfo.IsReadOnly()))
			{
				continue;
			}
			DynamicMethod dynamicMethod = new DynamicMethod(base.TargetType.Name + "_" + paramDef.portId + "_Extractor", null, DynParamTypes, typeof(JitFieldNode));
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			int num2 = -1;
			int num3 = 0;
			if (num >= 0)
			{
				UniversalDelegateParam universalDelegateParam3 = delegateParams[num];
				iLGenerator.DeclareLocal(universalDelegateParam3.GetCurrentType());
				num2++;
				num3++;
			}
			iLGenerator.DeclareLocal(universalDelegateParam2.GetCurrentType());
			if (num >= 0)
			{
				UniversalDelegateParam universalDelegateParam4 = delegateParams[num];
				iLGenerator.Emit(OpCodes.Ldarg, 0);
				iLGenerator.Emit(OpCodes.Ldc_I4, num2);
				iLGenerator.Emit(OpCodes.Ldelem_Ref);
				iLGenerator.Emit(OpCodes.Ldfld, universalDelegateParam4.ValueField);
				iLGenerator.Emit(OpCodes.Stloc, num2);
			}
			if (fieldInfo != null)
			{
				if (num >= 0)
				{
					iLGenerator.Emit(OpCodes.Ldloc, num2);
					iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
				}
			}
			if (methodInfo != null)
			{
				if (num >= 0)
				{
					iLGenerator.Emit(delegateParams[num].GetCurrentType().RTIsValueType() ? OpCodes.Ldloca : OpCodes.Ldloc, num2);
				}
				if (num < 0 || delegateParams[num].GetCurrentType().RTIsValueType())
				{
					iLGenerator.Emit(OpCodes.Call, methodInfo);
				}
				else
				{
					iLGenerator.Emit(OpCodes.Callvirt, methodInfo);
				}
			}
			iLGenerator.Emit(OpCodes.Stloc, num3);
			iLGenerator.Emit(OpCodes.Ldarg, 0);
			iLGenerator.Emit(OpCodes.Ldc_I4, num3);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Ldloc, num3);
			iLGenerator.Emit(OpCodes.Stfld, universalDelegateParam2.ValueField);
			iLGenerator.Emit(OpCodes.Ret);
			universalDelegateParam2.referencedDelegate = (UniversalDelegate)dynamicMethod.CreateDelegate(typeof(UniversalDelegate));
			universalDelegateParam2.referencedParams = ((num < 0) ? new UniversalDelegateParam[1] { universalDelegateParam2 } : new UniversalDelegateParam[2]
			{
				delegateParams[num],
				universalDelegateParam2
			});
		}
	}

	private void Call(UniversalDelegateParam targetParam)
	{
		if (targetParam == null || targetParam.referencedDelegate == null || targetParam.referencedParams == null)
		{
			return;
		}
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			if (universalDelegateParam != null && universalDelegateParam.paramDef.paramMode == ParamMode.Instance)
			{
				universalDelegateParam.SetFromInput();
				break;
			}
		}
		targetParam.referencedDelegate(targetParam.referencedParams);
	}

	protected override bool InitInternal()
	{
		int num = 0;
		if (base.Params.instanceDef.paramMode == ParamMode.Instance)
		{
			num++;
		}
		List<ParamDef> list = base.Params.paramDefinitions ?? new List<ParamDef>();
		for (int i = 0; i <= list.Count - 1; i++)
		{
			ParamDef paramDef = list[i];
			if (paramDef.paramMode == ParamMode.Out && !(paramDef.presentedInfo == null))
			{
				num++;
			}
		}
		delegateParams = new UniversalDelegateParam[num];
		int num2 = 0;
		if (base.Params.instanceDef.paramMode == ParamMode.Instance)
		{
			tmpTypes[0] = base.TargetType;
			delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			delegateParams[num2].paramDef = base.Params.instanceDef;
			num2++;
		}
		for (int j = 0; j <= list.Count - 1; j++)
		{
			ParamDef paramDef2 = list[j];
			if (paramDef2.paramMode == ParamMode.Out && !(paramDef2.presentedInfo == null))
			{
				tmpTypes[0] = paramDef2.paramType;
				delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
				delegateParams[num2].paramDef = paramDef2;
				num2++;
			}
		}
		try
		{
			CreateDelegates();
		}
		catch
		{
			return false;
		}
		return true;
	}

	public override void RegisterPorts(FlowNode node)
	{
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			if (universalDelegateParam == null)
			{
				continue;
			}
			ParamDef paramDef = universalDelegateParam.paramDef;
			if (paramDef.paramMode == ParamMode.Instance)
			{
				universalDelegateParam.RegisterAsInput(node);
			}
			if (paramDef.paramMode == ParamMode.Out)
			{
				FieldInfo fieldInfo = paramDef.presentedInfo as FieldInfo;
				if (fieldInfo != null && fieldInfo.IsStatic && fieldInfo.IsReadOnly())
				{
					universalDelegateParam.SetFromValue(fieldInfo.GetValue(null));
					universalDelegateParam.RegisterAsOutput(node);
				}
				else
				{
					universalDelegateParam.RegisterAsOutput(node, Call);
				}
			}
		}
	}
}
