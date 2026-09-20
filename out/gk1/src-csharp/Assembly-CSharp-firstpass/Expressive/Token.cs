namespace Expressive;

internal sealed class Token
{
	internal string CurrentToken { get; private set; }

	internal int Length { get; private set; }

	internal int StartIndex { get; private set; }

	public Token(string currentToken, int startIndex)
	{
		CurrentToken = currentToken;
		StartIndex = startIndex;
		Length = ((CurrentToken != null) ? CurrentToken.Length : 0);
	}
}
