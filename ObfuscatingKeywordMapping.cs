using System.Collections.Generic;

namespace PoorMansTSqlFormatterLib
{
	internal static class ObfuscatingKeywordMapping
	{
		public static Dictionary<string, string> Instance { get; private set; }

		static ObfuscatingKeywordMapping()
		{
			Instance = new Dictionary<string, string>();
			Instance.Add("PROCEDURE", "PROC");
			Instance.Add("LEFT OUTER JOIN", "LEFT JOIN");
			Instance.Add("RIGHT OUTER JOIN", "RIGHT JOIN");
			Instance.Add("FULL OUTER JOIN", "FULL JOIN");
			Instance.Add("INNER JOIN", "JOIN");
			Instance.Add("TRANSACTION", "TRAN");
			Instance.Add("BEGIN TRANSACTION", "BEGIN TRAN");
			Instance.Add("COMMIT TRANSACTION", "COMMIT TRAN");
			Instance.Add("ROLLBACK TRANSACTION", "ROLLBACK TRAN");
			Instance.Add("VARBINARY", "BINARY VARYING");
			Instance.Add("VARCHAR", "CHARACTER VARYING");
			Instance.Add("CHARACTER", "CHAR");
			Instance.Add("CHAR VARYING", "VARCHAR");
			Instance.Add("DECIMAL", "DEC");
			Instance.Add("FLOAT", "DOUBLE PRECISION");
			Instance.Add("INTEGER", "INT");
			Instance.Add("NCHAR", "NATIONAL CHARACTER");
			Instance.Add("NATIONAL CHAR", "NCHAR");
			Instance.Add("NVARCHAR", "NATIONAL CHARACTER VARYING");
			Instance.Add("NATIONAL CHAR VARYING", "NVARCHAR");
			Instance.Add("NTEXT", "NATIONAL TEXT");
		}
	}
}
