using System;
using System.Collections.Generic;
using System.Linq;

namespace PoorMansTSqlFormatterLib.ParseStructure
{
	public static class NodeExtensions
	{
		public static Node FollowingChild(this Node value, Node fromChild)
		{
			if (value == null)
			{
				return null;
			}
			if (fromChild == null)
			{
				throw new ArgumentNullException("fromChild");
			}
			bool targetFound;
			targetFound = false;
			Node sibling;
			sibling = null;
			foreach (Node child in value.Children)
			{
				if (targetFound)
				{
					sibling = child;
					break;
				}
				if (child == fromChild)
				{
					targetFound = true;
				}
			}
			return sibling;
		}

		public static Node PreviousChild(this Node value, Node fromChild)
		{
			if (value == null)
			{
				return null;
			}
			if (fromChild == null)
			{
				throw new ArgumentNullException("fromChild");
			}
			Node previousSibling;
			previousSibling = null;
			foreach (Node child in value.Children)
			{
				if (child == fromChild)
				{
					return previousSibling;
				}
				previousSibling = child;
			}
			return null;
		}

		public static Node NextSibling(this Node value)
		{
			if (value == null || value.Parent == null)
			{
				return null;
			}
			return value.Parent.FollowingChild(value);
		}

		public static Node PreviousSibling(this Node value)
		{
			if (value == null || value.Parent == null)
			{
				return null;
			}
			return value.Parent.PreviousChild(value);
		}

		public static Node RootContainer(this Node value)
		{
			if (value == null)
			{
				return null;
			}
			Node currentParent;
			currentParent = value;
			while (currentParent.Parent != null)
			{
				currentParent = currentParent.Parent;
			}
			return currentParent;
		}

		public static IEnumerable<Node> ChildrenByName(this Node value, string name)
		{
			if (value == null)
			{
				return Enumerable.Empty<Node>();
			}
			return value.Children.Where(p => p.Name == name);
		}

		public static IEnumerable<Node> ChildrenByNames(this Node value, IEnumerable<string> names)
		{
			if (value == null)
			{
				return Enumerable.Empty<Node>();
			}
			return value.Children.Where(p => names.Contains(p.Name));
		}

		public static IEnumerable<Node> ChildrenExcludingNames(this Node value, IEnumerable<string> names)
		{
			if (value == null)
			{
				return Enumerable.Empty<Node>();
			}
			return value.Children.Where(p => !names.Contains(p.Name));
		}

		public static Node ChildByName(this Node value, string name)
		{
			return value.ChildrenByName(name).SingleOrDefault();
		}

		public static Node ChildByNames(this Node value, IEnumerable<string> names)
		{
			return value.ChildrenByNames(names).SingleOrDefault();
		}

		public static Node ChildExcludingNames(this Node value, IEnumerable<string> names)
		{
			return value.ChildrenExcludingNames(names).SingleOrDefault();
		}

		public static Node ExtractStructureBetween(Node startingElement, Node endingElement)
		{
			Node currentNode;
			currentNode = startingElement;
			Node previousNode;
			previousNode = null;
			Node remainder;
			remainder = null;
			Node remainderPosition;
			remainderPosition = null;
			while (currentNode != null && !currentNode.Equals(endingElement))
			{
				if (previousNode != null)
				{
					Node copyOfThisNode;
					copyOfThisNode = NodeFactory.CreateNode(currentNode.Name, currentNode.TextValue);
					foreach (KeyValuePair<string, string> attribute in currentNode.Attributes)
					{
						copyOfThisNode.SetAttribute(attribute.Key, attribute.Value);
					}
					if (remainderPosition == null)
					{
						remainderPosition = copyOfThisNode;
						remainder = copyOfThisNode;
					}
					else if (currentNode.Equals(previousNode.Parent) && remainderPosition.Parent != null)
					{
						remainderPosition = remainderPosition.Parent;
					}
					else if (currentNode.Equals(previousNode.Parent) && remainderPosition.Parent == null)
					{
						copyOfThisNode.AddChild(remainderPosition);
						remainderPosition = copyOfThisNode;
						remainder = copyOfThisNode;
					}
					else if (currentNode.Equals(previousNode.NextSibling()) && remainderPosition.Parent != null)
					{
						remainderPosition.Parent.AddChild(copyOfThisNode);
						remainderPosition = copyOfThisNode;
					}
					else if (currentNode.Equals(previousNode.NextSibling()) && remainderPosition.Parent == null)
					{
						Node copyOfThisNodesParent;
						copyOfThisNodesParent = NodeFactory.CreateNode(currentNode.Parent.Name, currentNode.Parent.TextValue);
						remainder = copyOfThisNodesParent;
						remainder.AddChild(remainderPosition);
						remainder.AddChild(copyOfThisNode);
						remainderPosition = copyOfThisNode;
					}
					else
					{
						remainderPosition.AddChild(copyOfThisNode);
						remainderPosition = copyOfThisNode;
					}
				}
				Node nextNode;
				nextNode = null;
				nextNode = ((previousNode != null && currentNode.Children.Any() && !currentNode.Equals(previousNode.Parent)) ? currentNode.Children.FirstOrDefault() : ((currentNode.NextSibling() == null) ? currentNode.Parent : currentNode.NextSibling()));
				previousNode = currentNode;
				currentNode = nextNode;
			}
			return remainder;
		}
	}
}
