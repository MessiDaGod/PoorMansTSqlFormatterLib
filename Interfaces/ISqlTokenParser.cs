using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Interfaces
{
	public interface ISqlTokenParser
	{
		Node ParseSQL(ITokenList tokenList);
	}
}
