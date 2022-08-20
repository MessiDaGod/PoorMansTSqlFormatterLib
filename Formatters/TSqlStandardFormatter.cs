using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
	public class TSqlStandardFormatter : ISqlTreeFormatter
	{
		private class TSqlStandardFormattingState : BaseFormatterState
		{
			private static Regex _startsWithBreakChecker = new Regex("^\\s*(\\r|\\n)", RegexOptions.None);

			private static Regex _lineBreakMatcher = new Regex("(\\r|\\n)+", RegexOptions.Compiled);

			private Dictionary<int, string> _mostRecentKeywordsAtEachLevel = new Dictionary<int, string>();

			private string IndentString { get; set; }

			private int IndentLength { get; set; }

			private int MaxLineWidth { get; set; }

			public bool StatementBreakExpected { get; set; }

			public bool BreakExpected { get; set; }

			public bool WordSeparatorExpected { get; set; }

			public bool SourceBreakPending { get; set; }

			public int AdditionalBreaksExpected { get; set; }

			public bool UnIndentInitialBreak { get; set; }

			public int IndentLevel { get; private set; }

			public int CurrentLineLength { get; private set; }

			public bool CurrentLineHasContent { get; private set; }

			public SpecialRegionType? SpecialRegionActive { get; set; }

			public Node RegionStartNode { get; set; }

			public bool StartsWithBreak => _startsWithBreakChecker.IsMatch(_outBuilder.ToString());

			public bool OutputContainsLineBreak => _lineBreakMatcher.IsMatch(_outBuilder.ToString());

			public TSqlStandardFormattingState(bool htmlOutput, string indentString, int spacesPerTab, int maxLineWidth, int initialIndentLevel)
				: base(htmlOutput)
			{
				IndentLevel = initialIndentLevel;
				HtmlOutput = htmlOutput;
				IndentString = indentString;
				MaxLineWidth = maxLineWidth;
				int tabCount;
				tabCount = indentString.Split('\t').Length - 1;
				int tabExtraCharacters;
				tabExtraCharacters = tabCount * (spacesPerTab - 1);
				IndentLength = indentString.Length + tabExtraCharacters;
			}

			public TSqlStandardFormattingState(TSqlStandardFormattingState sourceState)
				: base(sourceState.HtmlOutput)
			{
				IndentLevel = sourceState.IndentLevel;
				HtmlOutput = sourceState.HtmlOutput;
				IndentString = sourceState.IndentString;
				IndentLength = sourceState.IndentLength;
				MaxLineWidth = sourceState.MaxLineWidth;
				CurrentLineLength = IndentLevel * IndentLength;
				CurrentLineHasContent = sourceState.CurrentLineHasContent;
			}

			public override void AddOutputContent(string content)
			{
				if (!SpecialRegionActive.HasValue)
				{
					AddOutputContent(content, null);
				}
			}

			public override void AddOutputContent(string content, string htmlClassName)
			{
				if (CurrentLineHasContent && content.Length + CurrentLineLength > MaxLineWidth)
				{
					WhiteSpace_BreakToNextLine();
				}
				if (!SpecialRegionActive.HasValue)
				{
					base.AddOutputContent(content, htmlClassName);
				}
				CurrentLineHasContent = true;
				CurrentLineLength += content.Length;
			}

			public override void AddOutputLineBreak()
			{
				if (!SpecialRegionActive.HasValue)
				{
					base.AddOutputLineBreak();
				}
				CurrentLineLength = 0;
				CurrentLineHasContent = false;
			}

			internal void AddOutputSpace()
			{
				if (!SpecialRegionActive.HasValue)
				{
					_outBuilder.Append(" ");
				}
			}

			public void Indent(int indentLevel)
			{
				for (int i = 0; i < indentLevel; i++)
				{
					if (!SpecialRegionActive.HasValue)
					{
						base.AddOutputContent(IndentString, "");
					}
					CurrentLineLength += IndentLength;
				}
			}

			internal void WhiteSpace_BreakToNextLine()
			{
				AddOutputLineBreak();
				Indent(IndentLevel);
				BreakExpected = false;
				SourceBreakPending = false;
				WordSeparatorExpected = false;
			}

			public void Assimilate(TSqlStandardFormattingState partialState)
			{
				CurrentLineLength += partialState.CurrentLineLength;
				CurrentLineHasContent = CurrentLineHasContent || partialState.CurrentLineHasContent;
				if (!SpecialRegionActive.HasValue)
				{
					_outBuilder.Append(partialState.DumpOutput());
				}
			}

			public TSqlStandardFormattingState IncrementIndent()
			{
				IndentLevel++;
				return this;
			}

			public TSqlStandardFormattingState DecrementIndent()
			{
				IndentLevel--;
				return this;
			}

			public void SetRecentKeyword(string ElementName)
			{
				if (!_mostRecentKeywordsAtEachLevel.ContainsKey(IndentLevel))
				{
					_mostRecentKeywordsAtEachLevel.Add(IndentLevel, ElementName.ToUpperInvariant());
				}
			}

			public string GetRecentKeyword()
			{
				string keywordFound;
				keywordFound = null;
				int? keywordFoundAt;
				keywordFoundAt = null;
				foreach (int key in _mostRecentKeywordsAtEachLevel.Keys)
				{
					if ((!keywordFoundAt.HasValue || keywordFoundAt.Value > key) && key >= IndentLevel)
					{
						keywordFoundAt = key;
						keywordFound = _mostRecentKeywordsAtEachLevel[key];
					}
				}
				return keywordFound;
			}

			public void ResetKeywords()
			{
				List<int> descendentLevelKeys;
				descendentLevelKeys = new List<int>();
				foreach (int key2 in _mostRecentKeywordsAtEachLevel.Keys)
				{
					if (key2 >= IndentLevel)
					{
						descendentLevelKeys.Add(key2);
					}
				}
				foreach (int key in descendentLevelKeys)
				{
					_mostRecentKeywordsAtEachLevel.Remove(key);
				}
			}
		}

		public enum SpecialRegionType
		{
			NoFormat = 1,
			Minify = 2
		}

		public IDictionary<string, string> KeywordMapping = new Dictionary<string, string>();

		public TSqlStandardFormatterOptions Options { get; private set; }

		[Obsolete("Use Options.IndentString instead")]
		public string IndentString
		{
			get
			{
				return Options.IndentString;
			}
			set
			{
				Options.IndentString = value;
			}
		}

		[Obsolete("Use Options.SpacesPerTab instead")]
		public int SpacesPerTab
		{
			get
			{
				return Options.SpacesPerTab;
			}
			set
			{
				Options.SpacesPerTab = value;
			}
		}

		[Obsolete("Use Options.MaxLineWidth instead")]
		public int MaxLineWidth
		{
			get
			{
				return Options.MaxLineWidth;
			}
			set
			{
				Options.MaxLineWidth = value;
			}
		}

		[Obsolete("Use Options.ExpandCommaLists instead")]
		public bool ExpandCommaLists
		{
			get
			{
				return Options.ExpandCommaLists;
			}
			set
			{
				Options.ExpandCommaLists = value;
			}
		}

		[Obsolete("Use Options.TrailingCommas instead")]
		public bool TrailingCommas
		{
			get
			{
				return Options.TrailingCommas;
			}
			set
			{
				Options.TrailingCommas = value;
			}
		}

		[Obsolete("Use Options.SpaceAfterExpandedComma instead")]
		public bool SpaceAfterExpandedComma
		{
			get
			{
				return Options.SpaceAfterExpandedComma;
			}
			set
			{
				Options.SpaceAfterExpandedComma = value;
			}
		}

		[Obsolete("Use Options.ExpandBooleanExpressions instead")]
		public bool ExpandBooleanExpressions
		{
			get
			{
				return Options.ExpandBooleanExpressions;
			}
			set
			{
				Options.ExpandBooleanExpressions = value;
			}
		}

		[Obsolete("Use Options.ExpandBetweenConditions instead")]
		public bool ExpandCaseStatements
		{
			get
			{
				return Options.ExpandCaseStatements;
			}
			set
			{
				Options.ExpandCaseStatements = value;
			}
		}

		[Obsolete("Use Options.ExpandCaseStatements instead")]
		public bool ExpandBetweenConditions
		{
			get
			{
				return Options.ExpandBetweenConditions;
			}
			set
			{
				Options.ExpandBetweenConditions = value;
			}
		}

		[Obsolete("Use Options.UppercaseKeywords instead")]
		public bool UppercaseKeywords
		{
			get
			{
				return Options.UppercaseKeywords;
			}
			set
			{
				Options.UppercaseKeywords = value;
			}
		}

		[Obsolete("Use Options.BreakJoinOnSections instead")]
		public bool BreakJoinOnSections
		{
			get
			{
				return Options.BreakJoinOnSections;
			}
			set
			{
				Options.BreakJoinOnSections = value;
			}
		}

		[Obsolete("Use Options.HTMLColoring instead")]
		public bool HTMLColoring
		{
			get
			{
				return Options.HTMLColoring;
			}
			set
			{
				Options.HTMLColoring = value;
			}
		}

		public bool HTMLFormatted => Options.HTMLColoring;

		public string ErrorOutputPrefix { get; set; }

		public TSqlStandardFormatter()
			: this(new TSqlStandardFormatterOptions())
		{
		}

		public TSqlStandardFormatter(TSqlStandardFormatterOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			Options = options;
			if (options.KeywordStandardization)
			{
				KeywordMapping = StandardKeywordRemapping.Instance;
			}
			ErrorOutputPrefix = "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!" + Environment.NewLine;
		}

		[Obsolete("Use the constructor with the TSqlStandardFormatterOptions parameter")]
		public TSqlStandardFormatter(string indentString, int spacesPerTab, int maxLineWidth, bool expandCommaLists, bool trailingCommas, bool spaceAfterExpandedComma, bool expandBooleanExpressions, bool expandCaseStatements, bool expandBetweenConditions, bool breakJoinOnSections, bool uppercaseKeywords, bool htmlColoring, bool keywordStandardization)
		{
			Options = new TSqlStandardFormatterOptions
			{
				IndentString = indentString,
				SpacesPerTab = spacesPerTab,
				MaxLineWidth = maxLineWidth,
				ExpandCommaLists = expandCommaLists,
				TrailingCommas = trailingCommas,
				SpaceAfterExpandedComma = spaceAfterExpandedComma,
				ExpandBooleanExpressions = expandBooleanExpressions,
				ExpandBetweenConditions = expandBetweenConditions,
				ExpandCaseStatements = expandCaseStatements,
				UppercaseKeywords = uppercaseKeywords,
				BreakJoinOnSections = breakJoinOnSections,
				HTMLColoring = htmlColoring,
				KeywordStandardization = keywordStandardization
			};
			if (keywordStandardization)
			{
				KeywordMapping = StandardKeywordRemapping.Instance;
			}
			ErrorOutputPrefix = "--WARNING! ERRORS ENCOUNTERED DURING SQL PARSING!" + Environment.NewLine;
		}

		public string FormatSQLTree(Node sqlTreeDoc)
		{
			TSqlStandardFormattingState state;
			state = new TSqlStandardFormattingState(Options.HTMLColoring, Options.IndentString, Options.SpacesPerTab, Options.MaxLineWidth, 0);
			if (sqlTreeDoc.Name == "SqlRoot" && sqlTreeDoc.GetAttributeValue("errorFound") == "1")
			{
				state.AddOutputContent(ErrorOutputPrefix);
			}
			ProcessSqlNodeList(sqlTreeDoc.Children, state);
			WhiteSpace_BreakAsExpected(state);
			if (state.SpecialRegionActive == SpecialRegionType.NoFormat)
			{
				Node skippedXml2;
				skippedXml2 = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, sqlTreeDoc);
				TSqlIdentityFormatter tempFormatter2;
				tempFormatter2 = new TSqlIdentityFormatter(Options.HTMLColoring);
				state.AddOutputContentRaw(tempFormatter2.FormatSQLTree(skippedXml2));
			}
			else if (state.SpecialRegionActive == SpecialRegionType.Minify)
			{
				Node skippedXml;
				skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, sqlTreeDoc);
				TSqlObfuscatingFormatter tempFormatter;
				tempFormatter = new TSqlObfuscatingFormatter();
				if (HTMLFormatted)
				{
					state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter.FormatSQLTree(skippedXml)));
				}
				else
				{
					state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
				}
			}
			return state.DumpOutput();
		}

		private void ProcessSqlNodeList(IEnumerable<Node> rootList, TSqlStandardFormattingState state)
		{
			foreach (Node contentElement in rootList)
			{
				ProcessSqlNode(contentElement, state);
			}
		}

		private void ProcessSqlNode(Node contentElement, TSqlStandardFormattingState state)
		{
			int initialIndent;
			initialIndent = state.IndentLevel;
			if (contentElement.GetAttributeValue("hasError") == "1")
			{
				state.OpenClass("SQLErrorHighlight");
			}
			switch (contentElement.Name)
			{
			case "SqlStatement":
				WhiteSpace_SeparateStatements(contentElement, state);
				state.ResetKeywords();
				ProcessSqlNodeList(contentElement.Children, state);
				state.StatementBreakExpected = true;
				break;
			case "Clause":
				state.UnIndentInitialBreak = true;
				ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
				state.DecrementIndent();
				if (Options.NewClauseLineBreaks > 0)
				{
					state.BreakExpected = true;
				}
				if (Options.NewClauseLineBreaks > 1)
				{
					state.AdditionalBreaksExpected = Options.NewClauseLineBreaks - 1;
				}
				break;
			case "SetOperatorClause":
				state.DecrementIndent();
				state.WhiteSpace_BreakToNextLine();
				state.WhiteSpace_BreakToNextLine();
				ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
				state.BreakExpected = true;
				state.AdditionalBreaksExpected = 1;
				break;
			case "BatchSeparator":
				state.WhiteSpace_BreakToNextLine();
				ProcessSqlNodeList(contentElement.Children, state);
				state.BreakExpected = true;
				break;
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
				ProcessSqlNodeList(contentElement.Children, state);
				break;
			case "Input":
			case "BooleanExpression":
			case "LowerBound":
			case "UpperBound":
				WhiteSpace_SeparateWords(state);
				ProcessSqlNodeList(contentElement.Children, state);
				break;
			case "ContainerSingleStatementBody":
			case "ContainerMultiStatementBody":
			case "MergeAction":
			{
				bool singleStatementIsIf;
				singleStatementIsIf = false;
				foreach (Node statement in contentElement.ChildrenByName("SqlStatement"))
				{
					foreach (Node clause in statement.ChildrenByName("Clause"))
					{
						foreach (Node ifStatement in clause.ChildrenByName("IfStatement"))
						{
							singleStatementIsIf = true;
						}
					}
				}
				if (singleStatementIsIf && contentElement.Parent.Name.Equals("ElseClause"))
				{
					state.DecrementIndent();
				}
				else
				{
					state.BreakExpected = true;
				}
				ProcessSqlNodeList(contentElement.Children, state);
				if (singleStatementIsIf && contentElement.Parent.Name.Equals("ElseClause"))
				{
					state.IncrementIndent();
				}
				state.StatementBreakExpected = false;
				state.UnIndentInitialBreak = false;
				break;
			}
			case "PermissionsTarget":
			case "PermissionsRecipient":
			case "DDLWith":
			case "MergeCondition":
			case "MergeThen":
				state.BreakExpected = true;
				state.UnIndentInitialBreak = true;
				ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
				state.DecrementIndent();
				break;
			case "JoinOn":
				if (Options.BreakJoinOnSections)
				{
					state.BreakExpected = true;
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state);
				if (Options.BreakJoinOnSections)
				{
					state.IncrementIndent();
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerContentBody"), state);
				if (Options.BreakJoinOnSections)
				{
					state.DecrementIndent();
				}
				break;
			case "CTEAlias":
				state.UnIndentInitialBreak = true;
				ProcessSqlNodeList(contentElement.Children, state);
				break;
			case "ElseClause":
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state.DecrementIndent());
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerSingleStatementBody"), state.IncrementIndent());
				break;
			case "DDLAsBlock":
			case "CursorForBlock":
				state.BreakExpected = true;
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state.DecrementIndent());
				state.BreakExpected = true;
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerContentBody"), state);
				state.IncrementIndent();
				break;
			case "TriggerCondition":
				state.DecrementIndent();
				state.WhiteSpace_BreakToNextLine();
				ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
				break;
			case "CursorForOptions":
			case "CTEAsBlock":
				state.BreakExpected = true;
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state.DecrementIndent());
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerContentBody"), state.IncrementIndent());
				break;
			case "DDLReturns":
			case "MergeUsing":
			case "MergeWhen":
				state.BreakExpected = true;
				state.UnIndentInitialBreak = true;
				ProcessSqlNodeList(contentElement.Children, state);
				break;
			case "Between":
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state);
				state.IncrementIndent();
				ProcessSqlNodeList(contentElement.ChildrenByName("LowerBound"), state.IncrementIndent());
				if (Options.ExpandBetweenConditions)
				{
					state.BreakExpected = true;
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerClose"), state.DecrementIndent());
				ProcessSqlNodeList(contentElement.ChildrenByName("UpperBound"), state.IncrementIndent());
				state.DecrementIndent();
				state.DecrementIndent();
				break;
			case "DDLDetailParens":
			case "FunctionParens":
				state.WordSeparatorExpected = false;
				WhiteSpace_BreakAsExpected(state);
				state.AddOutputContent(FormatOperator("("), "SQLOperator");
				ProcessSqlNodeList(contentElement.Children, state.IncrementIndent());
				state.DecrementIndent();
				WhiteSpace_BreakAsExpected(state);
				state.AddOutputContent(FormatOperator(")"), "SQLOperator");
				state.WordSeparatorExpected = true;
				break;
			case "DDLParens":
			case "ExpressionParens":
			case "SelectionTargetParens":
			case "InParens":
			{
				WhiteSpace_SeparateWords(state);
				if (contentElement.Name.Equals("ExpressionParens") || contentElement.Name.Equals("InParens"))
				{
					state.IncrementIndent();
				}
				state.AddOutputContent(FormatOperator("("), "SQLOperator");
				TSqlStandardFormattingState innerState;
				innerState = new TSqlStandardFormattingState(state);
				ProcessSqlNodeList(contentElement.Children, innerState);
				if (innerState.BreakExpected || innerState.OutputContainsLineBreak)
				{
					if (!innerState.StartsWithBreak)
					{
						state.WhiteSpace_BreakToNextLine();
					}
					state.Assimilate(innerState);
					state.WhiteSpace_BreakToNextLine();
				}
				else
				{
					state.Assimilate(innerState);
				}
				state.AddOutputContent(FormatOperator(")"), "SQLOperator");
				if (contentElement.Name.Equals("ExpressionParens") || contentElement.Name.Equals("InParens"))
				{
					state.DecrementIndent();
				}
				state.WordSeparatorExpected = true;
				break;
			}
			case "BeginEndBlock":
			case "TryBlock":
			case "CatchBlock":
				if (contentElement.Parent.Name.Equals("Clause") && contentElement.Parent.Parent.Name.Equals("SqlStatement") && contentElement.Parent.Parent.Parent.Name.Equals("ContainerSingleStatementBody"))
				{
					state.DecrementIndent();
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state);
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerMultiStatementBody"), state);
				state.DecrementIndent();
				state.BreakExpected = true;
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerClose"), state);
				state.IncrementIndent();
				if (contentElement.Parent.Name.Equals("Clause") && contentElement.Parent.Parent.Name.Equals("SqlStatement") && contentElement.Parent.Parent.Parent.Name.Equals("ContainerSingleStatementBody"))
				{
					state.IncrementIndent();
				}
				break;
			case "CaseStatement":
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state);
				state.IncrementIndent();
				ProcessSqlNodeList(contentElement.ChildrenByName("Input"), state);
				ProcessSqlNodeList(contentElement.ChildrenByName("When"), state);
				ProcessSqlNodeList(contentElement.ChildrenByName("CaseElse"), state);
				if (Options.ExpandCaseStatements)
				{
					state.BreakExpected = true;
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerClose"), state);
				state.DecrementIndent();
				break;
			case "When":
			case "Then":
			case "CaseElse":
				if (Options.ExpandCaseStatements)
				{
					state.BreakExpected = true;
				}
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerOpen"), state);
				ProcessSqlNodeList(contentElement.ChildrenByName("ContainerContentBody"), state.IncrementIndent());
				ProcessSqlNodeList(contentElement.ChildrenByName("Then"), state);
				state.DecrementIndent();
				break;
			case "And":
			case "Or":
				if (Options.ExpandBooleanExpressions)
				{
					state.BreakExpected = true;
				}
				ProcessSqlNode(contentElement.ChildByName("OtherKeyword"), state);
				break;
			case "MultiLineComment":
				if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.TextValue.ToUpperInvariant().Contains("[/NOFORMAT]"))
				{
					Node skippedXml3;
					skippedXml3 = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
					if (skippedXml3 != null)
					{
						TSqlIdentityFormatter tempFormatter3;
						tempFormatter3 = new TSqlIdentityFormatter(Options.HTMLColoring);
						state.AddOutputContentRaw(tempFormatter3.FormatSQLTree(skippedXml3));
						state.WordSeparatorExpected = false;
						state.BreakExpected = false;
					}
					state.SpecialRegionActive = null;
					state.RegionStartNode = null;
				}
				else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.TextValue.ToUpperInvariant().Contains("[/MINIFY]"))
				{
					Node skippedXml4;
					skippedXml4 = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
					if (skippedXml4 != null)
					{
						TSqlObfuscatingFormatter tempFormatter4;
						tempFormatter4 = new TSqlObfuscatingFormatter();
						if (HTMLFormatted)
						{
							state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter4.FormatSQLTree(skippedXml4)));
						}
						else
						{
							state.AddOutputContentRaw(tempFormatter4.FormatSQLTree(skippedXml4));
						}
						state.WordSeparatorExpected = false;
						state.BreakExpected = false;
					}
					state.SpecialRegionActive = null;
					state.RegionStartNode = null;
				}
				WhiteSpace_SeparateComment(contentElement, state);
				state.AddOutputContent("/*" + contentElement.TextValue + "*/", "SQLComment");
				if (contentElement.Parent.Name.Equals("SqlStatement") || (contentElement.NextSibling() != null && contentElement.NextSibling().Name.Equals("WhiteSpace") && Regex.IsMatch(contentElement.NextSibling().TextValue, "(\\r|\\n)+")))
				{
					state.BreakExpected = true;
				}
				else
				{
					state.WordSeparatorExpected = true;
				}
				if (!state.SpecialRegionActive.HasValue && contentElement.TextValue.ToUpperInvariant().Contains("[NOFORMAT]"))
				{
					state.SpecialRegionActive = SpecialRegionType.NoFormat;
					state.RegionStartNode = contentElement;
				}
				else if (!state.SpecialRegionActive.HasValue && contentElement.TextValue.ToUpperInvariant().Contains("[MINIFY]"))
				{
					state.SpecialRegionActive = SpecialRegionType.Minify;
					state.RegionStartNode = contentElement;
				}
				break;
			case "SingleLineComment":
			case "SingleLineCommentCStyle":
				if (state.SpecialRegionActive == SpecialRegionType.NoFormat && contentElement.TextValue.ToUpperInvariant().Contains("[/NOFORMAT]"))
				{
					Node skippedXml;
					skippedXml = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
					if (skippedXml != null)
					{
						TSqlIdentityFormatter tempFormatter;
						tempFormatter = new TSqlIdentityFormatter(Options.HTMLColoring);
						state.AddOutputContentRaw(tempFormatter.FormatSQLTree(skippedXml));
						state.WordSeparatorExpected = false;
						state.BreakExpected = false;
					}
					state.SpecialRegionActive = null;
					state.RegionStartNode = null;
				}
				else if (state.SpecialRegionActive == SpecialRegionType.Minify && contentElement.TextValue.ToUpperInvariant().Contains("[/MINIFY]"))
				{
					Node skippedXml2;
					skippedXml2 = NodeExtensions.ExtractStructureBetween(state.RegionStartNode, contentElement);
					if (skippedXml2 != null)
					{
						TSqlObfuscatingFormatter tempFormatter2;
						tempFormatter2 = new TSqlObfuscatingFormatter();
						if (HTMLFormatted)
						{
							state.AddOutputContentRaw(Utils.HtmlEncode(tempFormatter2.FormatSQLTree(skippedXml2)));
						}
						else
						{
							state.AddOutputContentRaw(tempFormatter2.FormatSQLTree(skippedXml2));
						}
						state.WordSeparatorExpected = false;
						state.BreakExpected = false;
					}
					state.SpecialRegionActive = null;
					state.RegionStartNode = null;
				}
				WhiteSpace_SeparateComment(contentElement, state);
				state.AddOutputContent(((contentElement.Name == "SingleLineComment") ? "--" : "//") + contentElement.TextValue.Replace("\r", "").Replace("\n", ""), "SQLComment");
				state.BreakExpected = true;
				state.SourceBreakPending = true;
				if (!state.SpecialRegionActive.HasValue && contentElement.TextValue.ToUpperInvariant().Contains("[NOFORMAT]"))
				{
					state.AddOutputLineBreak();
					state.SpecialRegionActive = SpecialRegionType.NoFormat;
					state.RegionStartNode = contentElement;
				}
				else if (!state.SpecialRegionActive.HasValue && contentElement.TextValue.ToUpperInvariant().Contains("[MINIFY]"))
				{
					state.AddOutputLineBreak();
					state.SpecialRegionActive = SpecialRegionType.Minify;
					state.RegionStartNode = contentElement;
				}
				break;
			case "String":
			case "NationalString":
			{
				WhiteSpace_SeparateWords(state);
				string outValue;
				outValue = null;
				outValue = ((!contentElement.Name.Equals("NationalString")) ? ("'" + contentElement.TextValue.Replace("'", "''") + "'") : ("N'" + contentElement.TextValue.Replace("'", "''") + "'"));
				state.AddOutputContent(outValue, "SQLString");
				state.WordSeparatorExpected = true;
				break;
			}
			case "BracketQuotedName":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent("[" + contentElement.TextValue.Replace("]", "]]") + "]");
				state.WordSeparatorExpected = true;
				break;
			case "QuotedString":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent("\"" + contentElement.TextValue.Replace("\"", "\"\"") + "\"");
				state.WordSeparatorExpected = true;
				break;
			case "Comma":
				if (Options.TrailingCommas)
				{
					WhiteSpace_BreakAsExpected(state);
					state.AddOutputContent(FormatOperator(","), "SQLOperator");
					if ((Options.ExpandCommaLists && !contentElement.Parent.Name.Equals("DDLDetailParens") && !contentElement.Parent.Name.Equals("FunctionParens") && !contentElement.Parent.Name.Equals("InParens")) || (Options.ExpandInLists && contentElement.Parent.Name.Equals("InParens")))
					{
						state.BreakExpected = true;
					}
					else
					{
						state.WordSeparatorExpected = true;
					}
				}
				else if ((Options.ExpandCommaLists && !contentElement.Parent.Name.Equals("DDLDetailParens") && !contentElement.Parent.Name.Equals("FunctionParens") && !contentElement.Parent.Name.Equals("InParens")) || (Options.ExpandInLists && contentElement.Parent.Name.Equals("InParens")))
				{
					state.WhiteSpace_BreakToNextLine();
					state.AddOutputContent(FormatOperator(","), "SQLOperator");
					if (Options.SpaceAfterExpandedComma)
					{
						state.WordSeparatorExpected = true;
					}
				}
				else
				{
					WhiteSpace_BreakAsExpected(state);
					state.AddOutputContent(FormatOperator(","), "SQLOperator");
					state.WordSeparatorExpected = true;
				}
				break;
			case "Period":
			case "Semicolon":
			case "ScopeResolutionOperator":
				state.WordSeparatorExpected = false;
				WhiteSpace_BreakAsExpected(state);
				state.AddOutputContent(FormatOperator(contentElement.TextValue), "SQLOperator");
				break;
			case "Asterisk":
			case "EqualsSign":
			case "AlphaOperator":
			case "OtherOperator":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent(FormatOperator(contentElement.TextValue), "SQLOperator");
				state.WordSeparatorExpected = true;
				break;
			case "CompoundKeyword":
				WhiteSpace_SeparateWords(state);
				state.SetRecentKeyword(contentElement.GetAttributeValue("simpleText"));
				state.AddOutputContent(FormatKeyword(contentElement.GetAttributeValue("simpleText")), "SQLKeyword");
				state.WordSeparatorExpected = true;
				ProcessSqlNodeList(contentElement.ChildrenByNames(SqlStructureConstants.ENAMELIST_COMMENT), state.IncrementIndent());
				state.DecrementIndent();
				state.WordSeparatorExpected = true;
				break;
			case "OtherKeyword":
			case "DataTypeKeyword":
				WhiteSpace_SeparateWords(state);
				state.SetRecentKeyword(contentElement.TextValue);
				state.AddOutputContent(FormatKeyword(contentElement.TextValue), "SQLKeyword");
				state.WordSeparatorExpected = true;
				break;
			case "PseudoName":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent(FormatKeyword(contentElement.TextValue), "SQLKeyword");
				state.WordSeparatorExpected = true;
				break;
			case "FunctionKeyword":
				WhiteSpace_SeparateWords(state);
				state.SetRecentKeyword(contentElement.TextValue);
				state.AddOutputContent(contentElement.TextValue, "SQLFunction");
				state.WordSeparatorExpected = true;
				break;
			case "Other":
			case "MonetaryValue":
			case "Label":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent(contentElement.TextValue);
				state.WordSeparatorExpected = true;
				break;
			case "NumberValue":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent(contentElement.TextValue.ToLowerInvariant());
				state.WordSeparatorExpected = true;
				break;
			case "BinaryValue":
				WhiteSpace_SeparateWords(state);
				state.AddOutputContent("0x");
				state.AddOutputContent(contentElement.TextValue.Substring(2).ToUpperInvariant());
				state.WordSeparatorExpected = true;
				break;
			case "WhiteSpace":
				if (Regex.IsMatch(contentElement.TextValue, "(\\r|\\n)+"))
				{
					state.SourceBreakPending = true;
				}
				break;
			default:
				throw new Exception("Unrecognized element in SQL Xml!");
			}
			if (contentElement.GetAttributeValue("hasError") == "1")
			{
				state.CloseClass();
			}
			if (initialIndent != state.IndentLevel)
			{
				throw new Exception("Messed up the indenting!! Check code/stack or panic!");
			}
		}

		private string FormatKeyword(string keyword)
		{
			if (!KeywordMapping.TryGetValue(keyword.ToUpperInvariant(), out var outputKeyword))
			{
				outputKeyword = keyword;
			}
			if (Options.UppercaseKeywords)
			{
				return outputKeyword.ToUpperInvariant();
			}
			return outputKeyword.ToLowerInvariant();
		}

		private string FormatOperator(string operatorValue)
		{
			if (Options.UppercaseKeywords)
			{
				return operatorValue.ToUpperInvariant();
			}
			return operatorValue.ToLowerInvariant();
		}

		private void WhiteSpace_SeparateStatements(Node contentElement, TSqlStandardFormattingState state)
		{
			if (!state.StatementBreakExpected)
			{
				return;
			}
			Node thisClauseStarter;
			thisClauseStarter = FirstSemanticElementChild(contentElement);
			if (thisClauseStarter == null || !thisClauseStarter.Name.Equals("OtherKeyword") || state.GetRecentKeyword() == null || ((!thisClauseStarter.TextValue.ToUpperInvariant().Equals("SET") || !state.GetRecentKeyword().Equals("SET")) && (!thisClauseStarter.TextValue.ToUpperInvariant().Equals("DECLARE") || !state.GetRecentKeyword().Equals("DECLARE")) && (!thisClauseStarter.TextValue.ToUpperInvariant().Equals("PRINT") || !state.GetRecentKeyword().Equals("PRINT"))))
			{
				for (int i = Options.NewStatementLineBreaks; i > 0; i--)
				{
					state.AddOutputLineBreak();
				}
			}
			else
			{
				for (int j = Options.NewClauseLineBreaks; j > 0; j--)
				{
					state.AddOutputLineBreak();
				}
			}
			state.Indent(state.IndentLevel);
			state.BreakExpected = false;
			state.AdditionalBreaksExpected = 0;
			state.SourceBreakPending = false;
			state.StatementBreakExpected = false;
			state.WordSeparatorExpected = false;
		}

		private Node FirstSemanticElementChild(Node contentElement)
		{
			Node target;
			target = null;
			while (contentElement != null)
			{
				target = contentElement.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).FirstOrDefault();
				contentElement = ((target == null || !SqlStructureConstants.ENAMELIST_NONSEMANTICCONTENT.Contains(target.Name)) ? null : target);
			}
			return target;
		}

		private void WhiteSpace_SeparateWords(TSqlStandardFormattingState state)
		{
			if (state.BreakExpected || state.AdditionalBreaksExpected > 0)
			{
				bool wasUnIndent;
				wasUnIndent = state.UnIndentInitialBreak;
				if (wasUnIndent)
				{
					state.DecrementIndent();
				}
				WhiteSpace_BreakAsExpected(state);
				if (wasUnIndent)
				{
					state.IncrementIndent();
				}
			}
			else if (state.WordSeparatorExpected)
			{
				state.AddOutputSpace();
			}
			state.UnIndentInitialBreak = false;
			state.SourceBreakPending = false;
			state.WordSeparatorExpected = false;
		}

		private void WhiteSpace_SeparateComment(Node contentElement, TSqlStandardFormattingState state)
		{
			if (state.CurrentLineHasContent && state.SourceBreakPending)
			{
				state.BreakExpected = true;
				WhiteSpace_BreakAsExpected(state);
			}
			else if (state.WordSeparatorExpected)
			{
				state.AddOutputSpace();
			}
			state.SourceBreakPending = false;
			state.WordSeparatorExpected = false;
		}

		private void WhiteSpace_BreakAsExpected(TSqlStandardFormattingState state)
		{
			if (state.BreakExpected)
			{
				state.WhiteSpace_BreakToNextLine();
			}
			while (state.AdditionalBreaksExpected > 0)
			{
				state.WhiteSpace_BreakToNextLine();
				state.AdditionalBreaksExpected--;
			}
		}
	}
}
