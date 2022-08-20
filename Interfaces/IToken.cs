namespace PoorMansTSqlFormatterLib.Interfaces
{
	public interface IToken
	{
		SqlTokenType Type { get; set; }

		string Value { get; set; }
	}
}
