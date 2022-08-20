using System.Collections.Generic;

namespace PoorMansTSqlFormatterLib.ParseStructure
{
	public interface Node
	{
		string Name { get; }

		string TextValue { get; }

		IDictionary<string, string> Attributes { get; }

		Node Parent { get; }

		IEnumerable<Node> Children { get; }

		void SetAttribute(string name, string value);

		string GetAttributeValue(string name);

		void RemoveAttribute(string name);

		void AddChild(Node child);

		void InsertChildBefore(Node newChild, Node existingChild);

		void RemoveChild(Node childThing);
	}
}
