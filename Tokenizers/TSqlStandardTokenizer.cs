using System;
using System.Text;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib.Tokenizers
{
	public class TSqlStandardTokenizer : ISqlTokenizer
	{
		private class TokenizationState
		{
			public TokenList TokenContainer { get; private set; } = new TokenList();


			public SimplifiedStringReader InputReader { get; private set; }


			public SqlTokenizationType? CurrentTokenizationType { get; set; }


			public StringBuilder CurrentTokenValue { get; set; } = new StringBuilder();


			public int CommentNesting { get; set; }


			public int CurrentCharInt { get; set; } = -1;


			public char CurrentChar
			{
				get
				{
					if (CurrentCharInt < 0)
					{
						throw new InvalidOperationException("No character has been read from the stream");
					}
					if (!HasUnprocessedCurrentCharacter)
					{
						throw new InvalidOperationException("The current character has already been consumed");
					}
					return (char)CurrentCharInt;
				}
			}

			public long? RequestedMarkerPosition { get; private set; }

			public bool HasUnprocessedCurrentCharacter { get; set; }

			public TokenizationState(string inputSQL, long? requestedMarkerPosition)
			{
				if (requestedMarkerPosition > inputSQL.Length)
				{
					throw new ArgumentException("Requested marker position cannot be beyond the end of the input string", "requestedMarkerPosition");
				}
				InputReader = new SimplifiedStringReader(inputSQL);
				RequestedMarkerPosition = requestedMarkerPosition;
			}

			internal void ReadNextCharacter()
			{
				if (HasUnprocessedCurrentCharacter)
				{
					throw new Exception("Unprocessed character detected!");
				}
				CurrentCharInt = InputReader.Read();
				if (CurrentCharInt >= 0)
				{
					HasUnprocessedCurrentCharacter = true;
				}
			}

			internal void ConsumeCurrentCharacterIntoToken()
			{
				if (!HasUnprocessedCurrentCharacter)
				{
					throw new Exception("No current character to consume!");
				}
				CurrentTokenValue.Append(CurrentChar);
				HasUnprocessedCurrentCharacter = false;
			}

			internal void HasRemainingCharacters()
			{
				if (!HasUnprocessedCurrentCharacter)
				{
					throw new Exception("No current character to consume!");
				}
				CurrentTokenValue.Append(CurrentChar);
				HasUnprocessedCurrentCharacter = false;
			}

			internal void DiscardNextCharacter()
			{
				ReadNextCharacter();
				HasUnprocessedCurrentCharacter = false;
			}
		}

		public enum SqlTokenizationType
		{
			WhiteSpace = 0,
			OtherNode = 1,
			SingleLineComment = 2,
			SingleLineCommentCStyle = 3,
			BlockComment = 4,
			String = 5,
			NString = 6,
			QuotedString = 7,
			BracketQuotedName = 8,
			OtherOperator = 9,
			Number = 10,
			BinaryValue = 11,
			MonetaryValue = 12,
			DecimalValue = 13,
			FloatValue = 14,
			PseudoName = 15,
			SingleAsterisk = 16,
			SingleDollar = 17,
			SingleHyphen = 18,
			SingleSlash = 19,
			SingleN = 20,
			SingleLT = 21,
			SingleGT = 22,
			SingleExclamation = 23,
			SinglePeriod = 24,
			SingleZero = 25,
			SinglePipe = 26,
			SingleEquals = 27,
			SingleOtherCompoundableOperator = 28
		}

		public ITokenList TokenizeSQL(string inputSQL)
		{
			return TokenizeSQL(inputSQL, null);
		}

		public ITokenList TokenizeSQL(string inputSQL, long? requestedMarkerPosition)
		{
			TokenizationState state;
			state = new TokenizationState(inputSQL, requestedMarkerPosition);
			state.ReadNextCharacter();
			while (state.HasUnprocessedCurrentCharacter)
			{
				if (!state.CurrentTokenizationType.HasValue)
				{
					ProcessOrOpenToken(state);
					state.ReadNextCharacter();
					continue;
				}
				switch (state.CurrentTokenizationType.Value)
				{
				case SqlTokenizationType.WhiteSpace:
					if (IsWhitespace(state.CurrentChar))
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SinglePeriod:
					if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
						state.CurrentTokenValue.Append('.');
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						state.CurrentTokenValue.Append('.');
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleZero:
					if (state.CurrentChar == 'x' || state.CurrentChar == 'X')
					{
						state.CurrentTokenizationType = SqlTokenizationType.BinaryValue;
						state.CurrentTokenValue.Append('0');
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.CurrentTokenizationType = SqlTokenizationType.Number;
						state.CurrentTokenValue.Append('0');
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar == '.')
					{
						state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
						state.CurrentTokenValue.Append('0');
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						state.CurrentTokenValue.Append('0');
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.Number:
					if (state.CurrentChar == 'e' || state.CurrentChar == 'E')
					{
						state.CurrentTokenizationType = SqlTokenizationType.FloatValue;
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar == '.')
					{
						state.CurrentTokenizationType = SqlTokenizationType.DecimalValue;
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.DecimalValue:
					if (state.CurrentChar == 'e' || state.CurrentChar == 'E')
					{
						state.CurrentTokenizationType = SqlTokenizationType.FloatValue;
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.FloatValue:
					if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if ((state.CurrentChar == '-' || state.CurrentChar == '+') && state.CurrentTokenValue.ToString().ToUpper().EndsWith("E"))
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.BinaryValue:
					if ((state.CurrentChar >= '0' && state.CurrentChar <= '9') || (state.CurrentChar >= 'A' && state.CurrentChar <= 'F') || (state.CurrentChar >= 'a' && state.CurrentChar <= 'f'))
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleDollar:
					state.CurrentTokenValue.Append('$');
					if ((state.CurrentChar >= 'A' && state.CurrentChar <= 'Z') || (state.CurrentChar >= 'a' && state.CurrentChar <= 'z'))
					{
						state.CurrentTokenizationType = SqlTokenizationType.PseudoName;
					}
					else
					{
						state.CurrentTokenizationType = SqlTokenizationType.MonetaryValue;
					}
					state.ConsumeCurrentCharacterIntoToken();
					break;
				case SqlTokenizationType.MonetaryValue:
					if (state.CurrentChar >= '0' && state.CurrentChar <= '9')
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar == '-' && state.CurrentTokenValue.Length == 1)
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else if (state.CurrentChar == '.' && !state.CurrentTokenValue.ToString().Contains("."))
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleHyphen:
					if (state.CurrentChar == '-')
					{
						state.CurrentTokenizationType = SqlTokenizationType.SingleLineComment;
						state.HasUnprocessedCurrentCharacter = false;
					}
					else if (state.CurrentChar == '=')
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						state.CurrentTokenValue.Append('-');
						AppendCharAndCompleteToken(state);
					}
					else
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						state.CurrentTokenValue.Append('-');
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleSlash:
					if (state.CurrentChar == '*')
					{
						state.CurrentTokenizationType = SqlTokenizationType.BlockComment;
						state.HasUnprocessedCurrentCharacter = false;
						state.CommentNesting++;
					}
					else if (state.CurrentChar == '/')
					{
						state.CurrentTokenizationType = SqlTokenizationType.SingleLineCommentCStyle;
						state.HasUnprocessedCurrentCharacter = false;
					}
					else if (state.CurrentChar == '=')
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						state.CurrentTokenValue.Append('/');
						AppendCharAndCompleteToken(state);
					}
					else
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						state.CurrentTokenValue.Append('/');
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleLineComment:
				case SqlTokenizationType.SingleLineCommentCStyle:
					if (state.CurrentChar == '\r' || state.CurrentChar == '\n')
					{
						int nextCharInt;
						nextCharInt = state.InputReader.Peek();
						if (state.CurrentChar == '\r' && nextCharInt == 10)
						{
							state.ConsumeCurrentCharacterIntoToken();
							state.ReadNextCharacter();
						}
						AppendCharAndCompleteToken(state);
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.BlockComment:
					if (state.CurrentChar == '*')
					{
						if (state.InputReader.Peek() == 47)
						{
							state.CommentNesting--;
							if (state.CommentNesting > 0)
							{
								state.ConsumeCurrentCharacterIntoToken();
								state.ReadNextCharacter();
								state.ConsumeCurrentCharacterIntoToken();
							}
							else
							{
								state.HasUnprocessedCurrentCharacter = false;
								state.ReadNextCharacter();
								SwallowOutstandingCharacterAndCompleteToken(state);
							}
						}
						else
						{
							state.ConsumeCurrentCharacterIntoToken();
						}
					}
					else if (state.CurrentChar == '/' && state.InputReader.Peek() == 42)
					{
						state.ConsumeCurrentCharacterIntoToken();
						state.ReadNextCharacter();
						state.ConsumeCurrentCharacterIntoToken();
						state.CommentNesting++;
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.OtherNode:
				case SqlTokenizationType.PseudoName:
					if (IsNonWordCharacter(state.CurrentChar))
					{
						CompleteTokenAndProcessNext(state);
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.SingleN:
					if (state.CurrentChar == '\'')
					{
						state.CurrentTokenizationType = SqlTokenizationType.NString;
						state.HasUnprocessedCurrentCharacter = false;
					}
					else if (IsNonWordCharacter(state.CurrentChar))
					{
						CompleteTokenAndProcessNext(state);
					}
					else
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
						state.CurrentTokenValue.Append('N');
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.String:
				case SqlTokenizationType.NString:
					if (state.CurrentChar == '\'')
					{
						if (state.InputReader.Peek() == 39)
						{
							state.ConsumeCurrentCharacterIntoToken();
							state.DiscardNextCharacter();
						}
						else
						{
							SwallowOutstandingCharacterAndCompleteToken(state);
						}
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.QuotedString:
					if (state.CurrentChar == '"')
					{
						if (state.InputReader.Peek() == 34)
						{
							state.ConsumeCurrentCharacterIntoToken();
							state.DiscardNextCharacter();
						}
						else
						{
							SwallowOutstandingCharacterAndCompleteToken(state);
						}
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.BracketQuotedName:
					if (state.CurrentChar == ']')
					{
						if (state.InputReader.Peek() == 93)
						{
							state.ConsumeCurrentCharacterIntoToken();
							state.DiscardNextCharacter();
						}
						else
						{
							SwallowOutstandingCharacterAndCompleteToken(state);
						}
					}
					else
					{
						state.ConsumeCurrentCharacterIntoToken();
					}
					break;
				case SqlTokenizationType.SingleLT:
					state.CurrentTokenValue.Append('<');
					state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
					if (state.CurrentChar == '=' || state.CurrentChar == '>' || state.CurrentChar == '<')
					{
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleGT:
					state.CurrentTokenValue.Append('>');
					state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
					if (state.CurrentChar == '=' || state.CurrentChar == '>')
					{
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleAsterisk:
					state.CurrentTokenValue.Append('*');
					if (state.CurrentChar == '=')
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleOtherCompoundableOperator:
					state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
					if (state.CurrentChar == '=')
					{
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SinglePipe:
					state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
					state.CurrentTokenValue.Append('|');
					if (state.CurrentChar == '=' || state.CurrentChar == '|')
					{
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleEquals:
					state.CurrentTokenValue.Append('=');
					if (state.CurrentChar == '=')
					{
						AppendCharAndCompleteToken(state);
					}
					else
					{
						CompleteTokenAndProcessNext(state);
					}
					break;
				case SqlTokenizationType.SingleExclamation:
					state.CurrentTokenValue.Append('!');
					if (state.CurrentChar == '=' || state.CurrentChar == '<' || state.CurrentChar == '>')
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherOperator;
						AppendCharAndCompleteToken(state);
					}
					else
					{
						state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
						CompleteTokenAndProcessNext(state);
					}
					break;
				default:
					throw new Exception("In-progress node unrecognized!");
				}
				state.ReadNextCharacter();
			}
			if (state.CurrentTokenizationType.HasValue)
			{
				if (state.CurrentTokenizationType.Value == SqlTokenizationType.BlockComment || state.CurrentTokenizationType.Value == SqlTokenizationType.String || state.CurrentTokenizationType.Value == SqlTokenizationType.NString || state.CurrentTokenizationType.Value == SqlTokenizationType.QuotedString || state.CurrentTokenizationType.Value == SqlTokenizationType.BracketQuotedName)
				{
					state.TokenContainer.HasUnfinishedToken = true;
				}
				SwallowOutstandingCharacterAndCompleteToken(state);
			}
			return state.TokenContainer;
		}

		private static bool IsWhitespace(char targetCharacter)
		{
			return targetCharacter == ' ' || targetCharacter == '\t' || targetCharacter == '\n' || targetCharacter == '\r';
		}

		private static bool IsNonWordCharacter(char currentCharacter)
		{
			return IsWhitespace(currentCharacter) || IsOperatorCharacter(currentCharacter) || (IsCurrencyPrefix(currentCharacter) && currentCharacter != '$') || currentCharacter == '\'' || currentCharacter == '"' || currentCharacter == ',' || currentCharacter == '.' || currentCharacter == '[' || currentCharacter == '(' || currentCharacter == ')' || currentCharacter == '!' || currentCharacter == ';' || currentCharacter == ':';
		}

		private static bool IsCompoundableOperatorCharacter(char currentCharacter)
		{
			return currentCharacter == '/' || currentCharacter == '-' || currentCharacter == '+' || currentCharacter == '*' || currentCharacter == '%' || currentCharacter == '&' || currentCharacter == '^' || currentCharacter == '<' || currentCharacter == '>' || currentCharacter == '|';
		}

		private static bool IsOperatorCharacter(char currentCharacter)
		{
			return currentCharacter == '/' || currentCharacter == '-' || currentCharacter == '+' || currentCharacter == '%' || currentCharacter == '*' || currentCharacter == '&' || currentCharacter == '|' || currentCharacter == '^' || currentCharacter == '=' || currentCharacter == '<' || currentCharacter == '>' || currentCharacter == '~';
		}

		private static bool IsCurrencyPrefix(char currentCharacter)
		{
			return currentCharacter == '$' || currentCharacter == '¢' || currentCharacter == '£' || currentCharacter == '¤' || currentCharacter == '¥' || currentCharacter == '৲' || currentCharacter == '৳' || currentCharacter == '฿' || currentCharacter == '៛' || currentCharacter == '₠' || currentCharacter == '₡' || currentCharacter == '₢' || currentCharacter == '₣' || currentCharacter == '₤' || currentCharacter == '₥' || currentCharacter == '₦' || currentCharacter == '₧' || currentCharacter == '₨' || currentCharacter == '₩' || currentCharacter == '₪' || currentCharacter == '₫' || currentCharacter == '€' || currentCharacter == '₭' || currentCharacter == '₮' || currentCharacter == '₯' || currentCharacter == '₰' || currentCharacter == '₱' || currentCharacter == '﷼' || currentCharacter == '﹩' || currentCharacter == '＄' || currentCharacter == '￠' || currentCharacter == '￡' || currentCharacter == '￥' || currentCharacter == '￦';
		}

		private static void CompleteTokenAndProcessNext(TokenizationState state)
		{
			CompleteToken(state, nextCharRead: true);
			ProcessOrOpenToken(state);
		}

		private static void AppendCharAndCompleteToken(TokenizationState state)
		{
			state.ConsumeCurrentCharacterIntoToken();
			CompleteToken(state, nextCharRead: false);
		}

		private static void SwallowOutstandingCharacterAndCompleteToken(TokenizationState state)
		{
			state.HasUnprocessedCurrentCharacter = false;
			CompleteToken(state, nextCharRead: false);
		}

		private static void ProcessOrOpenToken(TokenizationState state)
		{
			if (state.CurrentTokenizationType.HasValue)
			{
				throw new Exception("Cannot start a new Token: existing Tokenization Type is not null");
			}
			if (!state.HasUnprocessedCurrentCharacter)
			{
				throw new Exception("Cannot start a new Token: no (outstanding) current character specified!");
			}
			state.CurrentTokenValue.Length = 0;
			if (IsWhitespace(state.CurrentChar))
			{
				state.CurrentTokenizationType = SqlTokenizationType.WhiteSpace;
				state.ConsumeCurrentCharacterIntoToken();
			}
			else if (state.CurrentChar == '-')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleHyphen;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '$')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleDollar;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '/')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleSlash;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == 'N')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleN;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '\'')
			{
				state.CurrentTokenizationType = SqlTokenizationType.String;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '"')
			{
				state.CurrentTokenizationType = SqlTokenizationType.QuotedString;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '[')
			{
				state.CurrentTokenizationType = SqlTokenizationType.BracketQuotedName;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '(')
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.OpenParens);
			}
			else if (state.CurrentChar == ')')
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.CloseParens);
			}
			else if (state.CurrentChar == ',')
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.Comma);
			}
			else if (state.CurrentChar == '.')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SinglePeriod;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '0')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleZero;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar >= '1' && state.CurrentChar <= '9')
			{
				state.CurrentTokenizationType = SqlTokenizationType.Number;
				state.ConsumeCurrentCharacterIntoToken();
			}
			else if (IsCurrencyPrefix(state.CurrentChar))
			{
				state.CurrentTokenizationType = SqlTokenizationType.MonetaryValue;
				state.ConsumeCurrentCharacterIntoToken();
			}
			else if (state.CurrentChar == ';')
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.Semicolon);
			}
			else if (state.CurrentChar == ':')
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.Colon);
			}
			else if (state.CurrentChar == '*')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleAsterisk;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '=')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleEquals;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '<')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleLT;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '>')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleGT;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '!')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleExclamation;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (state.CurrentChar == '|')
			{
				state.CurrentTokenizationType = SqlTokenizationType.SinglePipe;
				state.HasUnprocessedCurrentCharacter = false;
			}
			else if (IsCompoundableOperatorCharacter(state.CurrentChar))
			{
				state.CurrentTokenizationType = SqlTokenizationType.SingleOtherCompoundableOperator;
				state.ConsumeCurrentCharacterIntoToken();
			}
			else if (IsOperatorCharacter(state.CurrentChar))
			{
				SaveCurrentCharToNewToken(state, SqlTokenType.OtherOperator);
			}
			else
			{
				state.CurrentTokenizationType = SqlTokenizationType.OtherNode;
				state.ConsumeCurrentCharacterIntoToken();
			}
		}

		private static void CompleteToken(TokenizationState state, bool nextCharRead)
		{
			if (!state.CurrentTokenizationType.HasValue)
			{
				throw new Exception("Cannot complete Token, as there is no current Tokenization Type");
			}
			switch (state.CurrentTokenizationType)
			{
			case SqlTokenizationType.BlockComment:
				SaveToken(state, SqlTokenType.MultiLineComment, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.OtherNode:
				SaveToken(state, SqlTokenType.OtherNode, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.PseudoName:
				SaveToken(state, SqlTokenType.PseudoName, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleLineComment:
				SaveToken(state, SqlTokenType.SingleLineComment, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleLineCommentCStyle:
				SaveToken(state, SqlTokenType.SingleLineCommentCStyle, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleHyphen:
				SaveToken(state, SqlTokenType.OtherOperator, "-");
				break;
			case SqlTokenizationType.SingleDollar:
				SaveToken(state, SqlTokenType.MonetaryValue, "$");
				break;
			case SqlTokenizationType.SingleSlash:
				SaveToken(state, SqlTokenType.OtherOperator, "/");
				break;
			case SqlTokenizationType.WhiteSpace:
				SaveToken(state, SqlTokenType.WhiteSpace, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleN:
				SaveToken(state, SqlTokenType.OtherNode, "N");
				break;
			case SqlTokenizationType.SingleExclamation:
				SaveToken(state, SqlTokenType.OtherNode, "!");
				break;
			case SqlTokenizationType.SinglePipe:
				SaveToken(state, SqlTokenType.OtherNode, "|");
				break;
			case SqlTokenizationType.SingleGT:
				SaveToken(state, SqlTokenType.OtherOperator, ">");
				break;
			case SqlTokenizationType.SingleLT:
				SaveToken(state, SqlTokenType.OtherOperator, "<");
				break;
			case SqlTokenizationType.NString:
				SaveToken(state, SqlTokenType.NationalString, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.String:
				SaveToken(state, SqlTokenType.String, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.QuotedString:
				SaveToken(state, SqlTokenType.QuotedString, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.BracketQuotedName:
				SaveToken(state, SqlTokenType.BracketQuotedName, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.OtherOperator:
			case SqlTokenizationType.SingleOtherCompoundableOperator:
				SaveToken(state, SqlTokenType.OtherOperator, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleZero:
				SaveToken(state, SqlTokenType.Number, "0");
				break;
			case SqlTokenizationType.SinglePeriod:
				SaveToken(state, SqlTokenType.Period, ".");
				break;
			case SqlTokenizationType.SingleAsterisk:
				SaveToken(state, SqlTokenType.Asterisk, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.SingleEquals:
				SaveToken(state, SqlTokenType.EqualsSign, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.Number:
			case SqlTokenizationType.DecimalValue:
			case SqlTokenizationType.FloatValue:
				SaveToken(state, SqlTokenType.Number, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.BinaryValue:
				SaveToken(state, SqlTokenType.BinaryValue, state.CurrentTokenValue.ToString());
				break;
			case SqlTokenizationType.MonetaryValue:
				SaveToken(state, SqlTokenType.MonetaryValue, state.CurrentTokenValue.ToString());
				break;
			default:
				throw new Exception("Unrecognized SQL Node Type");
			}
			state.CurrentTokenizationType = null;
		}

		private static void SaveCurrentCharToNewToken(TokenizationState state, SqlTokenType tokenType)
		{
			char charToSave;
			charToSave = state.CurrentChar;
			state.HasUnprocessedCurrentCharacter = false;
			SaveToken(state, tokenType, charToSave.ToString());
		}

		private static void SaveToken(TokenizationState state, SqlTokenType tokenType, string tokenValue)
		{
			Token foundToken;
			foundToken = new Token(tokenType, tokenValue);
			state.TokenContainer.Add(foundToken);
			long positionOfLastCharacterInToken;
			positionOfLastCharacterInToken = state.InputReader.LastCharacterPosition - (state.HasUnprocessedCurrentCharacter ? 1 : 0);
			if (state.RequestedMarkerPosition.HasValue && state.TokenContainer.MarkerToken == null && state.RequestedMarkerPosition <= positionOfLastCharacterInToken)
			{
				state.TokenContainer.MarkerToken = foundToken;
				long? rawPositionInToken;
				rawPositionInToken = foundToken.Value.Length - (positionOfLastCharacterInToken - state.RequestedMarkerPosition);
				state.TokenContainer.MarkerPosition = ((rawPositionInToken > foundToken.Value.Length) ? foundToken.Value.Length : rawPositionInToken);
			}
		}
	}
}
