using System.Collections;
using System.Collections.Generic;
using System.Text;
using PoorMansTSqlFormatterLib.Interfaces;

namespace PoorMansTSqlFormatterLib
{
	public class TokenList : List<IToken>, ITokenList, IList<IToken>, ICollection<IToken>, IEnumerable<IToken>, IEnumerable
	{
		public bool HasUnfinishedToken { get; set; }

		public IToken MarkerToken { get; set; }

		public long? MarkerPosition { get; set; }

		public string PrettyPrint()
		{
			StringBuilder outString;
			outString = new StringBuilder();
			foreach (IToken contentToken in this)
			{
				string tokenType;
				tokenType = contentToken.Type.ToString();
				outString.Append(tokenType.PadRight(20));
				outString.Append(": ");
				outString.Append(contentToken.Value);
				if (contentToken.Equals(MarkerToken))
				{
					outString.Append(" (MARKER - pos ");
					outString.Append(MarkerPosition.ToString());
					outString.Append(")");
				}
				outString.AppendLine();
			}
			return outString.ToString();
		}

		public new IList<IToken> GetRange(int index, int count)
		{
			return base.GetRange(index, count);
		}

		public IList<IToken> GetRangeByIndex(int fromIndex, int toIndex)
		{
			return GetRange(fromIndex, toIndex - fromIndex + 1);
		}
	}
}
