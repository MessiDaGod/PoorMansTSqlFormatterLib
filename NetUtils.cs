using System.Collections.Generic;
using System.Xml;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib
{
	public static class NetUtils
	{
		public static XmlDocument ToXmlDoc(this Node value)
		{
			XmlDocument outDoc;
			outDoc = new XmlDocument();
			outDoc.AppendChild(ConvertThingToXmlNode(outDoc, value));
			return outDoc;
		}

		private static XmlNode ConvertThingToXmlNode(XmlDocument outDoc, Node currentNode)
		{
			XmlNode copyOfThisNode;
			copyOfThisNode = outDoc.CreateNode(XmlNodeType.Element, currentNode.Name, null);
			foreach (KeyValuePair<string, string> attribute in currentNode.Attributes)
			{
				XmlAttribute newAttribute;
				newAttribute = outDoc.CreateAttribute(null, attribute.Key, null);
				newAttribute.Value = attribute.Value;
				copyOfThisNode.Attributes.Append(newAttribute);
			}
			copyOfThisNode.InnerText = currentNode.TextValue ?? "";
			foreach (Node child in currentNode.Children)
			{
				copyOfThisNode.AppendChild(ConvertThingToXmlNode(outDoc, child));
			}
			return copyOfThisNode;
		}

		public static char ToLowerInvariant(this char value)
		{
			return char.ToLowerInvariant(value);
		}

		public static char ToUpperInvariant(this char value)
		{
			return char.ToLowerInvariant(value);
		}
	}
}
