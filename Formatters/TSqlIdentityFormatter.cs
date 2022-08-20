using System;
using System.Collections.Generic;
using System.Text;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
	public class TSqlIdentityFormatter : ISqlTokenFormatter, ISqlTreeFormatter
	{
		public bool HTMLColoring { get; set; }

		public bool HTMLFormatted => HTMLColoring;

		public string ErrorOutputPrefix { get; set; }

		public TSqlIdentityFormatter()
			: this(htmlColoring: false)
		{
		}

		public TSqlIdentityFormatter(bool htmlColoring)
		{
			HTMLColoring = htmlColoring;
			ErrorOutputPrefix = "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!" + Environment.NewLine;
		}

		public string FormatSQLTree(Node sqlTreeDoc)
		{
			BaseFormatterState state;
			state = new BaseFormatterState(HTMLColoring);
			if (sqlTreeDoc.Name == "SqlRoot" && sqlTreeDoc.GetAttributeValue("errorFound") == "1")
			{
				state.AddOutputContent(ErrorOutputPrefix);
			}
			ProcessSqlNodeList(new Node[1] { sqlTreeDoc }, state);
			return state.DumpOutput();
		}

		private static void ProcessSqlNodeList(IEnumerable<Node> rootList, BaseFormatterState state)
		{
			foreach (Node contentElement in rootList)
			{
				ProcessSqlNode(state, contentElement);
			}
		}

		private static void ProcessSqlNode(BaseFormatterState state, Node contentElement)
		{
			if (contentElement.GetAttributeValue("hasError") == "1")
			{
				state.OpenClass("SQLErrorHighlight");
			}
			switch (contentElement.Name)
			{
			case "DDLDetailParens":
			case "DDLParens":
			case "FunctionParens":
			case "InParens":
			case "ExpressionParens":
			case "SelectionTargetParens":
				state.AddOutputContent("(");
				ProcessSqlNodeList(contentElement.Children, state);
				state.AddOutputContent(")");
				break;
			case "SqlRoot":
			case "SqlStatement":
			case "Clause":
			case "BooleanExpression":
			case "DDLProceduralBlock":
			case "DDLOtherBlock":
			case "DDLDeclareBlock":
			case "CursorDeclaration":
			case "BeginEndBlock":
			case "TryBlock":
			case "CatchBlock":
			case "CaseStatement":
			case "Input":
			case "When":
			case "Then":
			case "CaseElse":
			case "IfStatement":
			case "ElseClause":
			case "WhileLoop":
			case "DDLAsBlock":
			case "Between":
			case "LowerBound":
			case "UpperBound":
			case "CTEWithClause":
			case "CTEAlias":
			case "CTEAsBlock":
			case "CursorForBlock":
			case "CursorForOptions":
			case "TriggerCondition":
			case "CompoundKeyword":
			case "BeginTransaction":
			case "RollbackTransaction":
			case "SaveTransaction":
			case "CommitTransaction":
			case "BatchSeparator":
			case "SetOperatorClause":
			case "ContainerOpen":
			case "ContainerMultiStatementBody":
			case "ContainerSingleStatementBody":
			case "ContainerContentBody":
			case "ContainerClose":
			case "SelectionTarget":
			case "PermissionsBlock":
			case "PermissionsDetail":
			case "PermissionsTarget":
			case "PermissionsRecipient":
			case "DDLWith":
			case "MergeClause":
			case "MergeTarget":
			case "MergeUsing":
			case "MergeCondition":
			case "MergeWhen":
			case "MergeThen":
			case "MergeAction":
			case "JoinOn":
			case "DDLReturns":
				foreach (Node childNode in contentElement.Children)
				{
					ProcessSqlNode(state, childNode);
				}
				break;
			case "MultiLineComment":
				state.AddOutputContent("/*" + contentElement.TextValue + "*/", "SQLComment");
				break;
			case "SingleLineComment":
				state.AddOutputContent("--" + contentElement.TextValue, "SQLComment");
				break;
			case "SingleLineCommentCStyle":
				state.AddOutputContent("//" + contentElement.TextValue, "SQLComment");
				break;
			case "String":
				state.AddOutputContent("'" + contentElement.TextValue.Replace("'", "''") + "'", "SQLString");
				break;
			case "NationalString":
				state.AddOutputContent("N'" + contentElement.TextValue.Replace("'", "''") + "'", "SQLString");
				break;
			case "QuotedString":
				state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
				break;
			case "BracketQuotedName":
				state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
				break;
			case "Comma":
			case "Period":
			case "Semicolon":
			case "Asterisk":
			case "EqualsSign":
			case "ScopeResolutionOperator":
			case "AlphaOperator":
			case "OtherOperator":
				state.AddOutputContent(contentElement.TextValue, "SQLOperator");
				break;
			case "And":
			case "Or":
				state.AddOutputContent(contentElement.ChildByName("OtherKeyword").TextValue, "SQLOperator");
				break;
			case "FunctionKeyword":
				state.AddOutputContent(contentElement.TextValue, "SQLFunction");
				break;
			case "OtherKeyword":
			case "DataTypeKeyword":
			case "PseudoName":
				state.AddOutputContent(contentElement.TextValue, "SQLKeyword");
				break;
			case "Other":
			case "WhiteSpace":
			case "NumberValue":
			case "MonetaryValue":
			case "BinaryValue":
			case "Label":
				state.AddOutputContent(contentElement.TextValue);
				break;
			default:
				throw new Exception("Unrecognized element in SQL Xml!");
			}
			if (contentElement.GetAttributeValue("hasError") == "1")
			{
				state.CloseClass();
			}
		}

		public string FormatSQLTokens(ITokenList sqlTokenList)
		{
			StringBuilder outString;
			outString = new StringBuilder();
			if (sqlTokenList.HasUnfinishedToken)
			{
				outString.Append(ErrorOutputPrefix);
			}
			foreach (IToken entry in sqlTokenList)
			{
				switch (entry.Type)
				{
				case SqlTokenType.MultiLineComment:
					outString.Append("/*");
					outString.Append(entry.Value);
					outString.Append("*/");
					break;
				case SqlTokenType.SingleLineComment:
					outString.Append("--");
					outString.Append(entry.Value);
					break;
				case SqlTokenType.SingleLineCommentCStyle:
					outString.Append("//");
					outString.Append(entry.Value);
					break;
				case SqlTokenType.String:
					outString.Append("'");
					outString.Append(entry.Value.Replace("'", "''"));
					outString.Append("'");
					break;
				case SqlTokenType.NationalString:
					outString.Append("N'");
					outString.Append(entry.Value.Replace("'", "''"));
					outString.Append("'");
					break;
				case SqlTokenType.QuotedString:
					outString.Append("\"");
					outString.Append(entry.Value.Replace("\"", "\"\""));
					outString.Append("\"");
					break;
				case SqlTokenType.BracketQuotedName:
					outString.Append("[");
					outString.Append(entry.Value.Replace("]", "]]"));
					outString.Append("]");
					break;
				case SqlTokenType.OpenParens:
				case SqlTokenType.CloseParens:
				case SqlTokenType.WhiteSpace:
				case SqlTokenType.OtherNode:
				case SqlTokenType.Comma:
				case SqlTokenType.Period:
				case SqlTokenType.Semicolon:
				case SqlTokenType.Colon:
				case SqlTokenType.Asterisk:
				case SqlTokenType.EqualsSign:
				case SqlTokenType.MonetaryValue:
				case SqlTokenType.Number:
				case SqlTokenType.BinaryValue:
				case SqlTokenType.OtherOperator:
				case SqlTokenType.PseudoName:
					outString.Append(entry.Value);
					break;
				default:
					throw new Exception("Unrecognized Token Type in Token List!");
				}
			}
			return outString.ToString();
		}
	}
}
