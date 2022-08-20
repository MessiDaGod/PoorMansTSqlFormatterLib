using System.Text;

namespace PoorMansTSqlFormatterLib
{
	public static class Utils
	{
		public static string HtmlEncode(string raw)
		{
			if (raw == null)
			{
				return null;
			}
			StringBuilder outBuilder;
			outBuilder = null;
			int latestCheckPos;
			latestCheckPos = 0;
			int latestReplacementPos;
			latestReplacementPos = 0;
			foreach (char c in raw)
			{
				string replacementString;
				replacementString = null;
				switch (c)
				{
				case '>':
					replacementString = "&gt;";
					break;
				case '<':
					replacementString = "&lt;";
					break;
				case '&':
					replacementString = "&amp;";
					break;
				case '"':
					replacementString = "&quot;";
					break;
				}
				if (replacementString != null)
				{
					if (outBuilder == null)
					{
						outBuilder = new StringBuilder(raw.Length);
					}
					if (latestReplacementPos < latestCheckPos)
					{
						outBuilder.Append(raw.Substring(latestReplacementPos, latestCheckPos - latestReplacementPos));
					}
					outBuilder.Append(replacementString);
					latestReplacementPos = latestCheckPos + 1;
				}
				latestCheckPos++;
			}
			if (outBuilder != null)
			{
				if (latestReplacementPos < latestCheckPos)
				{
					outBuilder.Append(raw.Substring(latestReplacementPos));
				}
				return outBuilder.ToString();
			}
			return raw;
		}
	}
}
