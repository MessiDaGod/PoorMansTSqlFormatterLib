namespace PoorMansTSqlFormatterLib.Interfaces
{
	public interface ISqlTokenFormatter
	{
		string ErrorOutputPrefix { get; set; }

		string FormatSQLTokens(ITokenList sqlTokenList);
	}
}
