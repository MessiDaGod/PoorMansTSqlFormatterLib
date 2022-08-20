using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Interfaces
{
	public interface ISqlTreeFormatter
	{
		bool HTMLFormatted { get; }

		string ErrorOutputPrefix { get; set; }

		string FormatSQLTree(Node sqlTree);
	}
}
