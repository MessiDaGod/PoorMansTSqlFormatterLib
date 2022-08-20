using System;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
	public class TSqlObfuscatingFormatter : ISqlTreeFormatter
	{
		private class TSqlObfuscatingFormattingState : BaseFormatterState
		{
			private const int MIN_COLOR_WORD_LENGTH = 3;

			private const int MAX_COLOR_WORD_LENGTH = 15;

			private const int MIN_LINE_LENGTH = 10;

			private const int MAX_LINE_LENGTH = 80;

			private Random _randomizer = new Random();

			private int _currentLineLength;

			private int _thisLineLimit = 80;

			private int _currentColorLength;

			private int _currentColorLimit = 15;

			private string _currentColor;

			private bool RandomizeColor { get; set; }

			private bool RandomizeLineLength { get; set; }

			internal bool BreakExpected { get; set; }

			internal bool SpaceExpectedForAnsiString { get; set; }

			internal bool SpaceExpectedForE { get; set; }

			internal bool SpaceExpectedForX { get; set; }

			internal bool SpaceExpectedForPlusMinus { get; set; }

			internal bool SpaceExpected { get; set; }

			public TSqlObfuscatingFormattingState(bool randomizeColor, bool randomizeLineLength)
				: base(randomizeColor)
			{
				RandomizeColor = randomizeColor;
				RandomizeLineLength = randomizeLineLength;
				if (RandomizeColor)
				{
					_currentColorLimit = _randomizer.Next(3, 15);
					_currentColor = $"#{_randomizer.Next(0, 127):x2}{_randomizer.Next(0, 127):x2}{_randomizer.Next(0, 127):x2}";
				}
				if (RandomizeLineLength)
				{
					_thisLineLimit = _randomizer.Next(10, 80);
				}
			}

			public void BreakIfExpected()
			{
				if (BreakExpected)
				{
					BreakExpected = false;
					base.AddOutputLineBreak();
					SetSpaceNoLongerExpected();
					_currentLineLength = 0;
					if (RandomizeLineLength)
					{
						_thisLineLimit = _randomizer.Next(10, 80);
					}
				}
			}

			public void SpaceIfExpectedForAnsiString()
			{
				if (SpaceExpectedForAnsiString)
				{
					base.AddOutputContent(" ", null);
					SetSpaceNoLongerExpected();
				}
			}

			public void SpaceIfExpected()
			{
				if (SpaceExpected)
				{
					base.AddOutputContent(" ", null);
					SetSpaceNoLongerExpected();
				}
			}

			public override void AddOutputContent(string content, string htmlClassName)
			{
				if (htmlClassName != null)
				{
					throw new NotSupportedException("Obfuscating formatter does not use html class names...");
				}
				BreakIfExpected();
				SpaceIfExpected();
				if (_currentLineLength > 0 && _currentLineLength + content.Length > _thisLineLimit)
				{
					BreakExpected = true;
					BreakIfExpected();
				}
				else if ((SpaceExpectedForE && content.Substring(0, 1).ToLower().Equals("e")) || (SpaceExpectedForX && content.Substring(0, 1).ToLower().Equals("x")) || (SpaceExpectedForPlusMinus && content.Substring(0, 1).Equals("+")) || (SpaceExpectedForPlusMinus && content.Substring(0, 1).Equals("-")))
				{
					SpaceExpected = true;
					SpaceIfExpected();
				}
				_currentLineLength += content.Length;
				if (RandomizeColor)
				{
					int lengthWritten;
					lengthWritten = 0;
					while (lengthWritten < content.Length)
					{
						if (_currentColorLength == _currentColorLimit)
						{
							_currentColorLimit = _randomizer.Next(3, 15);
							_currentColor = $"#{_randomizer.Next(0, 127):x2}{_randomizer.Next(0, 127):x2}{_randomizer.Next(0, 127):x2}";
							_currentColorLength = 0;
						}
						int writing;
						writing = ((content.Length - lengthWritten >= _currentColorLimit - _currentColorLength) ? (_currentColorLimit - _currentColorLength) : (content.Length - lengthWritten));
						base.AddOutputContentRaw("<span style=\"color: ");
						base.AddOutputContentRaw(_currentColor);
						base.AddOutputContentRaw(";\">");
						base.AddOutputContentRaw(Utils.HtmlEncode(content.Substring(lengthWritten, writing)));
						base.AddOutputContentRaw("</span>");
						lengthWritten += writing;
						_currentColorLength += writing;
					}
				}
				else
				{
					base.AddOutputContent(content, null);
				}
				SetSpaceNoLongerExpected();
			}

			private void SetSpaceNoLongerExpected()
			{
				SpaceExpected = false;
				SpaceExpectedForAnsiString = false;
				SpaceExpectedForE = false;
				SpaceExpectedForX = false;
				SpaceExpectedForPlusMinus = false;
			}

			public override void AddOutputLineBreak()
			{
				throw new NotSupportedException();
			}
		}

		public IDictionary<string, string> KeywordMapping = new Dictionary<string, string>();

		private const int MIN_CASE_WORD_LENGTH = 2;

		private const int MAX_CASE_WORD_LENGTH = 8;

		private Random _randomizer = new Random();

		private int _currentCaseLength;

		private int _currentCaseLimit = 8;

		private bool _currentlyUppercase;

		public bool RandomizeCase { get; set; }

		public bool RandomizeColor { get; set; }

		public bool RandomizeLineLength { get; set; }

		public bool PreserveComments { get; set; }

		public bool HTMLFormatted => RandomizeColor;

		public string ErrorOutputPrefix { get; set; }

		public TSqlObfuscatingFormatter()
			: this(randomizeCase: false, randomizeColor: false, randomizeLineLength: false, preserveComments: false, subtituteKeywords: false)
		{
		}

		public TSqlObfuscatingFormatter(bool randomizeCase, bool randomizeColor, bool randomizeLineLength, bool preserveComments, bool subtituteKeywords)
		{
			RandomizeCase = randomizeCase;
			RandomizeColor = randomizeColor;
			RandomizeLineLength = randomizeLineLength;
			PreserveComments = preserveComments;
			if (subtituteKeywords)
			{
				KeywordMapping = ObfuscatingKeywordMapping.Instance;
			}
			ErrorOutputPrefix = "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!" + Environment.NewLine;
			if (RandomizeCase)
			{
				_currentCaseLimit = _randomizer.Next(2, 8);
				_currentlyUppercase = _randomizer.Next(0, 2) == 0;
			}
		}

		public string FormatSQLTree(Node sqlTreeDoc)
		{
			TSqlObfuscatingFormattingState state;
			state = new TSqlObfuscatingFormattingState(RandomizeColor, RandomizeLineLength);
			if (sqlTreeDoc.Name == "SqlRoot" && sqlTreeDoc.GetAttributeValue("errorFound") == "1")
			{
				state.AddOutputContent(ErrorOutputPrefix);
			}
			ProcessSqlNodeList(new Node[1] { sqlTreeDoc }, state);
			state.BreakIfExpected();
			return state.DumpOutput();
		}

		private void ProcessSqlNodeList(IEnumerable<Node> rootList, TSqlObfuscatingFormattingState state)
		{
			foreach (Node contentElement in rootList)
			{
				ProcessSqlNode(contentElement, state);
			}
		}

		private void ProcessSqlNode(Node contentElement, TSqlObfuscatingFormattingState state)
		{
			switch (contentElement.Name)
			{
			case "SqlRoot":
			case "SqlStatement":
			case "Clause":
			case "SetOperatorClause":
			case "DDLProceduralBlock":
			case "DDLOtherBlock":
			case "DDLDeclareBlock":
			case "CursorDeclaration":
			case "BeginTransaction":
			case "SaveTransaction":
			case "CommitTransaction":
			case "RollbackTransaction":
			case "ContainerOpen":
			case "ContainerClose":
			case "WhileLoop":
			case "IfStatement":
			case "SelectionTarget":
			case "ContainerContentBody":
			case "CTEWithClause":
			case "PermissionsBlock":
			case "PermissionsDetail":
			case "MergeClause":
			case "MergeTarget":
			case "Input":
			case "BooleanExpression":
			case "LowerBound":
			case "UpperBound":
			case "ContainerSingleStatementBody":
			case "ContainerMultiStatementBody":
			case "MergeAction":
			case "PermissionsTarget":
			case "PermissionsRecipient":
			case "DDLWith":
			case "MergeCondition":
			case "MergeThen":
			case "JoinOn":
			case "CTEAlias":
			case "ElseClause":
			case "DDLAsBlock":
			case "CursorForBlock":
			case "TriggerCondition":
			case "CursorForOptions":
			case "CTEAsBlock":
			case "DDLReturns":
			case "MergeUsing":
			case "MergeWhen":
			case "Between":
			case "BeginEndBlock":
			case "TryBlock":
			case "CatchBlock":
			case "CaseStatement":
			case "When":
			case "Then":
			case "CaseElse":
			case "And":
			case "Or":
				ProcessSqlNodeList(contentElement.Children, state);
				break;
			case "DDLDetailParens":
			case "FunctionParens":
			case "InParens":
			case "DDLParens":
			case "ExpressionParens":
			case "SelectionTargetParens":
				state.SpaceExpected = false;
				state.AddOutputContent("(");
				ProcessSqlNodeList(contentElement.Children, state);
				state.SpaceExpected = false;
				state.SpaceExpectedForAnsiString = false;
				state.AddOutputContent(")");
				break;
			case "WhiteSpace":
				break;
			case "MultiLineComment":
				if (PreserveComments)
				{
					state.SpaceExpected = false;
					state.AddOutputContent("/*" + contentElement.TextValue + "*/");
				}
				break;
			case "SingleLineComment":
				if (PreserveComments)
				{
					state.SpaceExpected = false;
					state.AddOutputContent("--" + contentElement.TextValue.Replace("\r", "").Replace("\n", ""));
					state.BreakExpected = true;
				}
				break;
			case "SingleLineCommentCStyle":
				if (PreserveComments)
				{
					state.SpaceExpected = false;
					state.AddOutputContent("//" + contentElement.TextValue.Replace("\r", "").Replace("\n", ""));
					state.BreakExpected = true;
				}
				break;
			case "BatchSeparator":
				state.BreakExpected = true;
				ProcessSqlNodeList(contentElement.Children, state);
				state.BreakExpected = true;
				break;
			case "String":
				state.SpaceIfExpectedForAnsiString();
				state.SpaceExpected = false;
				state.AddOutputContent("'" + contentElement.TextValue.Replace("'", "''") + "'");
				state.SpaceExpectedForAnsiString = true;
				break;
			case "NationalString":
				state.AddOutputContent("N'" + contentElement.TextValue.Replace("'", "''") + "'");
				state.SpaceExpectedForAnsiString = true;
				break;
			case "BracketQuotedName":
				state.SpaceExpected = false;
				state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
				break;
			case "QuotedString":
				state.SpaceExpected = false;
				state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
				break;
			case "Comma":
			case "Period":
			case "Semicolon":
			case "ScopeResolutionOperator":
			case "Asterisk":
			case "EqualsSign":
			case "OtherOperator":
				state.SpaceExpected = false;
				state.AddOutputContent(contentElement.TextValue);
				break;
			case "CompoundKeyword":
				state.AddOutputContent(FormatKeyword(contentElement.GetAttributeValue("simpleText")));
				state.SpaceExpected = true;
				break;
			case "Label":
				state.AddOutputContent(contentElement.TextValue);
				state.BreakExpected = true;
				break;
			case "OtherKeyword":
			case "AlphaOperator":
			case "DataTypeKeyword":
			case "PseudoName":
			case "BinaryValue":
				state.AddOutputContent(FormatKeyword(contentElement.TextValue));
				state.SpaceExpected = true;
				break;
			case "NumberValue":
				state.AddOutputContent(FormatKeyword(contentElement.TextValue));
				if (!contentElement.TextValue.ToLowerInvariant().Contains("e"))
				{
					state.SpaceExpectedForE = true;
					if (contentElement.TextValue.Equals("0"))
					{
						state.SpaceExpectedForX = true;
					}
				}
				break;
			case "MonetaryValue":
				if (!contentElement.TextValue.Substring(0, 1).Equals("$"))
				{
					state.SpaceExpected = false;
				}
				state.AddOutputContent(contentElement.TextValue);
				if (contentElement.TextValue.Length == 1)
				{
					state.SpaceExpectedForPlusMinus = true;
				}
				break;
			case "Other":
			case "FunctionKeyword":
				state.AddOutputContent(contentElement.TextValue);
				state.SpaceExpected = true;
				break;
			default:
				throw new Exception("Unrecognized element in SQL Xml!");
			}
		}

		private string FormatKeyword(string keyword)
		{
			if (!KeywordMapping.TryGetValue(keyword.ToUpperInvariant(), out var outputKeyword))
			{
				outputKeyword = keyword;
			}
			if (RandomizeCase)
			{
				return GetCaseRandomized(outputKeyword);
			}
			return outputKeyword;
		}

		private string GetCaseRandomized(string outputKeyword)
		{
			char[] keywordCharArray;
			keywordCharArray = outputKeyword.ToCharArray();
			for (int i = 0; i < keywordCharArray.Length; i++)
			{
				if (_currentCaseLength == _currentCaseLimit)
				{
					_currentCaseLimit = _randomizer.Next(2, 8);
					_currentlyUppercase = _randomizer.Next(0, 2) == 0;
					_currentCaseLength = 0;
				}
				keywordCharArray[i] = (_currentlyUppercase ? keywordCharArray[i].ToUpperInvariant() : keywordCharArray[i].ToLowerInvariant());
				_currentCaseLength++;
			}
			return new string(keywordCharArray);
		}
	}
}
