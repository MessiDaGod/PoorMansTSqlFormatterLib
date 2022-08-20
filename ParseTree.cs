using System;
using System.Linq;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib
{
	internal class ParseTree : NodeImpl, Node
	{
		private Node _currentContainer;

		private bool _newStatementDue;

		internal Node CurrentContainer
		{
			get
			{
				return _currentContainer;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("CurrentContainer");
				}
				if (!value.RootContainer().Equals(this))
				{
					throw new Exception("Current Container node can only be set to an element in the current document.");
				}
				_currentContainer = value;
			}
		}

		internal bool NewStatementDue
		{
			get
			{
				return _newStatementDue;
			}
			set
			{
				_newStatementDue = value;
			}
		}

		internal bool ErrorFound
		{
			get
			{
				return _newStatementDue;
			}
			private set
			{
				if (value)
				{
					SetAttribute("errorFound", "1");
				}
				else
				{
					RemoveAttribute("errorFound");
				}
			}
		}

		public ParseTree(string rootName)
		{
			Name = rootName;
			CurrentContainer = this;
		}

		internal void SetError()
		{
			CurrentContainer.SetAttribute("hasError", "1");
			ErrorFound = true;
		}

		internal Node SaveNewElement(string newElementName, string newElementValue)
		{
			return SaveNewElement(newElementName, newElementValue, CurrentContainer);
		}

		internal Node SaveNewElement(string newElementName, string newElementValue, Node targetNode)
		{
			Node newElement;
			newElement = NodeFactory.CreateNode(newElementName, newElementValue);
			targetNode.AddChild(newElement);
			return newElement;
		}

		internal Node SaveNewElementWithError(string newElementName, string newElementValue)
		{
			Node newElement;
			newElement = SaveNewElement(newElementName, newElementValue);
			SetError();
			return newElement;
		}

		internal Node SaveNewElementAsPriorSibling(string newElementName, string newElementValue, Node nodeToSaveBefore)
		{
			Node newElement;
			newElement = NodeFactory.CreateNode(newElementName, newElementValue);
			nodeToSaveBefore.Parent.InsertChildBefore(newElement, nodeToSaveBefore);
			return newElement;
		}

		internal void StartNewContainer(string newElementName, string containerOpenValue, string containerType)
		{
			CurrentContainer = SaveNewElement(newElementName, "");
			Node containerOpen;
			containerOpen = SaveNewElement("ContainerOpen", "");
			SaveNewElement("OtherKeyword", containerOpenValue, containerOpen);
			CurrentContainer = SaveNewElement(containerType, "");
		}

		internal void StartNewStatement()
		{
			StartNewStatement(CurrentContainer);
		}

		internal void StartNewStatement(Node targetNode)
		{
			NewStatementDue = false;
			Node newStatement;
			newStatement = SaveNewElement("SqlStatement", "", targetNode);
			CurrentContainer = SaveNewElement("Clause", "", newStatement);
		}

		internal void EscapeAnyBetweenConditions()
		{
			if (PathNameMatches(0, "UpperBound") && PathNameMatches(1, "Between"))
			{
				MoveToAncestorContainer(2);
			}
		}

		internal void EscapeMergeAction()
		{
			if (PathNameMatches(0, "Clause") && PathNameMatches(1, "SqlStatement") && PathNameMatches(2, "MergeAction") && HasNonWhiteSpaceNonCommentContent(CurrentContainer))
			{
				MoveToAncestorContainer(4);
			}
		}

		internal void EscapePartialStatementContainers()
		{
			if (PathNameMatches(0, "DDLProceduralBlock") || PathNameMatches(0, "DDLOtherBlock") || PathNameMatches(0, "DDLDeclareBlock"))
			{
				MoveToAncestorContainer(1);
			}
			else if (PathNameMatches(0, "ContainerContentBody") && PathNameMatches(1, "CursorForOptions"))
			{
				MoveToAncestorContainer(3);
			}
			else if (PathNameMatches(0, "ContainerContentBody") && PathNameMatches(1, "PermissionsRecipient"))
			{
				MoveToAncestorContainer(3);
			}
			else if (PathNameMatches(0, "ContainerContentBody") && PathNameMatches(1, "DDLWith") && (PathNameMatches(2, "PermissionsBlock") || PathNameMatches(2, "DDLProceduralBlock") || PathNameMatches(2, "DDLOtherBlock") || PathNameMatches(2, "DDLDeclareBlock")))
			{
				MoveToAncestorContainer(3);
			}
			else if (PathNameMatches(0, "MergeWhen"))
			{
				MoveToAncestorContainer(2);
			}
			else if (PathNameMatches(0, "ContainerContentBody") && (PathNameMatches(1, "CTEWithClause") || PathNameMatches(1, "DDLDeclareBlock")))
			{
				MoveToAncestorContainer(2);
			}
		}

		internal void EscapeAnySingleOrPartialStatementContainers()
		{
			EscapeAnyBetweenConditions();
			EscapeAnySelectionTarget();
			EscapeJoinCondition();
			if (!HasNonWhiteSpaceNonCommentContent(CurrentContainer))
			{
				return;
			}
			EscapeCursorForBlock();
			EscapeMergeAction();
			EscapePartialStatementContainers();
			while (PathNameMatches(0, "Clause") && PathNameMatches(1, "SqlStatement") && PathNameMatches(2, "ContainerSingleStatementBody"))
			{
				Node currentSingleContainer;
				currentSingleContainer = CurrentContainer.Parent.Parent;
				if (PathNameMatches(currentSingleContainer, 1, "ElseClause"))
				{
					CurrentContainer = currentSingleContainer.Parent.Parent.Parent;
				}
				else
				{
					CurrentContainer = currentSingleContainer.Parent.Parent;
				}
			}
		}

		private void EscapeCursorForBlock()
		{
			if (PathNameMatches(0, "Clause") && PathNameMatches(1, "SqlStatement") && PathNameMatches(2, "ContainerContentBody") && PathNameMatches(3, "CursorForBlock") && HasNonWhiteSpaceNonCommentContent(CurrentContainer))
			{
				MoveToAncestorContainer(5);
			}
		}

		private Node EscapeAndLocateNextStatementContainer(bool escapeEmptyContainer)
		{
			EscapeAnySingleOrPartialStatementContainers();
			if (PathNameMatches(0, "BooleanExpression") && (PathNameMatches(1, "IfStatement") || PathNameMatches(1, "WhileLoop")))
			{
				return SaveNewElement("ContainerSingleStatementBody", "", CurrentContainer.Parent);
			}
			if (PathNameMatches(0, "Clause") && PathNameMatches(1, "SqlStatement") && (escapeEmptyContainer || HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer)))
			{
				return CurrentContainer.Parent.Parent;
			}
			return null;
		}

		private void MigrateApplicableCommentsFromContainer(Node previousContainerElement)
		{
			Node migrationContext;
			migrationContext = previousContainerElement;
			Node migrationCandidate;
			migrationCandidate = previousContainerElement.Children.Last();
			Node insertBeforeNode;
			insertBeforeNode = CurrentContainer;
			while (migrationCandidate != null)
			{
				if (migrationCandidate.Name.Equals("WhiteSpace"))
				{
					migrationCandidate = migrationCandidate.PreviousSibling();
				}
				else if (migrationCandidate.PreviousSibling() != null && SqlStructureConstants.ENAMELIST_COMMENT.Contains(migrationCandidate.Name) && SqlStructureConstants.ENAMELIST_NONCONTENT.Contains(migrationCandidate.PreviousSibling().Name))
				{
					if (migrationCandidate.PreviousSibling().Name.Equals("WhiteSpace") && Regex.IsMatch(migrationCandidate.PreviousSibling().TextValue, "(\\r|\\n)+"))
					{
						while (!migrationContext.Children.Last().Equals(migrationCandidate))
						{
							Node movingNode;
							movingNode = migrationContext.Children.Last();
							movingNode.Parent.RemoveChild(movingNode);
							CurrentContainer.Parent.InsertChildBefore(movingNode, insertBeforeNode);
							insertBeforeNode = movingNode;
						}
						migrationCandidate.Parent.RemoveChild(migrationCandidate);
						CurrentContainer.Parent.InsertChildBefore(migrationCandidate, insertBeforeNode);
						insertBeforeNode = migrationCandidate;
						migrationCandidate = migrationContext.Children.Last();
					}
					else
					{
						migrationCandidate = migrationCandidate.PreviousSibling();
					}
				}
				else if (!string.IsNullOrEmpty(migrationCandidate.TextValue))
				{
					migrationCandidate = null;
				}
				else
				{
					migrationContext = migrationCandidate;
					migrationCandidate = migrationCandidate.Children.LastOrDefault();
				}
			}
		}

		internal void ConsiderStartingNewStatement()
		{
			EscapeAnyBetweenConditions();
			EscapeAnySelectionTarget();
			EscapeJoinCondition();
			Node previousContainerElement;
			previousContainerElement = CurrentContainer;
			Node nextStatementContainer;
			nextStatementContainer = EscapeAndLocateNextStatementContainer(escapeEmptyContainer: false);
			if (nextStatementContainer != null)
			{
				Node inBetweenContainerElement;
				inBetweenContainerElement = CurrentContainer;
				StartNewStatement(nextStatementContainer);
				if (!inBetweenContainerElement.Equals(previousContainerElement))
				{
					MigrateApplicableCommentsFromContainer(inBetweenContainerElement);
				}
				MigrateApplicableCommentsFromContainer(previousContainerElement);
			}
		}

		internal void ConsiderStartingNewClause()
		{
			EscapeAnySelectionTarget();
			EscapeAnyBetweenConditions();
			EscapePartialStatementContainers();
			EscapeJoinCondition();
			if (CurrentContainer.Name.Equals("Clause") && HasNonWhiteSpaceNonSingleCommentContent(CurrentContainer))
			{
				Node previousContainerElement;
				previousContainerElement = CurrentContainer;
				CurrentContainer = SaveNewElement("Clause", "", CurrentContainer.Parent);
				MigrateApplicableCommentsFromContainer(previousContainerElement);
			}
			else if (CurrentContainer.Name.Equals("ExpressionParens") || CurrentContainer.Name.Equals("InParens") || CurrentContainer.Name.Equals("SelectionTargetParens") || CurrentContainer.Name.Equals("SqlStatement"))
			{
				CurrentContainer = SaveNewElement("Clause", "");
			}
		}

		internal void EscapeAnySelectionTarget()
		{
			if (PathNameMatches(0, "SelectionTarget"))
			{
				CurrentContainer = CurrentContainer.Parent;
			}
		}

		internal void EscapeJoinCondition()
		{
			if (PathNameMatches(0, "ContainerContentBody") && PathNameMatches(1, "JoinOn"))
			{
				MoveToAncestorContainer(2);
			}
		}

		internal bool FindValidBatchEnd()
		{
			Node nextStatementContainer;
			nextStatementContainer = EscapeAndLocateNextStatementContainer(escapeEmptyContainer: true);
			return nextStatementContainer != null && (nextStatementContainer.Name.Equals("SqlRoot") || (nextStatementContainer.Name.Equals("ContainerContentBody") && nextStatementContainer.Parent.Name.Equals("DDLAsBlock")));
		}

		internal bool PathNameMatches(int levelsUp, string nameToMatch)
		{
			return PathNameMatches(CurrentContainer, levelsUp, nameToMatch);
		}

		internal bool PathNameMatches(Node targetNode, int levelsUp, string nameToMatch)
		{
			Node currentNode;
			currentNode = targetNode;
			while (levelsUp > 0)
			{
				currentNode = currentNode.Parent;
				levelsUp--;
			}
			return currentNode?.Name.Equals(nameToMatch) ?? false;
		}

		private static bool HasNonWhiteSpaceNonSingleCommentContent(Node containerNode)
		{
			foreach (Node testElement in containerNode.Children)
			{
				if (!testElement.Name.Equals("WhiteSpace") && !testElement.Name.Equals("SingleLineComment") && !testElement.Name.Equals("SingleLineCommentCStyle") && (!testElement.Name.Equals("MultiLineComment") || Regex.IsMatch(testElement.TextValue, "(\\r|\\n)+")))
				{
					return true;
				}
			}
			return false;
		}

		internal bool HasNonWhiteSpaceNonCommentContent(Node containerNode)
		{
			return containerNode.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).Any();
		}

		internal Node GetFirstNonWhitespaceNonCommentChildElement(Node targetElement)
		{
			return targetElement.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).FirstOrDefault();
		}

		internal Node GetLastNonWhitespaceNonCommentChildElement(Node targetElement)
		{
			return targetElement.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
		}

		internal void MoveToAncestorContainer(int levelsUp)
		{
			MoveToAncestorContainer(levelsUp, null);
		}

		internal void MoveToAncestorContainer(int levelsUp, string targetContainerName)
		{
			Node candidateContainer;
			candidateContainer = CurrentContainer;
			while (levelsUp > 0)
			{
				candidateContainer = candidateContainer.Parent;
				levelsUp--;
			}
			if (string.IsNullOrEmpty(targetContainerName) || candidateContainer.Name.Equals(targetContainerName))
			{
				CurrentContainer = candidateContainer;
				return;
			}
			throw new Exception("Ancestor node does not match expected name!");
		}
	}
}
