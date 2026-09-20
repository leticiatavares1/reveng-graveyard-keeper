namespace FlowCanvas.Nodes;

public abstract class PureFunctionNode<TResult> : PureFunctionNodeBase
{
	public abstract TResult Invoke();

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		node.AddValueOutput("Value", () => Invoke());
	}
}
public abstract class PureFunctionNode<TResult, T1> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		node.AddValueOutput("Value", () => Invoke(p1.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p5 = node.AddValueInput<T5>(GetParameterName(4));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value, p5.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5, T6> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p5 = node.AddValueInput<T5>(GetParameterName(4));
		ValueInput<T6> p6 = node.AddValueInput<T6>(GetParameterName(5));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5, T6, T7> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p5 = node.AddValueInput<T5>(GetParameterName(4));
		ValueInput<T6> p6 = node.AddValueInput<T6>(GetParameterName(5));
		ValueInput<T7> p7 = node.AddValueInput<T7>(GetParameterName(6));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5, T6, T7, T8> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p5 = node.AddValueInput<T5>(GetParameterName(4));
		ValueInput<T6> p6 = node.AddValueInput<T6>(GetParameterName(5));
		ValueInput<T7> p7 = node.AddValueInput<T7>(GetParameterName(6));
		ValueInput<T8> p8 = node.AddValueInput<T8>(GetParameterName(7));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h, T9 i);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p2 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p3 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p4 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p5 = node.AddValueInput<T5>(GetParameterName(4));
		ValueInput<T6> p6 = node.AddValueInput<T6>(GetParameterName(5));
		ValueInput<T7> p7 = node.AddValueInput<T7>(GetParameterName(6));
		ValueInput<T8> p8 = node.AddValueInput<T8>(GetParameterName(7));
		ValueInput<T9> p9 = node.AddValueInput<T9>(GetParameterName(8));
		node.AddValueOutput("Value", () => Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value, p9.value));
	}
}
public abstract class PureFunctionNode<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : PureFunctionNodeBase
{
	public abstract TResult Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h, T9 i, T10 j);

	protected sealed override void OnRegisterPorts(FlowNode node)
	{
		ValueInput<T1> p1 = node.AddValueInput<T1>(GetParameterName(0));
		ValueInput<T2> p3 = node.AddValueInput<T2>(GetParameterName(1));
		ValueInput<T3> p4 = node.AddValueInput<T3>(GetParameterName(2));
		ValueInput<T4> p5 = node.AddValueInput<T4>(GetParameterName(3));
		ValueInput<T5> p6 = node.AddValueInput<T5>(GetParameterName(4));
		ValueInput<T6> p7 = node.AddValueInput<T6>(GetParameterName(5));
		ValueInput<T7> p8 = node.AddValueInput<T7>(GetParameterName(6));
		ValueInput<T8> p9 = node.AddValueInput<T8>(GetParameterName(7));
		ValueInput<T9> p10 = node.AddValueInput<T9>(GetParameterName(8));
		ValueInput<T10> p2 = node.AddValueInput<T10>(GetParameterName(9));
		node.AddValueOutput("Value", () => Invoke(p1.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value, p9.value, p10.value, p2.value));
	}
}
