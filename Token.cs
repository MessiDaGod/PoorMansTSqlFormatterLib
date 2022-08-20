using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib
{
	public class Token : IToken
	{
		public SqlTokenType Type { get; set; }

		public string Value { get; set; }

		public Token(SqlTokenType type, string value)
		{
			Type = type;
			Value = value;
		}
	}
}
