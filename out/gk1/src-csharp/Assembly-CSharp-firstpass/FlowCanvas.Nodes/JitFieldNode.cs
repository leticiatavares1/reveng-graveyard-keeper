using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ParadoxNotion;

namespace FlowCanvas.Nodes;

public class JitFieldNode : BaseReflectedFieldNode
{
	private static readonly Dictionary<string, UniversalDelegate> GetDelegates = new Dictionary<string, UniversalDelegate>(StringComparer.Ordinal);

	private static readonly Dictionary<string, UniversalDelegate> SetDelegates = new Dictionary<string, UniversalDelegate>(StringComparer.Ordinal);

	private static readonly Type[] DynParamTypes = new Type[1] { typeof(UniversalDelegateParam[]) };

	private readonly Type[] tmpTypes = new Type[1];

	private UniversalDelegate getDelegat;

	private UniversalDelegate setDelegat;

	private UniversalDelegateParam[] delegateParams;

	private Action getValue;

	private bool isConstant;

	private void CreateDelegates()
	{
		string generatedKey = ReflectedNodesHelper.GetGeneratedKey(fieldInfo);
		if (!GetDelegates.TryGetValue(generatedKey, out getDelegat) || getDelegat == null)
		{
			DynamicMethod dynamicMethod = new DynamicMethod(fieldInfo.Name + "_DynamicGet", null, DynParamTypes, typeof(JitFieldNode));
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			int num = -1;
			int num2 = -1;
			int num3 = 0;
			int arg = -1;
			int arg2 = -1;
			for (int i = 0; i <= delegateParams.Length - 1; i++)
			{
				ParamDef paramDef = delegateParams[i].paramDef;
				if (paramDef.paramMode == ParamMode.Instance)
				{
					num = i;
					iLGenerator.DeclareLocal(paramDef.paramType);
					arg = num3;
					num3++;
				}
				if (paramDef.paramMode == ParamMode.Result)
				{
					num2 = i;
					iLGenerator.DeclareLocal(paramDef.paramType);
					arg2 = num3;
					num3++;
				}
			}
			if (num >= 0)
			{
				iLGenerator.Emit(OpCodes.Ldarg, 0);
				iLGenerator.Emit(OpCodes.Ldc_I4, num);
				iLGenerator.Emit(OpCodes.Ldelem_Ref);
				iLGenerator.Emit(OpCodes.Ldfld, delegateParams[num].ValueField);
				iLGenerator.Emit(OpCodes.Stloc, arg);
				iLGenerator.Emit(OpCodes.Ldloc, arg);
				iLGenerator.Emit(OpCodes.Ldfld, fieldInfo);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldsfld, fieldInfo);
			}
			if (num2 >= 0)
			{
				iLGenerator.Emit(OpCodes.Stloc, arg2);
				iLGenerator.Emit(OpCodes.Ldarg, 0);
				iLGenerator.Emit(OpCodes.Ldc_I4, num2);
				iLGenerator.Emit(OpCodes.Ldelem_Ref);
				iLGenerator.Emit(OpCodes.Ldloc, arg2);
				iLGenerator.Emit(OpCodes.Stfld, delegateParams[num2].ValueField);
			}
			iLGenerator.Emit(OpCodes.Ret);
			getDelegat = (UniversalDelegate)dynamicMethod.CreateDelegate(typeof(UniversalDelegate));
			GetDelegates[generatedKey] = getDelegat;
		}
		if ((SetDelegates.TryGetValue(generatedKey, out setDelegat) && setDelegat != null) || fieldInfo.IsReadOnly())
		{
			return;
		}
		DynamicMethod dynamicMethod2 = new DynamicMethod(fieldInfo.Name + "_DynamicSet", null, DynParamTypes, typeof(JitFieldNode));
		ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
		int num4 = -1;
		int num5 = -1;
		int num6 = 0;
		int arg3 = -1;
		int arg4 = -1;
		for (int j = 0; j <= delegateParams.Length - 1; j++)
		{
			ParamDef paramDef2 = delegateParams[j].paramDef;
			if (paramDef2.paramMode == ParamMode.Instance)
			{
				num4 = j;
				iLGenerator2.DeclareLocal(paramDef2.paramType);
				arg3 = num6;
				num6++;
			}
			if (paramDef2.paramMode == ParamMode.Result)
			{
				num5 = j;
				iLGenerator2.DeclareLocal(paramDef2.paramType);
				arg4 = num6;
				num6++;
			}
		}
		if (num4 >= 0)
		{
			iLGenerator2.Emit(OpCodes.Ldarg, 0);
			iLGenerator2.Emit(OpCodes.Ldc_I4, num4);
			iLGenerator2.Emit(OpCodes.Ldelem_Ref);
			iLGenerator2.Emit(OpCodes.Ldfld, delegateParams[num4].ValueField);
			iLGenerator2.Emit(OpCodes.Stloc, arg3);
		}
		if (num5 >= 0)
		{
			iLGenerator2.Emit(OpCodes.Ldarg, 0);
			iLGenerator2.Emit(OpCodes.Ldc_I4, num5);
			iLGenerator2.Emit(OpCodes.Ldelem_Ref);
			iLGenerator2.Emit(OpCodes.Ldfld, delegateParams[num5].ValueField);
			iLGenerator2.Emit(OpCodes.Stloc, arg4);
			if (num4 >= 0)
			{
				iLGenerator2.Emit(delegateParams[num4].GetCurrentType().RTIsValueType() ? OpCodes.Ldloca : OpCodes.Ldloc, arg3);
				iLGenerator2.Emit(OpCodes.Ldloc, arg4);
				iLGenerator2.Emit(OpCodes.Stfld, fieldInfo);
			}
			else
			{
				iLGenerator2.Emit(OpCodes.Ldloc, arg4);
				iLGenerator2.Emit(OpCodes.Stsfld, fieldInfo);
			}
		}
		if (num4 >= 0)
		{
			iLGenerator2.Emit(OpCodes.Ldarg, 0);
			iLGenerator2.Emit(OpCodes.Ldc_I4, num4);
			iLGenerator2.Emit(OpCodes.Ldelem_Ref);
			iLGenerator2.Emit(OpCodes.Ldloc, arg3);
			iLGenerator2.Emit(OpCodes.Stfld, delegateParams[num4].ValueField);
		}
		iLGenerator2.Emit(OpCodes.Ret);
		setDelegat = (UniversalDelegate)dynamicMethod2.CreateDelegate(typeof(UniversalDelegate));
		SetDelegates[generatedKey] = setDelegat;
	}

	protected override bool InitInternal(FieldInfo field)
	{
		isConstant = field.IsStatic && field.IsReadOnly();
		if (isConstant)
		{
			delegateParams = new UniversalDelegateParam[1];
			tmpTypes[0] = field.FieldType;
			delegateParams[0] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			delegateParams[0].paramDef = resultDef;
			delegateParams[0].SetFromValue(field.GetValue(null));
			return true;
		}
		getDelegat = null;
		setDelegat = null;
		int num = 0;
		if (instanceDef.paramMode != 0)
		{
			num++;
		}
		if (resultDef.paramMode != 0)
		{
			num++;
		}
		delegateParams = new UniversalDelegateParam[num];
		int num2 = 0;
		if (instanceDef.paramMode != 0)
		{
			tmpTypes[0] = instanceDef.paramType;
			delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			delegateParams[num2].paramDef = instanceDef;
			num2++;
		}
		if (resultDef.paramMode != 0)
		{
			tmpTypes[0] = resultDef.paramType;
			delegateParams[num2] = (UniversalDelegateParam)typeof(UniversalDelegateParam<>).RTMakeGenericType(tmpTypes).CreateObject();
			delegateParams[num2].paramDef = resultDef;
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

	private void SetValue()
	{
		if (setDelegat != null && !fieldInfo.IsReadOnly())
		{
			for (int i = 0; i <= delegateParams.Length - 1; i++)
			{
				delegateParams[i].SetFromInput();
			}
			setDelegat(delegateParams);
		}
	}

	private void GetValue()
	{
		if (getDelegat != null)
		{
			for (int i = 0; i <= delegateParams.Length - 1; i++)
			{
				delegateParams[i].SetFromInput();
			}
			getDelegat(delegateParams);
		}
	}

	public override void RegisterPorts(FlowNode node, ReflectedFieldNodeWrapper.AccessMode accessMode)
	{
		if (isConstant)
		{
			delegateParams[0].RegisterAsOutput(node);
		}
		if (getValue == null)
		{
			getValue = GetValue;
		}
		if (accessMode == ReflectedFieldNodeWrapper.AccessMode.SetField && !fieldInfo.IsReadOnly())
		{
			FlowOutput output = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", delegate(Flow flow)
			{
				SetValue();
				output.Call(flow);
			});
		}
		for (int i = 0; i <= delegateParams.Length - 1; i++)
		{
			UniversalDelegateParam universalDelegateParam = delegateParams[i];
			ParamDef paramDef = universalDelegateParam.paramDef;
			if (paramDef.paramMode == ParamMode.Instance)
			{
				universalDelegateParam.RegisterAsInput(node);
				if (accessMode == ReflectedFieldNodeWrapper.AccessMode.SetField && !fieldInfo.IsReadOnly())
				{
					universalDelegateParam.RegisterAsOutput(node);
				}
			}
			if (paramDef.paramMode == ParamMode.Result)
			{
				if (accessMode == ReflectedFieldNodeWrapper.AccessMode.SetField && !fieldInfo.IsReadOnly())
				{
					universalDelegateParam.RegisterAsInput(node);
				}
				else
				{
					universalDelegateParam.RegisterAsOutput(node, getValue);
				}
			}
		}
	}
}
