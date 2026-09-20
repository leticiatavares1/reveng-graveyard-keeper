namespace Expressive.Operators;

public enum OperatorPrecedence
{
	Minimum,
	Or,
	And,
	Equal,
	NotEqual,
	LessThan,
	GreaterThan,
	LessThanOrEqual,
	GreaterThanOrEqual,
	Not,
	BitwiseOr,
	BitwiseXOr,
	BitwiseAnd,
	LeftShift,
	RightShift,
	Add,
	Subtract,
	Multiply,
	Modulus,
	Divide,
	NullCoalescing,
	UnaryPlus,
	UnaryMinus,
	ParenthesisOpen,
	ParenthesisClose
}
