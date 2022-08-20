using System.Runtime.InteropServices;
using PoorMansTSqlFormatterLib.Formatters;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.ParseStructure;
using PoorMansTSqlFormatterLib.Tokenizers;

namespace PoorMansTSqlFormatterLib
{
	[Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2B")]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	[ProgId("PoorMansTSqlFormatter.SqlFormattingManager")]
	public class SqlFormattingManager : _SqlFormattingManager
	{
		public ISqlTokenizer Tokenizer { get; set; }

		public ISqlTokenParser Parser { get; set; }

		public ISqlTreeFormatter Formatter { get; set; }

		public SqlFormattingManager()
			: this(new TSqlStandardTokenizer(), new TSqlStandardParser(), new TSqlStandardFormatter())
		{
		}

		public SqlFormattingManager(ISqlTreeFormatter formatter)
			: this(new TSqlStandardTokenizer(), new TSqlStandardParser(), formatter)
		{
		}

		public SqlFormattingManager(ISqlTokenizer tokenizer, ISqlTokenParser parser, ISqlTreeFormatter formatter)
		{
			Tokenizer = tokenizer;
			Parser = parser;
			Formatter = formatter;
		}

		public string Format(string inputSQL)
		{
			bool error;
			error = false;
			return Format(inputSQL, ref error);
		}

		public string Format(string inputSQL, ref bool errorEncountered)
		{
			Node sqlTree;
			sqlTree = Parser.ParseSQL(Tokenizer.TokenizeSQL(inputSQL));
			errorEncountered = sqlTree.GetAttributeValue("errorFound") == "1";
			return Formatter.FormatSQLTree(sqlTree);
		}

		public static string DefaultFormat(string inputSQL)
		{
			return new SqlFormattingManager().Format(inputSQL);
		}

		public static string DefaultFormat(string inputSQL, ref bool errorsEncountered)
		{
			return new SqlFormattingManager().Format(inputSQL, ref errorsEncountered);
		}
	}
}
