using System;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
	public class HtmlPageWrapper : ISqlTreeFormatter
	{
		private ISqlTreeFormatter _underlyingFormatter;

		private const string HTML_OUTER_PAGE = "<!DOCTYPE html >\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n<style type=\"text/css\">\r\n.SQLCode {{\r\n\tfont-size: 13px;\r\n\tfont-weight: bold;\r\n\tfont-family: monospace;;\r\n\twhite-space: pre;\r\n    -o-tab-size: 4;\r\n    -moz-tab-size: 4;\r\n    -webkit-tab-size: 4;\r\n}}\r\n.SQLComment {{\r\n\tcolor: #00AA00;\r\n}}\r\n.SQLString {{\r\n\tcolor: #AA0000;\r\n}}\r\n.SQLFunction {{\r\n\tcolor: #AA00AA;\r\n}}\r\n.SQLKeyword {{\r\n\tcolor: #0000AA;\r\n}}\r\n.SQLOperator {{\r\n\tcolor: #777777;\r\n}}\r\n.SQLErrorHighlight {{\r\n\tbackground-color: #FFFF00;\r\n}}\r\n\r\n\r\n</style>\r\n<pre class=\"SQLCode\">{0}</pre>\r\n</body>\r\n</html>\r\n";

		public bool HTMLFormatted => true;

		public string ErrorOutputPrefix
		{
			get
			{
				return _underlyingFormatter.ErrorOutputPrefix;
			}
			set
			{
				throw new NotSupportedException("Error output prefix should be set on the underlying formatter - it cannot be set on the Html Page Wrapper.");
			}
		}

		public HtmlPageWrapper(ISqlTreeFormatter underlyingFormatter)
		{
			if (underlyingFormatter == null)
			{
				throw new ArgumentNullException("underlyingFormatter");
			}
			_underlyingFormatter = underlyingFormatter;
		}

		public string FormatSQLTree(Node sqlTree)
		{
			string formattedResult;
			formattedResult = _underlyingFormatter.FormatSQLTree(sqlTree);
			if (_underlyingFormatter.HTMLFormatted)
			{
				return $"<!DOCTYPE html >\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n<style type=\"text/css\">\r\n.SQLCode {{\r\n\tfont-size: 13px;\r\n\tfont-weight: bold;\r\n\tfont-family: monospace;;\r\n\twhite-space: pre;\r\n    -o-tab-size: 4;\r\n    -moz-tab-size: 4;\r\n    -webkit-tab-size: 4;\r\n}}\r\n.SQLComment {{\r\n\tcolor: #00AA00;\r\n}}\r\n.SQLString {{\r\n\tcolor: #AA0000;\r\n}}\r\n.SQLFunction {{\r\n\tcolor: #AA00AA;\r\n}}\r\n.SQLKeyword {{\r\n\tcolor: #0000AA;\r\n}}\r\n.SQLOperator {{\r\n\tcolor: #777777;\r\n}}\r\n.SQLErrorHighlight {{\r\n\tbackground-color: #FFFF00;\r\n}}\r\n\r\n\r\n</style>\r\n<pre class=\"SQLCode\">{formattedResult}</pre>\r\n</body>\r\n</html>\r\n";
			}
			return $"<!DOCTYPE html >\r\n<html>\r\n<head>\r\n</head>\r\n<body>\r\n<style type=\"text/css\">\r\n.SQLCode {{\r\n\tfont-size: 13px;\r\n\tfont-weight: bold;\r\n\tfont-family: monospace;;\r\n\twhite-space: pre;\r\n    -o-tab-size: 4;\r\n    -moz-tab-size: 4;\r\n    -webkit-tab-size: 4;\r\n}}\r\n.SQLComment {{\r\n\tcolor: #00AA00;\r\n}}\r\n.SQLString {{\r\n\tcolor: #AA0000;\r\n}}\r\n.SQLFunction {{\r\n\tcolor: #AA00AA;\r\n}}\r\n.SQLKeyword {{\r\n\tcolor: #0000AA;\r\n}}\r\n.SQLOperator {{\r\n\tcolor: #777777;\r\n}}\r\n.SQLErrorHighlight {{\r\n\tbackground-color: #FFFF00;\r\n}}\r\n\r\n\r\n</style>\r\n<pre class=\"SQLCode\">{Utils.HtmlEncode(formattedResult)}</pre>\r\n</body>\r\n</html>\r\n";
		}
	}
}
