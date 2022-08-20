using System;
using System.Collections.Generic;
using System.Linq;

namespace PoorMansTSqlFormatterLib.Formatters
{
	public class TSqlStandardFormatterOptions
	{
		private static readonly TSqlStandardFormatterOptions _defaultOptions = new TSqlStandardFormatterOptions();

		private string _indentString;

		public string IndentString
		{
			get
			{
				return _indentString;
			}
			set
			{
				_indentString = value.Replace("\\t", "\t").Replace("\\s", " ");
			}
		}

		public int SpacesPerTab { get; set; }

		public int MaxLineWidth { get; set; }

		public bool ExpandCommaLists { get; set; }

		public bool TrailingCommas { get; set; }

		public bool SpaceAfterExpandedComma { get; set; }

		public bool ExpandBooleanExpressions { get; set; }

		public bool ExpandCaseStatements { get; set; }

		public bool ExpandBetweenConditions { get; set; }

		public bool UppercaseKeywords { get; set; }

		public bool BreakJoinOnSections { get; set; }

		public bool HTMLColoring { get; set; }

		public bool KeywordStandardization { get; set; }

		public bool ExpandInLists { get; set; }

		public int NewClauseLineBreaks { get; set; }

		public int NewStatementLineBreaks { get; set; }

		public TSqlStandardFormatterOptions()
		{
			IndentString = "\t";
			SpacesPerTab = 4;
			MaxLineWidth = 999;
			ExpandCommaLists = true;
			TrailingCommas = false;
			SpaceAfterExpandedComma = false;
			ExpandBooleanExpressions = true;
			ExpandBetweenConditions = true;
			ExpandCaseStatements = true;
			UppercaseKeywords = true;
			BreakJoinOnSections = false;
			HTMLColoring = false;
			KeywordStandardization = false;
			ExpandInLists = true;
			NewClauseLineBreaks = 1;
			NewStatementLineBreaks = 2;
		}

		public TSqlStandardFormatterOptions(string serializedString)
			: this()
		{
			if (string.IsNullOrEmpty(serializedString))
			{
				return;
			}
			string[] array;
			array = serializedString.Split(',');
			foreach (string kvp in array)
			{
				string[] splitPair;
				splitPair = kvp.Split('=');
				string key;
				key = splitPair[0];
				string value;
				value = splitPair[1];
				switch (key)
				{
				case "IndentString":
					IndentString = value;
					break;
				case "SpacesPerTab":
					SpacesPerTab = Convert.ToInt32(value);
					break;
				case "MaxLineWidth":
					MaxLineWidth = Convert.ToInt32(value);
					break;
				case "ExpandCommaLists":
					ExpandCommaLists = Convert.ToBoolean(value);
					break;
				case "TrailingCommas":
					TrailingCommas = Convert.ToBoolean(value);
					break;
				case "SpaceAfterExpandedComma":
					SpaceAfterExpandedComma = Convert.ToBoolean(value);
					break;
				case "ExpandBooleanExpressions":
					ExpandBooleanExpressions = Convert.ToBoolean(value);
					break;
				case "ExpandBetweenConditions":
					ExpandBetweenConditions = Convert.ToBoolean(value);
					break;
				case "ExpandCaseStatements":
					ExpandCaseStatements = Convert.ToBoolean(value);
					break;
				case "UppercaseKeywords":
					UppercaseKeywords = Convert.ToBoolean(value);
					break;
				case "BreakJoinOnSections":
					BreakJoinOnSections = Convert.ToBoolean(value);
					break;
				case "HTMLColoring":
					HTMLColoring = Convert.ToBoolean(value);
					break;
				case "KeywordStandardization":
					KeywordStandardization = Convert.ToBoolean(value);
					break;
				case "ExpandInLists":
					ExpandInLists = Convert.ToBoolean(value);
					break;
				case "NewClauseLineBreaks":
					NewClauseLineBreaks = Convert.ToInt32(value);
					break;
				case "NewStatementLineBreaks":
					NewStatementLineBreaks = Convert.ToInt32(value);
					break;
				default:
					throw new ArgumentException("Unknown option: " + key);
				}
			}
		}

		public string ToSerializedString()
		{
			Dictionary<string, string> overrides;
			overrides = new Dictionary<string, string>();
			if (IndentString != _defaultOptions.IndentString)
			{
				overrides.Add("IndentString", IndentString);
			}
			if (SpacesPerTab != _defaultOptions.SpacesPerTab)
			{
				overrides.Add("SpacesPerTab", SpacesPerTab.ToString());
			}
			if (MaxLineWidth != _defaultOptions.MaxLineWidth)
			{
				overrides.Add("MaxLineWidth", MaxLineWidth.ToString());
			}
			if (ExpandCommaLists != _defaultOptions.ExpandCommaLists)
			{
				overrides.Add("ExpandCommaLists", ExpandCommaLists.ToString());
			}
			if (TrailingCommas != _defaultOptions.TrailingCommas)
			{
				overrides.Add("TrailingCommas", TrailingCommas.ToString());
			}
			if (SpaceAfterExpandedComma != _defaultOptions.SpaceAfterExpandedComma)
			{
				overrides.Add("SpaceAfterExpandedComma", SpaceAfterExpandedComma.ToString());
			}
			if (ExpandBooleanExpressions != _defaultOptions.ExpandBooleanExpressions)
			{
				overrides.Add("ExpandBooleanExpressions", ExpandBooleanExpressions.ToString());
			}
			if (ExpandBetweenConditions != _defaultOptions.ExpandBetweenConditions)
			{
				overrides.Add("ExpandBetweenConditions", ExpandBetweenConditions.ToString());
			}
			if (ExpandCaseStatements != _defaultOptions.ExpandCaseStatements)
			{
				overrides.Add("ExpandCaseStatements", ExpandCaseStatements.ToString());
			}
			if (UppercaseKeywords != _defaultOptions.UppercaseKeywords)
			{
				overrides.Add("UppercaseKeywords", UppercaseKeywords.ToString());
			}
			if (BreakJoinOnSections != _defaultOptions.BreakJoinOnSections)
			{
				overrides.Add("BreakJoinOnSections", BreakJoinOnSections.ToString());
			}
			if (HTMLColoring != _defaultOptions.HTMLColoring)
			{
				overrides.Add("HTMLColoring", HTMLColoring.ToString());
			}
			if (KeywordStandardization != _defaultOptions.KeywordStandardization)
			{
				overrides.Add("KeywordStandardization", KeywordStandardization.ToString());
			}
			if (ExpandInLists != _defaultOptions.ExpandInLists)
			{
				overrides.Add("ExpandInLists", ExpandInLists.ToString());
			}
			if (NewClauseLineBreaks != _defaultOptions.NewClauseLineBreaks)
			{
				overrides.Add("NewClauseLineBreaks", NewClauseLineBreaks.ToString());
			}
			if (NewStatementLineBreaks != _defaultOptions.NewStatementLineBreaks)
			{
				overrides.Add("NewStatementLineBreaks", NewStatementLineBreaks.ToString());
			}
			NewStatementLineBreaks = 2;
			if (overrides.Count == 0)
			{
				return string.Empty;
			}
			return string.Join(",", overrides.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());
		}
	}
}
