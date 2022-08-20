namespace PoorMansTSqlFormatterLib.ParseStructure
{
	public static class NodeFactory
	{
		public static Node CreateNode(string name, string textValue)
		{
			return new NodeImpl
			{
				Name = name,
				TextValue = textValue
			};
		}
	}
}
