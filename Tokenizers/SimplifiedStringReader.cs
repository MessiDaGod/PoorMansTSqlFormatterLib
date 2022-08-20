namespace PoorMansTSqlFormatterLib.Tokenizers
{
	internal class SimplifiedStringReader
	{
		private char[] inputChars;

		private int nextCharIndex;

		internal long LastCharacterPosition
		{
			get
			{
				if (nextCharIndex <= inputChars.Length)
				{
					return nextCharIndex;
				}
				return inputChars.Length;
			}
		}

		public SimplifiedStringReader(string inputString)
		{
			inputChars = inputString.ToCharArray();
		}

		internal int Read()
		{
			int nextChar;
			nextChar = Peek();
			nextCharIndex++;
			return nextChar;
		}

		internal int Peek()
		{
			if (nextCharIndex < inputChars.Length)
			{
				return inputChars[nextCharIndex];
			}
			return -1;
		}
	}
}
