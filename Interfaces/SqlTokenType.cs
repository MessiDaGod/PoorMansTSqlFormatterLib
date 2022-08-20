namespace PoorMansTSqlFormatterLib.Interfaces
{
	public enum SqlTokenType
	{
		OpenParens = 0,
		CloseParens = 1,
		WhiteSpace = 2,
		OtherNode = 3,
		SingleLineComment = 4,
		SingleLineCommentCStyle = 5,
		MultiLineComment = 6,
		String = 7,
		NationalString = 8,
		BracketQuotedName = 9,
		QuotedString = 10,
		Comma = 11,
		Period = 12,
		Semicolon = 13,
		Colon = 14,
		Asterisk = 15,
		EqualsSign = 16,
		MonetaryValue = 17,
		Number = 18,
		BinaryValue = 19,
		OtherOperator = 20,
		PseudoName = 21
	}
}
