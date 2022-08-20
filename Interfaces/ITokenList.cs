using System.Collections;
using System.Collections.Generic;

namespace PoorMansTSqlFormatterLib.Interfaces
{
	public interface ITokenList : IList<IToken>, ICollection<IToken>, IEnumerable<IToken>, IEnumerable
	{
		bool HasUnfinishedToken { get; set; }

		IToken MarkerToken { get; }

		long? MarkerPosition { get; }

		string PrettyPrint();

		IList<IToken> GetRange(int index, int count);

		IList<IToken> GetRangeByIndex(int fromIndex, int toIndex);
	}
}
