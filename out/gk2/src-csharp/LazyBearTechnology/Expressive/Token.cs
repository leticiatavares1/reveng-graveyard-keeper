namespace Expressive;

public sealed class Token
{
	public string CurrentToken { get; }

	public int Length { get; }

	public int StartIndex { get; }

	public Token(string currentToken, int startIndex)
	{
		CurrentToken = currentToken;
		StartIndex = startIndex;
		Length = CurrentToken?.Length ?? 0;
	}
}
