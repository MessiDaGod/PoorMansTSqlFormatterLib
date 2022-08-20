using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Parsers
{
	public class TSqlStandardParser : ISqlTokenParser
	{
		public enum KeywordType
		{
			OperatorKeyword = 0,
			FunctionKeyword = 1,
			DataTypeKeyword = 2,
			OtherKeyword = 3
		}

		private static Regex _JoinDetector;

		private static Regex _CursorDetector;

		private static Regex _TriggerConditionDetector;

		public static Dictionary<string, KeywordType> KeywordList { get; set; }

		static TSqlStandardParser()
		{
			_JoinDetector = new Regex("^((RIGHT|INNER|LEFT|CROSS|FULL) )?(OUTER )?((HASH|LOOP|MERGE|REMOTE) )?(JOIN|APPLY) ");
			_CursorDetector = new Regex("^DECLARE (?:[#\\$0-9@-Z_a-z\\xAA\\xB5\\xBA\\xC0-\\xD6\\xD8-\\xF6\\xF8-\\u02C1\\u02C6-\\u02D1\\u02E0-\\u02E4\\u02EC\\u02EE\\u0370-\\u0374\\u0376\\u0377\\u037A-\\u037D\\u037F\\u0386\\u0388-\\u038A\\u038C\\u038E-\\u03A1\\u03A3-\\u03F5\\u03F7-\\u0481\\u048A-\\u052F\\u0531-\\u0556\\u0559\\u0561-\\u0587\\u05D0-\\u05EA\\u05F0-\\u05F2\\u0620-\\u064A\\u066E\\u066F\\u0671-\\u06D3\\u06D5\\u06E5\\u06E6\\u06EE\\u06EF\\u06FA-\\u06FC\\u06FF\\u0710\\u0712-\\u072F\\u074D-\\u07A5\\u07B1\\u07CA-\\u07EA\\u07F4\\u07F5\\u07FA\\u0800-\\u0815\\u081A\\u0824\\u0828\\u0840-\\u0858\\u0860-\\u086A\\u08A0-\\u08B4\\u08B6-\\u08BD\\u0904-\\u0939\\u093D\\u0950\\u0958-\\u0961\\u0971-\\u0980\\u0985-\\u098C\\u098F\\u0990\\u0993-\\u09A8\\u09AA-\\u09B0\\u09B2\\u09B6-\\u09B9\\u09BD\\u09CE\\u09DC\\u09DD\\u09DF-\\u09E1\\u09F0\\u09F1\\u09FC\\u0A05-\\u0A0A\\u0A0F\\u0A10\\u0A13-\\u0A28\\u0A2A-\\u0A30\\u0A32\\u0A33\\u0A35\\u0A36\\u0A38\\u0A39\\u0A59-\\u0A5C\\u0A5E\\u0A72-\\u0A74\\u0A85-\\u0A8D\\u0A8F-\\u0A91\\u0A93-\\u0AA8\\u0AAA-\\u0AB0\\u0AB2\\u0AB3\\u0AB5-\\u0AB9\\u0ABD\\u0AD0\\u0AE0\\u0AE1\\u0AF9\\u0B05-\\u0B0C\\u0B0F\\u0B10\\u0B13-\\u0B28\\u0B2A-\\u0B30\\u0B32\\u0B33\\u0B35-\\u0B39\\u0B3D\\u0B5C\\u0B5D\\u0B5F-\\u0B61\\u0B71\\u0B83\\u0B85-\\u0B8A\\u0B8E-\\u0B90\\u0B92-\\u0B95\\u0B99\\u0B9A\\u0B9C\\u0B9E\\u0B9F\\u0BA3\\u0BA4\\u0BA8-\\u0BAA\\u0BAE-\\u0BB9\\u0BD0\\u0C05-\\u0C0C\\u0C0E-\\u0C10\\u0C12-\\u0C28\\u0C2A-\\u0C39\\u0C3D\\u0C58-\\u0C5A\\u0C60\\u0C61\\u0C80\\u0C85-\\u0C8C\\u0C8E-\\u0C90\\u0C92-\\u0CA8\\u0CAA-\\u0CB3\\u0CB5-\\u0CB9\\u0CBD\\u0CDE\\u0CE0\\u0CE1\\u0CF1\\u0CF2\\u0D05-\\u0D0C\\u0D0E-\\u0D10\\u0D12-\\u0D3A\\u0D3D\\u0D4E\\u0D54-\\u0D56\\u0D5F-\\u0D61\\u0D7A-\\u0D7F\\u0D85-\\u0D96\\u0D9A-\\u0DB1\\u0DB3-\\u0DBB\\u0DBD\\u0DC0-\\u0DC6\\u0E01-\\u0E30\\u0E32\\u0E33\\u0E40-\\u0E46\\u0E81\\u0E82\\u0E84\\u0E87\\u0E88\\u0E8A\\u0E8D\\u0E94-\\u0E97\\u0E99-\\u0E9F\\u0EA1-\\u0EA3\\u0EA5\\u0EA7\\u0EAA\\u0EAB\\u0EAD-\\u0EB0\\u0EB2\\u0EB3\\u0EBD\\u0EC0-\\u0EC4\\u0EC6\\u0EDC-\\u0EDF\\u0F00\\u0F40-\\u0F47\\u0F49-\\u0F6C\\u0F88-\\u0F8C\\u1000-\\u102A\\u103F\\u1050-\\u1055\\u105A-\\u105D\\u1061\\u1065\\u1066\\u106E-\\u1070\\u1075-\\u1081\\u108E\\u10A0-\\u10C5\\u10C7\\u10CD\\u10D0-\\u10FA\\u10FC-\\u1248\\u124A-\\u124D\\u1250-\\u1256\\u1258\\u125A-\\u125D\\u1260-\\u1288\\u128A-\\u128D\\u1290-\\u12B0\\u12B2-\\u12B5\\u12B8-\\u12BE\\u12C0\\u12C2-\\u12C5\\u12C8-\\u12D6\\u12D8-\\u1310\\u1312-\\u1315\\u1318-\\u135A\\u1380-\\u138F\\u13A0-\\u13F5\\u13F8-\\u13FD\\u1401-\\u166C\\u166F-\\u167F\\u1681-\\u169A\\u16A0-\\u16EA\\u16F1-\\u16F8\\u1700-\\u170C\\u170E-\\u1711\\u1720-\\u1731\\u1740-\\u1751\\u1760-\\u176C\\u176E-\\u1770\\u1780-\\u17B3\\u17D7\\u17DC\\u1820-\\u1877\\u1880-\\u1884\\u1887-\\u18A8\\u18AA\\u18B0-\\u18F5\\u1900-\\u191E\\u1950-\\u196D\\u1970-\\u1974\\u1980-\\u19AB\\u19B0-\\u19C9\\u1A00-\\u1A16\\u1A20-\\u1A54\\u1AA7\\u1B05-\\u1B33\\u1B45-\\u1B4B\\u1B83-\\u1BA0\\u1BAE\\u1BAF\\u1BBA-\\u1BE5\\u1C00-\\u1C23\\u1C4D-\\u1C4F\\u1C5A-\\u1C7D\\u1C80-\\u1C88\\u1CE9-\\u1CEC\\u1CEE-\\u1CF1\\u1CF5\\u1CF6\\u1D00-\\u1DBF\\u1E00-\\u1F15\\u1F18-\\u1F1D\\u1F20-\\u1F45\\u1F48-\\u1F4D\\u1F50-\\u1F57\\u1F59\\u1F5B\\u1F5D\\u1F5F-\\u1F7D\\u1F80-\\u1FB4\\u1FB6-\\u1FBC\\u1FBE\\u1FC2-\\u1FC4\\u1FC6-\\u1FCC\\u1FD0-\\u1FD3\\u1FD6-\\u1FDB\\u1FE0-\\u1FEC\\u1FF2-\\u1FF4\\u1FF6-\\u1FFC\\u2071\\u207F\\u2090-\\u209C\\u2102\\u2107\\u210A-\\u2113\\u2115\\u2119-\\u211D\\u2124\\u2126\\u2128\\u212A-\\u212D\\u212F-\\u2139\\u213C-\\u213F\\u2145-\\u2149\\u214E\\u2183\\u2184\\u2C00-\\u2C2E\\u2C30-\\u2C5E\\u2C60-\\u2CE4\\u2CEB-\\u2CEE\\u2CF2\\u2CF3\\u2D00-\\u2D25\\u2D27\\u2D2D\\u2D30-\\u2D67\\u2D6F\\u2D80-\\u2D96\\u2DA0-\\u2DA6\\u2DA8-\\u2DAE\\u2DB0-\\u2DB6\\u2DB8-\\u2DBE\\u2DC0-\\u2DC6\\u2DC8-\\u2DCE\\u2DD0-\\u2DD6\\u2DD8-\\u2DDE\\u2E2F\\u3005\\u3006\\u3031-\\u3035\\u303B\\u303C\\u3041-\\u3096\\u309D-\\u309F\\u30A1-\\u30FA\\u30FC-\\u30FF\\u3105-\\u312E\\u3131-\\u318E\\u31A0-\\u31BA\\u31F0-\\u31FF\\u3400-\\u4DB5\\u4E00-\\u9FEA\\uA000-\\uA48C\\uA4D0-\\uA4FD\\uA500-\\uA60C\\uA610-\\uA61F\\uA62A\\uA62B\\uA640-\\uA66E\\uA67F-\\uA69D\\uA6A0-\\uA6E5\\uA717-\\uA71F\\uA722-\\uA788\\uA78B-\\uA7AE\\uA7B0-\\uA7B7\\uA7F7-\\uA801\\uA803-\\uA805\\uA807-\\uA80A\\uA80C-\\uA822\\uA840-\\uA873\\uA882-\\uA8B3\\uA8F2-\\uA8F7\\uA8FB\\uA8FD\\uA90A-\\uA925\\uA930-\\uA946\\uA960-\\uA97C\\uA984-\\uA9B2\\uA9CF\\uA9E0-\\uA9E4\\uA9E6-\\uA9EF\\uA9FA-\\uA9FE\\uAA00-\\uAA28\\uAA40-\\uAA42\\uAA44-\\uAA4B\\uAA60-\\uAA76\\uAA7A\\uAA7E-\\uAAAF\\uAAB1\\uAAB5\\uAAB6\\uAAB9-\\uAABD\\uAAC0\\uAAC2\\uAADB-\\uAADD\\uAAE0-\\uAAEA\\uAAF2-\\uAAF4\\uAB01-\\uAB06\\uAB09-\\uAB0E\\uAB11-\\uAB16\\uAB20-\\uAB26\\uAB28-\\uAB2E\\uAB30-\\uAB5A\\uAB5C-\\uAB65\\uAB70-\\uABE2\\uAC00-\\uD7A3\\uD7B0-\\uD7C6\\uD7CB-\\uD7FB\\uF900-\\uFA6D\\uFA70-\\uFAD9\\uFB00-\\uFB06\\uFB13-\\uFB17\\uFB1D\\uFB1F-\\uFB28\\uFB2A-\\uFB36\\uFB38-\\uFB3C\\uFB3E\\uFB40\\uFB41\\uFB43\\uFB44\\uFB46-\\uFBB1\\uFBD3-\\uFD3D\\uFD50-\\uFD8F\\uFD92-\\uFDC7\\uFDF0-\\uFDFB\\uFE70-\\uFE74\\uFE76-\\uFEFC\\uFF21-\\uFF3A\\uFF41-\\uFF5A\\uFF66-\\uFFBE\\uFFC2-\\uFFC7\\uFFCA-\\uFFCF\\uFFD2-\\uFFD7\\uFFDA-\\uFFDC]|\\uD800[\\uDC00-\\uDC0B\\uDC0D-\\uDC26\\uDC28-\\uDC3A\\uDC3C\\uDC3D\\uDC3F-\\uDC4D\\uDC50-\\uDC5D\\uDC80-\\uDCFA\\uDE80-\\uDE9C\\uDEA0-\\uDED0\\uDF00-\\uDF1F\\uDF2D-\\uDF40\\uDF42-\\uDF49\\uDF50-\\uDF75\\uDF80-\\uDF9D\\uDFA0-\\uDFC3\\uDFC8-\\uDFCF]|\\uD801[\\uDC00-\\uDC9D\\uDCB0-\\uDCD3\\uDCD8-\\uDCFB\\uDD00-\\uDD27\\uDD30-\\uDD63\\uDE00-\\uDF36\\uDF40-\\uDF55\\uDF60-\\uDF67]|\\uD802[\\uDC00-\\uDC05\\uDC08\\uDC0A-\\uDC35\\uDC37\\uDC38\\uDC3C\\uDC3F-\\uDC55\\uDC60-\\uDC76\\uDC80-\\uDC9E\\uDCE0-\\uDCF2\\uDCF4\\uDCF5\\uDD00-\\uDD15\\uDD20-\\uDD39\\uDD80-\\uDDB7\\uDDBE\\uDDBF\\uDE00\\uDE10-\\uDE13\\uDE15-\\uDE17\\uDE19-\\uDE33\\uDE60-\\uDE7C\\uDE80-\\uDE9C\\uDEC0-\\uDEC7\\uDEC9-\\uDEE4\\uDF00-\\uDF35\\uDF40-\\uDF55\\uDF60-\\uDF72\\uDF80-\\uDF91]|\\uD803[\\uDC00-\\uDC48\\uDC80-\\uDCB2\\uDCC0-\\uDCF2]|\\uD804[\\uDC03-\\uDC37\\uDC83-\\uDCAF\\uDCD0-\\uDCE8\\uDD03-\\uDD26\\uDD50-\\uDD72\\uDD76\\uDD83-\\uDDB2\\uDDC1-\\uDDC4\\uDDDA\\uDDDC\\uDE00-\\uDE11\\uDE13-\\uDE2B\\uDE80-\\uDE86\\uDE88\\uDE8A-\\uDE8D\\uDE8F-\\uDE9D\\uDE9F-\\uDEA8\\uDEB0-\\uDEDE\\uDF05-\\uDF0C\\uDF0F\\uDF10\\uDF13-\\uDF28\\uDF2A-\\uDF30\\uDF32\\uDF33\\uDF35-\\uDF39\\uDF3D\\uDF50\\uDF5D-\\uDF61]|\\uD805[\\uDC00-\\uDC34\\uDC47-\\uDC4A\\uDC80-\\uDCAF\\uDCC4\\uDCC5\\uDCC7\\uDD80-\\uDDAE\\uDDD8-\\uDDDB\\uDE00-\\uDE2F\\uDE44\\uDE80-\\uDEAA\\uDF00-\\uDF19]|\\uD806[\\uDCA0-\\uDCDF\\uDCFF\\uDE00\\uDE0B-\\uDE32\\uDE3A\\uDE50\\uDE5C-\\uDE83\\uDE86-\\uDE89\\uDEC0-\\uDEF8]|\\uD807[\\uDC00-\\uDC08\\uDC0A-\\uDC2E\\uDC40\\uDC72-\\uDC8F\\uDD00-\\uDD06\\uDD08\\uDD09\\uDD0B-\\uDD30\\uDD46]|\\uD808[\\uDC00-\\uDF99]|\\uD809[\\uDC80-\\uDD43]|[\\uD80C\\uD81C-\\uD820\\uD840-\\uD868\\uD86A-\\uD86C\\uD86F-\\uD872\\uD874-\\uD879][\\uDC00-\\uDFFF]|\\uD80D[\\uDC00-\\uDC2E]|\\uD811[\\uDC00-\\uDE46]|\\uD81A[\\uDC00-\\uDE38\\uDE40-\\uDE5E\\uDED0-\\uDEED\\uDF00-\\uDF2F\\uDF40-\\uDF43\\uDF63-\\uDF77\\uDF7D-\\uDF8F]|\\uD81B[\\uDF00-\\uDF44\\uDF50\\uDF93-\\uDF9F\\uDFE0\\uDFE1]|\\uD821[\\uDC00-\\uDFEC]|\\uD822[\\uDC00-\\uDEF2]|\\uD82C[\\uDC00-\\uDD1E\\uDD70-\\uDEFB]|\\uD82F[\\uDC00-\\uDC6A\\uDC70-\\uDC7C\\uDC80-\\uDC88\\uDC90-\\uDC99]|\\uD835[\\uDC00-\\uDC54\\uDC56-\\uDC9C\\uDC9E\\uDC9F\\uDCA2\\uDCA5\\uDCA6\\uDCA9-\\uDCAC\\uDCAE-\\uDCB9\\uDCBB\\uDCBD-\\uDCC3\\uDCC5-\\uDD05\\uDD07-\\uDD0A\\uDD0D-\\uDD14\\uDD16-\\uDD1C\\uDD1E-\\uDD39\\uDD3B-\\uDD3E\\uDD40-\\uDD44\\uDD46\\uDD4A-\\uDD50\\uDD52-\\uDEA5\\uDEA8-\\uDEC0\\uDEC2-\\uDEDA\\uDEDC-\\uDEFA\\uDEFC-\\uDF14\\uDF16-\\uDF34\\uDF36-\\uDF4E\\uDF50-\\uDF6E\\uDF70-\\uDF88\\uDF8A-\\uDFA8\\uDFAA-\\uDFC2\\uDFC4-\\uDFCB]|\\uD83A[\\uDC00-\\uDCC4\\uDD00-\\uDD43]|\\uD83B[\\uDE00-\\uDE03\\uDE05-\\uDE1F\\uDE21\\uDE22\\uDE24\\uDE27\\uDE29-\\uDE32\\uDE34-\\uDE37\\uDE39\\uDE3B\\uDE42\\uDE47\\uDE49\\uDE4B\\uDE4D-\\uDE4F\\uDE51\\uDE52\\uDE54\\uDE57\\uDE59\\uDE5B\\uDE5D\\uDE5F\\uDE61\\uDE62\\uDE64\\uDE67-\\uDE6A\\uDE6C-\\uDE72\\uDE74-\\uDE77\\uDE79-\\uDE7C\\uDE7E\\uDE80-\\uDE89\\uDE8B-\\uDE9B\\uDEA1-\\uDEA3\\uDEA5-\\uDEA9\\uDEAB-\\uDEBB]|\\uD869[\\uDC00-\\uDED6\\uDF00-\\uDFFF]|\\uD86D[\\uDC00-\\uDF34\\uDF40-\\uDFFF]|\\uD86E[\\uDC00-\\uDC1D\\uDC20-\\uDFFF]|\\uD873[\\uDC00-\\uDEA1\\uDEB0-\\uDFFF]|\\uD87A[\\uDC00-\\uDFE0]|\\uD87E[\\uDC00-\\uDE1D])+ ((INSENSITIVE|SCROLL) ){0,2}CURSOR ");
			_TriggerConditionDetector = new Regex("^(FOR|AFTER|INSTEAD OF)( (INSERT|UPDATE|DELETE) (, (INSERT|UPDATE|DELETE) )?(, (INSERT|UPDATE|DELETE) )?)");
			InitializeKeywordList();
			KeywordList.Take(3);
		}

		public Node ParseSQL(ITokenList tokenList)
		{
			ParseTree sqlTree;
			sqlTree = new ParseTree("SqlRoot");
			sqlTree.StartNewStatement();
			int tokenCount;
			tokenCount = tokenList.Count;
			for (int tokenID = 0; tokenID < tokenCount; tokenID++)
			{
				IToken token;
				token = tokenList[tokenID];
				switch (token.Type)
				{
				case SqlTokenType.OpenParens:
				{
					Node firstNonCommentParensSibling;
					firstNonCommentParensSibling = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
					Node lastNonCommentParensSibling;
					lastNonCommentParensSibling = sqlTree.GetLastNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
					bool isInsertOrValuesClause;
					isInsertOrValuesClause = firstNonCommentParensSibling != null && ((firstNonCommentParensSibling.Name.Equals("OtherKeyword") && firstNonCommentParensSibling.TextValue.ToUpperInvariant().StartsWith("INSERT")) || (firstNonCommentParensSibling.Name.Equals("CompoundKeyword") && firstNonCommentParensSibling.GetAttributeValue("simpleText").ToUpperInvariant().StartsWith("INSERT ")) || (firstNonCommentParensSibling.Name.Equals("OtherKeyword") && firstNonCommentParensSibling.TextValue.ToUpperInvariant().StartsWith("VALUES")));
					if (sqlTree.CurrentContainer.Name.Equals("CTEAlias") && sqlTree.CurrentContainer.Parent.Name.Equals("CTEWithClause"))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLParens", "");
					}
					else if (sqlTree.CurrentContainer.Name.Equals("ContainerContentBody") && sqlTree.CurrentContainer.Parent.Name.Equals("CTEAsBlock"))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("SelectionTargetParens", "");
					}
					else if (firstNonCommentParensSibling == null && sqlTree.CurrentContainer.Name.Equals("SelectionTarget"))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("SelectionTargetParens", "");
					}
					else if (firstNonCommentParensSibling != null && firstNonCommentParensSibling.Name.Equals("SetOperatorClause"))
					{
						sqlTree.ConsiderStartingNewClause();
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("SelectionTargetParens", "");
					}
					else if (IsLatestTokenADDLDetailValue(sqlTree))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLDetailParens", "");
					}
					else if (sqlTree.CurrentContainer.Name.Equals("DDLProceduralBlock") || sqlTree.CurrentContainer.Name.Equals("DDLOtherBlock") || sqlTree.CurrentContainer.Name.Equals("DDLDeclareBlock") || (sqlTree.CurrentContainer.Name.Equals("Clause") && firstNonCommentParensSibling != null && firstNonCommentParensSibling.Name.Equals("OtherKeyword") && firstNonCommentParensSibling.TextValue.ToUpperInvariant().StartsWith("OPTION")) || isInsertOrValuesClause)
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLParens", "");
					}
					else if (lastNonCommentParensSibling != null && lastNonCommentParensSibling.Name.Equals("AlphaOperator") && lastNonCommentParensSibling.TextValue.ToUpperInvariant().Equals("IN"))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("InParens", "");
					}
					else if (IsLatestTokenAMiscName(sqlTree))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("FunctionParens", "");
					}
					else
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("ExpressionParens", "");
					}
					break;
				}
				case SqlTokenType.CloseParens:
					sqlTree.EscapeAnySingleOrPartialStatementContainers();
					if (sqlTree.CurrentContainer.Name.Equals("DDLDetailParens") || sqlTree.CurrentContainer.Name.Equals("DDLParens") || sqlTree.CurrentContainer.Name.Equals("FunctionParens") || sqlTree.CurrentContainer.Name.Equals("InParens") || sqlTree.CurrentContainer.Name.Equals("ExpressionParens") || sqlTree.CurrentContainer.Name.Equals("SelectionTargetParens"))
					{
						sqlTree.MoveToAncestorContainer(1);
					}
					else if (sqlTree.CurrentContainer.Name.Equals("Clause") && sqlTree.CurrentContainer.Parent.Name.Equals("SelectionTargetParens") && sqlTree.CurrentContainer.Parent.Parent.Name.Equals("ContainerContentBody") && sqlTree.CurrentContainer.Parent.Parent.Parent.Name.Equals("CTEAsBlock"))
					{
						sqlTree.MoveToAncestorContainer(4, "CTEWithClause");
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("ContainerContentBody", "");
					}
					else if (sqlTree.CurrentContainer.Name.Equals("Clause") && (sqlTree.CurrentContainer.Parent.Name.Equals("ExpressionParens") || sqlTree.CurrentContainer.Parent.Name.Equals("InParens") || sqlTree.CurrentContainer.Parent.Name.Equals("SelectionTargetParens")))
					{
						sqlTree.MoveToAncestorContainer(2);
					}
					else
					{
						sqlTree.SaveNewElementWithError("Other", ")");
					}
					break;
				case SqlTokenType.OtherNode:
				{
					List<int> significantTokenPositions;
					significantTokenPositions = GetSignificantTokenPositions(tokenList, tokenID, 7);
					string significantTokensString;
					significantTokensString = ExtractTokensString(tokenList, significantTokenPositions);
					if (sqlTree.PathNameMatches(0, "PermissionsDetail"))
					{
						if (significantTokensString.StartsWith("ON "))
						{
							sqlTree.MoveToAncestorContainer(1, "PermissionsBlock");
							sqlTree.StartNewContainer("PermissionsTarget", token.Value, "ContainerContentBody");
						}
						else if (significantTokensString.StartsWith("TO ") || significantTokensString.StartsWith("FROM "))
						{
							sqlTree.MoveToAncestorContainer(1, "PermissionsBlock");
							sqlTree.StartNewContainer("PermissionsRecipient", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("CREATE PROC") || significantTokensString.StartsWith("CREATE FUNC") || significantTokensString.StartsWith("CREATE TRIGGER ") || significantTokensString.StartsWith("CREATE VIEW ") || significantTokensString.StartsWith("ALTER PROC") || significantTokensString.StartsWith("ALTER FUNC") || significantTokensString.StartsWith("ALTER TRIGGER ") || significantTokensString.StartsWith("ALTER VIEW "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLProceduralBlock", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (_CursorDetector.IsMatch(significantTokensString))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("CursorDeclaration", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (sqlTree.PathNameMatches(0, "DDLProceduralBlock") && _TriggerConditionDetector.IsMatch(significantTokensString))
					{
						Match triggerConditions;
						triggerConditions = _TriggerConditionDetector.Match(significantTokensString);
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("TriggerCondition", "");
						Node triggerConditionType;
						triggerConditionType = sqlTree.SaveNewElement("CompoundKeyword", "");
						string triggerConditionTypeSimpleText;
						triggerConditionTypeSimpleText = triggerConditions.Groups[1].Value;
						triggerConditionType.SetAttribute("simpleText", triggerConditionTypeSimpleText);
						int triggerConditionTypeNodeCount;
						triggerConditionTypeNodeCount = triggerConditionTypeSimpleText.Split(' ').Length;
						AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[triggerConditionTypeNodeCount - 1]), "OtherKeyword", triggerConditionType);
						int triggerConditionNodeCount;
						triggerConditionNodeCount = triggerConditions.Groups[2].Value.Split(' ').Length - 2;
						AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[triggerConditionTypeNodeCount - 1] + 1, significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1]), "OtherKeyword", sqlTree.CurrentContainer);
						tokenID = significantTokenPositions[triggerConditionTypeNodeCount + triggerConditionNodeCount - 1];
						sqlTree.MoveToAncestorContainer(1, "DDLProceduralBlock");
						break;
					}
					if (significantTokensString.StartsWith("FOR "))
					{
						sqlTree.EscapeAnyBetweenConditions();
						sqlTree.EscapeAnySelectionTarget();
						sqlTree.EscapeJoinCondition();
						if (sqlTree.PathNameMatches(0, "CursorDeclaration"))
						{
							sqlTree.StartNewContainer("CursorForBlock", token.Value, "ContainerContentBody");
							sqlTree.StartNewStatement();
						}
						else if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && sqlTree.PathNameMatches(2, "ContainerContentBody") && sqlTree.PathNameMatches(3, "CursorForBlock"))
						{
							sqlTree.MoveToAncestorContainer(4, "CursorDeclaration");
							sqlTree.StartNewContainer("CursorForOptions", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.ConsiderStartingNewClause();
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("DECLARE "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLDeclareBlock", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("CREATE ") || significantTokensString.StartsWith("ALTER "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("DDLOtherBlock", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("GRANT ") || significantTokensString.StartsWith("DENY ") || significantTokensString.StartsWith("REVOKE "))
					{
						if (significantTokensString.StartsWith("GRANT ") && sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "DDLWith") && sqlTree.PathNameMatches(2, "PermissionsBlock") && sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer) == null)
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
							break;
						}
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.StartNewContainer("PermissionsBlock", token.Value, "PermissionsDetail");
						break;
					}
					if (sqlTree.CurrentContainer.Name.Equals("DDLProceduralBlock") && significantTokensString.StartsWith("RETURNS "))
					{
						sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("DDLReturns", ""));
						break;
					}
					if (significantTokensString.StartsWith("AS "))
					{
						if (sqlTree.PathNameMatches(0, "DDLProceduralBlock"))
						{
							bool isDataTypeDefinition;
							isDataTypeDefinition = false;
							if (significantTokenPositions.Count > 1 && KeywordList.TryGetValue(tokenList[significantTokenPositions[1]].Value.ToUpperInvariant(), out var nextKeywordType) && nextKeywordType == KeywordType.DataTypeKeyword)
							{
								isDataTypeDefinition = true;
							}
							if (isDataTypeDefinition)
							{
								sqlTree.SaveNewElement("OtherKeyword", token.Value);
								break;
							}
							sqlTree.StartNewContainer("DDLAsBlock", token.Value, "ContainerContentBody");
							sqlTree.StartNewStatement();
						}
						else if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "DDLWith") && sqlTree.PathNameMatches(2, "DDLProceduralBlock"))
						{
							sqlTree.MoveToAncestorContainer(2, "DDLProceduralBlock");
							sqlTree.StartNewContainer("DDLAsBlock", token.Value, "ContainerContentBody");
							sqlTree.StartNewStatement();
						}
						else if (sqlTree.PathNameMatches(0, "CTEAlias") && sqlTree.PathNameMatches(1, "CTEWithClause"))
						{
							sqlTree.MoveToAncestorContainer(1, "CTEWithClause");
							sqlTree.StartNewContainer("CTEAsBlock", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("BEGIN DISTRIBUTED TRANSACTION ") || significantTokensString.StartsWith("BEGIN DISTRIBUTED TRAN "))
					{
						sqlTree.ConsiderStartingNewStatement();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("BeginTransaction", ""), ref tokenID, significantTokenPositions, 3);
						break;
					}
					if (significantTokensString.StartsWith("BEGIN TRANSACTION ") || significantTokensString.StartsWith("BEGIN TRAN "))
					{
						sqlTree.ConsiderStartingNewStatement();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("BeginTransaction", ""), ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("SAVE TRANSACTION ") || significantTokensString.StartsWith("SAVE TRAN "))
					{
						sqlTree.ConsiderStartingNewStatement();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("SaveTransaction", ""), ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("COMMIT TRANSACTION ") || significantTokensString.StartsWith("COMMIT TRAN ") || significantTokensString.StartsWith("COMMIT WORK "))
					{
						sqlTree.ConsiderStartingNewStatement();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("CommitTransaction", ""), ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("COMMIT "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("CommitTransaction", token.Value));
						break;
					}
					if (significantTokensString.StartsWith("ROLLBACK TRANSACTION ") || significantTokensString.StartsWith("ROLLBACK TRAN ") || significantTokensString.StartsWith("ROLLBACK WORK "))
					{
						sqlTree.ConsiderStartingNewStatement();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("RollbackTransaction", ""), ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("ROLLBACK "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("RollbackTransaction", token.Value));
						break;
					}
					if (significantTokensString.StartsWith("BEGIN TRY "))
					{
						sqlTree.ConsiderStartingNewStatement();
						Node newTryBlock;
						newTryBlock = sqlTree.SaveNewElement("TryBlock", "");
						Node tryContainerOpen;
						tryContainerOpen = sqlTree.SaveNewElement("ContainerOpen", "", newTryBlock);
						ProcessCompoundKeyword(tokenList, sqlTree, tryContainerOpen, ref tokenID, significantTokenPositions, 2);
						Node tryMultiContainer;
						tryMultiContainer = sqlTree.SaveNewElement("ContainerMultiStatementBody", "", newTryBlock);
						sqlTree.StartNewStatement(tryMultiContainer);
						break;
					}
					if (significantTokensString.StartsWith("BEGIN CATCH "))
					{
						sqlTree.ConsiderStartingNewStatement();
						Node newCatchBlock;
						newCatchBlock = sqlTree.SaveNewElement("CatchBlock", "");
						Node catchContainerOpen;
						catchContainerOpen = sqlTree.SaveNewElement("ContainerOpen", "", newCatchBlock);
						ProcessCompoundKeyword(tokenList, sqlTree, catchContainerOpen, ref tokenID, significantTokenPositions, 2);
						Node catchMultiContainer;
						catchMultiContainer = sqlTree.SaveNewElement("ContainerMultiStatementBody", "", newCatchBlock);
						sqlTree.StartNewStatement(catchMultiContainer);
						break;
					}
					if (significantTokensString.StartsWith("BEGIN "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.StartNewContainer("BeginEndBlock", token.Value, "ContainerMultiStatementBody");
						sqlTree.StartNewStatement();
						break;
					}
					if (significantTokensString.StartsWith("MERGE "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.ConsiderStartingNewClause();
						sqlTree.StartNewContainer("MergeClause", token.Value, "MergeTarget");
						break;
					}
					if (significantTokensString.StartsWith("USING "))
					{
						if (sqlTree.PathNameMatches(0, "MergeTarget"))
						{
							sqlTree.MoveToAncestorContainer(1, "MergeClause");
							sqlTree.StartNewContainer("MergeUsing", token.Value, "SelectionTarget");
						}
						else
						{
							sqlTree.SaveNewElementWithError("Other", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("ON "))
					{
						sqlTree.EscapeAnySelectionTarget();
						if (sqlTree.PathNameMatches(0, "MergeUsing"))
						{
							sqlTree.MoveToAncestorContainer(1, "MergeClause");
							sqlTree.StartNewContainer("MergeCondition", token.Value, "ContainerContentBody");
						}
						else if (!sqlTree.PathNameMatches(0, "DDLProceduralBlock") && !sqlTree.PathNameMatches(0, "DDLOtherBlock") && !sqlTree.PathNameMatches(1, "DDLWith") && !sqlTree.PathNameMatches(0, "ExpressionParens") && !ContentStartsWithKeyword(sqlTree.CurrentContainer, "SET"))
						{
							sqlTree.StartNewContainer("JoinOn", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("CASE "))
					{
						sqlTree.StartNewContainer("CaseStatement", token.Value, "Input");
						break;
					}
					if (significantTokensString.StartsWith("WHEN "))
					{
						sqlTree.EscapeMergeAction();
						if (sqlTree.PathNameMatches(0, "Input") || (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "Then")))
						{
							if (sqlTree.PathNameMatches(0, "Input"))
							{
								sqlTree.MoveToAncestorContainer(1, "CaseStatement");
							}
							else
							{
								sqlTree.MoveToAncestorContainer(3, "CaseStatement");
							}
							sqlTree.StartNewContainer("When", token.Value, "ContainerContentBody");
						}
						else if ((sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "MergeCondition")) || sqlTree.PathNameMatches(0, "MergeWhen"))
						{
							if (sqlTree.PathNameMatches(1, "MergeCondition"))
							{
								sqlTree.MoveToAncestorContainer(2, "MergeClause");
							}
							else
							{
								sqlTree.MoveToAncestorContainer(1, "MergeClause");
							}
							sqlTree.StartNewContainer("MergeWhen", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.SaveNewElementWithError("Other", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("THEN "))
					{
						sqlTree.EscapeAnyBetweenConditions();
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "When"))
						{
							sqlTree.MoveToAncestorContainer(1, "When");
							sqlTree.StartNewContainer("Then", token.Value, "ContainerContentBody");
						}
						else if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "MergeWhen"))
						{
							sqlTree.MoveToAncestorContainer(1, "MergeWhen");
							sqlTree.StartNewContainer("MergeThen", token.Value, "MergeAction");
							sqlTree.StartNewStatement();
						}
						else
						{
							sqlTree.SaveNewElementWithError("Other", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("OUTPUT "))
					{
						bool isSprocArgument;
						isSprocArgument = false;
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXEC") || ContentStartsWithKeyword(sqlTree.CurrentContainer, "EXECUTE") || ContentStartsWithKeyword(sqlTree.CurrentContainer, null)))
						{
							isSprocArgument = true;
						}
						if (sqlTree.PathNameMatches(0, "DDLProceduralBlock"))
						{
							isSprocArgument = true;
						}
						if (!isSprocArgument)
						{
							sqlTree.EscapeMergeAction();
							sqlTree.ConsiderStartingNewClause();
						}
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("OPTION "))
					{
						if (!sqlTree.PathNameMatches(0, "ContainerContentBody") || !sqlTree.PathNameMatches(1, "DDLWith"))
						{
							sqlTree.EscapeMergeAction();
							sqlTree.ConsiderStartingNewClause();
						}
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("END TRY "))
					{
						sqlTree.EscapeAnySingleOrPartialStatementContainers();
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && sqlTree.PathNameMatches(2, "ContainerMultiStatementBody") && sqlTree.PathNameMatches(3, "TryBlock"))
						{
							Node tryBlock;
							tryBlock = sqlTree.CurrentContainer.Parent.Parent.Parent;
							Node tryContainerClose;
							tryContainerClose = sqlTree.SaveNewElement("ContainerClose", "", tryBlock);
							ProcessCompoundKeyword(tokenList, sqlTree, tryContainerClose, ref tokenID, significantTokenPositions, 2);
							sqlTree.CurrentContainer = tryBlock.Parent;
						}
						else
						{
							ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						}
						break;
					}
					if (significantTokensString.StartsWith("END CATCH "))
					{
						sqlTree.EscapeAnySingleOrPartialStatementContainers();
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && sqlTree.PathNameMatches(2, "ContainerMultiStatementBody") && sqlTree.PathNameMatches(3, "CatchBlock"))
						{
							Node catchBlock;
							catchBlock = sqlTree.CurrentContainer.Parent.Parent.Parent;
							Node catchContainerClose;
							catchContainerClose = sqlTree.SaveNewElement("ContainerClose", "", catchBlock);
							ProcessCompoundKeyword(tokenList, sqlTree, catchContainerClose, ref tokenID, significantTokenPositions, 2);
							sqlTree.CurrentContainer = catchBlock.Parent;
						}
						else
						{
							ProcessCompoundKeywordWithError(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						}
						break;
					}
					if (significantTokensString.StartsWith("END "))
					{
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "Then"))
						{
							sqlTree.MoveToAncestorContainer(3, "CaseStatement");
							sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("ContainerClose", ""));
							sqlTree.MoveToAncestorContainer(1);
							break;
						}
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "CaseElse"))
						{
							sqlTree.MoveToAncestorContainer(2, "CaseStatement");
							sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("ContainerClose", ""));
							sqlTree.MoveToAncestorContainer(1);
							break;
						}
						sqlTree.EscapeAnySingleOrPartialStatementContainers();
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && sqlTree.PathNameMatches(2, "ContainerMultiStatementBody") && sqlTree.PathNameMatches(3, "BeginEndBlock"))
						{
							Node beginBlock;
							beginBlock = sqlTree.CurrentContainer.Parent.Parent.Parent;
							Node beginContainerClose;
							beginContainerClose = sqlTree.SaveNewElement("ContainerClose", "", beginBlock);
							sqlTree.SaveNewElement("OtherKeyword", token.Value, beginContainerClose);
							sqlTree.CurrentContainer = beginBlock.Parent;
						}
						else
						{
							sqlTree.SaveNewElementWithError("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("GO "))
					{
						sqlTree.EscapeAnySingleOrPartialStatementContainers();
						if ((tokenID == 0 || IsLineBreakingWhiteSpaceOrComment(tokenList[tokenID - 1])) && IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(tokenList, tokenID))
						{
							if (sqlTree.FindValidBatchEnd())
							{
								Node sqlRoot;
								sqlRoot = sqlTree.RootContainer();
								Node batchSeparator;
								batchSeparator = sqlTree.SaveNewElement("BatchSeparator", "", sqlRoot);
								sqlTree.SaveNewElement("OtherKeyword", token.Value, batchSeparator);
								sqlTree.StartNewStatement(sqlRoot);
							}
							else
							{
								sqlTree.SaveNewElementWithError("OtherKeyword", token.Value);
							}
						}
						else
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("EXECUTE AS "))
					{
						bool executeAsInWithOptions;
						executeAsInWithOptions = false;
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "DDLWith") && (IsLatestTokenAComma(sqlTree) || !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer)))
						{
							executeAsInWithOptions = true;
						}
						if (!executeAsInWithOptions)
						{
							sqlTree.ConsiderStartingNewStatement();
							sqlTree.ConsiderStartingNewClause();
						}
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("EXEC ") || significantTokensString.StartsWith("EXECUTE "))
					{
						bool execShouldntTryToStartNewStatement;
						execShouldntTryToStartNewStatement = false;
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && (ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT") || ContentStartsWithKeyword(sqlTree.CurrentContainer, "INSERT INTO")))
						{
							int existingClauseCount;
							existingClauseCount = ((sqlTree.CurrentContainer.Parent != null) ? sqlTree.CurrentContainer.Parent.ChildrenByName("Clause").Count() : 0);
							if (existingClauseCount == 1)
							{
								execShouldntTryToStartNewStatement = true;
							}
						}
						if (!execShouldntTryToStartNewStatement)
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						sqlTree.ConsiderStartingNewClause();
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (_JoinDetector.IsMatch(significantTokensString))
					{
						sqlTree.ConsiderStartingNewClause();
						string joinText;
						joinText = _JoinDetector.Match(significantTokensString).Value;
						int targetKeywordCount;
						targetKeywordCount = joinText.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, targetKeywordCount);
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("SelectionTarget", "");
						break;
					}
					if (significantTokensString.StartsWith("UNION ALL "))
					{
						sqlTree.ConsiderStartingNewClause();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.SaveNewElement("SetOperatorClause", ""), ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("UNION ") || significantTokensString.StartsWith("INTERSECT ") || significantTokensString.StartsWith("EXCEPT "))
					{
						sqlTree.ConsiderStartingNewClause();
						Node unionClause;
						unionClause = sqlTree.SaveNewElement("SetOperatorClause", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value, unionClause);
						break;
					}
					if (significantTokensString.StartsWith("WHILE "))
					{
						sqlTree.ConsiderStartingNewStatement();
						Node newWhileLoop;
						newWhileLoop = sqlTree.SaveNewElement("WhileLoop", "");
						Node whileContainerOpen;
						whileContainerOpen = sqlTree.SaveNewElement("ContainerOpen", "", newWhileLoop);
						sqlTree.SaveNewElement("OtherKeyword", token.Value, whileContainerOpen);
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("BooleanExpression", "", newWhileLoop);
						break;
					}
					if (significantTokensString.StartsWith("IF "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.StartNewContainer("IfStatement", token.Value, "BooleanExpression");
						break;
					}
					if (significantTokensString.StartsWith("ELSE "))
					{
						sqlTree.EscapeAnyBetweenConditions();
						sqlTree.EscapeAnySelectionTarget();
						sqlTree.EscapeJoinCondition();
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "Then"))
						{
							sqlTree.MoveToAncestorContainer(3, "CaseStatement");
							sqlTree.StartNewContainer("CaseElse", token.Value, "ContainerContentBody");
							break;
						}
						sqlTree.EscapePartialStatementContainers();
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && sqlTree.PathNameMatches(2, "ContainerSingleStatementBody"))
						{
							Node currentNode;
							currentNode = sqlTree.CurrentContainer.Parent.Parent;
							bool stopSearching;
							stopSearching = false;
							while (!stopSearching)
							{
								if (sqlTree.PathNameMatches(currentNode, 1, "IfStatement"))
								{
									sqlTree.CurrentContainer = currentNode.Parent;
									sqlTree.StartNewContainer("ElseClause", token.Value, "ContainerSingleStatementBody");
									sqlTree.StartNewStatement();
									stopSearching = true;
								}
								else if (sqlTree.PathNameMatches(currentNode, 1, "ElseClause"))
								{
									currentNode = currentNode.Parent.Parent.Parent.Parent.Parent;
								}
								else if (sqlTree.PathNameMatches(currentNode, 1, "WhileLoop"))
								{
									currentNode = currentNode.Parent.Parent.Parent.Parent;
								}
								else
								{
									sqlTree.SaveNewElementWithError("OtherKeyword", token.Value);
									stopSearching = true;
								}
							}
						}
						else
						{
							sqlTree.SaveNewElementWithError("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("INSERT INTO "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.ConsiderStartingNewClause();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("NATIONAL CHARACTER VARYING "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
						break;
					}
					if (significantTokensString.StartsWith("NATIONAL CHAR VARYING "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 3);
						break;
					}
					if (significantTokensString.StartsWith("BINARY VARYING "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("CHAR VARYING "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("CHARACTER VARYING "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("DOUBLE PRECISION "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("NATIONAL CHARACTER "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("NATIONAL CHAR "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("NATIONAL TEXT "))
					{
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("INSERT "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.ConsiderStartingNewClause();
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("BULK INSERT "))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.ConsiderStartingNewClause();
						ProcessCompoundKeyword(tokenList, sqlTree, sqlTree.CurrentContainer, ref tokenID, significantTokenPositions, 2);
						break;
					}
					if (significantTokensString.StartsWith("SELECT "))
					{
						if (sqlTree.NewStatementDue)
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						bool selectShouldntTryToStartNewStatement;
						selectShouldntTryToStartNewStatement = false;
						if (sqlTree.PathNameMatches(0, "Clause"))
						{
							Node firstStatementClause;
							firstStatementClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer.Parent);
							bool isPrecededByInsertStatement;
							isPrecededByInsertStatement = false;
							foreach (Node clause4 in sqlTree.CurrentContainer.Parent.ChildrenByName("Clause"))
							{
								if (ContentStartsWithKeyword(clause4, "INSERT"))
								{
									isPrecededByInsertStatement = true;
								}
							}
							if (isPrecededByInsertStatement)
							{
								bool existingSelectClauseFound;
								existingSelectClauseFound = false;
								foreach (Node clause3 in sqlTree.CurrentContainer.Parent.ChildrenByName("Clause"))
								{
									if (ContentStartsWithKeyword(clause3, "SELECT"))
									{
										existingSelectClauseFound = true;
									}
								}
								bool existingValuesClauseFound;
								existingValuesClauseFound = false;
								foreach (Node clause2 in sqlTree.CurrentContainer.Parent.ChildrenByName("Clause"))
								{
									if (ContentStartsWithKeyword(clause2, "VALUES"))
									{
										existingValuesClauseFound = true;
									}
								}
								bool existingExecClauseFound;
								existingExecClauseFound = false;
								foreach (Node clause in sqlTree.CurrentContainer.Parent.ChildrenByName("Clause"))
								{
									if (ContentStartsWithKeyword(clause, "EXEC") || ContentStartsWithKeyword(clause, "EXECUTE"))
									{
										existingExecClauseFound = true;
									}
								}
								if (!existingSelectClauseFound && !existingValuesClauseFound && !existingExecClauseFound)
								{
									selectShouldntTryToStartNewStatement = true;
								}
							}
							Node firstEntryOfThisClause;
							firstEntryOfThisClause = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
							if (firstEntryOfThisClause != null && firstEntryOfThisClause.Name.Equals("SetOperatorClause"))
							{
								selectShouldntTryToStartNewStatement = true;
							}
						}
						if (!selectShouldntTryToStartNewStatement)
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						sqlTree.ConsiderStartingNewClause();
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("UPDATE "))
					{
						if (sqlTree.NewStatementDue)
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						if (!sqlTree.PathNameMatches(0, "ContainerContentBody") || !sqlTree.PathNameMatches(1, "CursorForOptions"))
						{
							sqlTree.ConsiderStartingNewStatement();
							sqlTree.ConsiderStartingNewClause();
						}
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("TO "))
					{
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "PermissionsTarget"))
						{
							sqlTree.MoveToAncestorContainer(2, "PermissionsBlock");
							sqlTree.StartNewContainer("PermissionsRecipient", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (significantTokensString.StartsWith("FROM "))
					{
						if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "PermissionsTarget"))
						{
							sqlTree.MoveToAncestorContainer(2, "PermissionsBlock");
							sqlTree.StartNewContainer("PermissionsRecipient", token.Value, "ContainerContentBody");
						}
						else
						{
							sqlTree.ConsiderStartingNewClause();
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
							sqlTree.CurrentContainer = sqlTree.SaveNewElement("SelectionTarget", "");
						}
						break;
					}
					if (significantTokensString.StartsWith("CASCADE ") && sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "PermissionsRecipient"))
					{
						sqlTree.MoveToAncestorContainer(2, "PermissionsBlock");
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("ContainerContentBody", "", sqlTree.SaveNewElement("DDLWith", ""));
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("SET "))
					{
						Node firstNonCommentSibling2;
						firstNonCommentSibling2 = sqlTree.GetFirstNonWhitespaceNonCommentChildElement(sqlTree.CurrentContainer);
						if (firstNonCommentSibling2 == null || !firstNonCommentSibling2.Name.Equals("OtherKeyword") || !firstNonCommentSibling2.TextValue.ToUpperInvariant().StartsWith("UPDATE"))
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						sqlTree.ConsiderStartingNewClause();
						sqlTree.SaveNewElement("OtherKeyword", token.Value);
						break;
					}
					if (significantTokensString.StartsWith("BETWEEN "))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("Between", "");
						sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("ContainerOpen", ""));
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("LowerBound", "");
						break;
					}
					if (significantTokensString.StartsWith("AND "))
					{
						if (sqlTree.PathNameMatches(0, "LowerBound"))
						{
							sqlTree.MoveToAncestorContainer(1, "Between");
							sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("ContainerClose", ""));
							sqlTree.CurrentContainer = sqlTree.SaveNewElement("UpperBound", "");
						}
						else
						{
							sqlTree.EscapeAnyBetweenConditions();
							sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("And", ""));
						}
						break;
					}
					if (significantTokensString.StartsWith("OR "))
					{
						sqlTree.EscapeAnyBetweenConditions();
						sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("Or", ""));
						break;
					}
					if (significantTokensString.StartsWith("WITH "))
					{
						if (sqlTree.NewStatementDue)
						{
							sqlTree.ConsiderStartingNewStatement();
						}
						if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && !sqlTree.HasNonWhiteSpaceNonCommentContent(sqlTree.CurrentContainer))
						{
							sqlTree.CurrentContainer = sqlTree.SaveNewElement("CTEWithClause", "");
							sqlTree.SaveNewElement("OtherKeyword", token.Value, sqlTree.SaveNewElement("ContainerOpen", ""));
							sqlTree.CurrentContainer = sqlTree.SaveNewElement("CTEAlias", "");
						}
						else if (sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "PermissionsRecipient"))
						{
							sqlTree.MoveToAncestorContainer(2, "PermissionsBlock");
							sqlTree.StartNewContainer("DDLWith", token.Value, "ContainerContentBody");
						}
						else if (sqlTree.PathNameMatches(0, "DDLProceduralBlock") || sqlTree.PathNameMatches(0, "DDLOtherBlock"))
						{
							sqlTree.StartNewContainer("DDLWith", token.Value, "ContainerContentBody");
						}
						else if (sqlTree.PathNameMatches(0, "SelectionTarget"))
						{
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						else
						{
							sqlTree.ConsiderStartingNewClause();
							sqlTree.SaveNewElement("OtherKeyword", token.Value);
						}
						break;
					}
					if (tokenList.Count > tokenID + 1 && tokenList[tokenID + 1].Type == SqlTokenType.Colon && (tokenList.Count <= tokenID + 2 || tokenList[tokenID + 2].Type != SqlTokenType.Colon))
					{
						sqlTree.ConsiderStartingNewStatement();
						sqlTree.SaveNewElement("Label", token.Value + tokenList[tokenID + 1].Value);
						tokenID++;
						break;
					}
					if (IsStatementStarter(token) || sqlTree.NewStatementDue)
					{
						sqlTree.ConsiderStartingNewStatement();
					}
					if (IsClauseStarter(token))
					{
						sqlTree.ConsiderStartingNewClause();
					}
					string newNodeName;
					newNodeName = "Other";
					if (KeywordList.TryGetValue(token.Value.ToUpperInvariant(), out var matchedKeywordType))
					{
						switch (matchedKeywordType)
						{
						case KeywordType.OperatorKeyword:
							newNodeName = "AlphaOperator";
							break;
						case KeywordType.FunctionKeyword:
							newNodeName = "FunctionKeyword";
							break;
						case KeywordType.DataTypeKeyword:
							newNodeName = "DataTypeKeyword";
							break;
						case KeywordType.OtherKeyword:
							sqlTree.EscapeAnySelectionTarget();
							newNodeName = "OtherKeyword";
							break;
						default:
							throw new Exception("Unrecognized Keyword Type!");
						}
					}
					sqlTree.SaveNewElement(newNodeName, token.Value);
					break;
				}
				case SqlTokenType.Semicolon:
					sqlTree.SaveNewElement("Semicolon", token.Value);
					sqlTree.NewStatementDue = true;
					break;
				case SqlTokenType.Colon:
					if (tokenList.Count > tokenID + 1 && tokenList[tokenID + 1].Type == SqlTokenType.Colon)
					{
						sqlTree.SaveNewElement("ScopeResolutionOperator", token.Value + tokenList[tokenID + 1].Value);
						tokenID++;
					}
					else if (tokenList.Count > tokenID + 1 && tokenList[tokenID + 1].Type == SqlTokenType.OtherNode)
					{
						sqlTree.SaveNewElement("Other", token.Value + tokenList[tokenID + 1].Value);
						tokenID++;
					}
					else
					{
						sqlTree.SaveNewElementWithError("OtherOperator", token.Value);
					}
					break;
				case SqlTokenType.Comma:
				{
					bool isCTESplitter;
					isCTESplitter = sqlTree.PathNameMatches(0, "ContainerContentBody") && sqlTree.PathNameMatches(1, "CTEWithClause");
					sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
					if (isCTESplitter)
					{
						sqlTree.MoveToAncestorContainer(1, "CTEWithClause");
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("CTEAlias", "");
					}
					break;
				}
				case SqlTokenType.EqualsSign:
					sqlTree.SaveNewElement("EqualsSign", token.Value);
					if (sqlTree.PathNameMatches(0, "DDLDeclareBlock"))
					{
						sqlTree.CurrentContainer = sqlTree.SaveNewElement("ContainerContentBody", "");
					}
					break;
				case SqlTokenType.WhiteSpace:
				case SqlTokenType.SingleLineComment:
				case SqlTokenType.SingleLineCommentCStyle:
				case SqlTokenType.MultiLineComment:
					if (sqlTree.PathNameMatches(0, "Clause") && sqlTree.PathNameMatches(1, "SqlStatement") && !sqlTree.CurrentContainer.Children.Any())
					{
						sqlTree.SaveNewElementAsPriorSibling(GetEquivalentSqlNodeName(token.Type), token.Value, sqlTree.CurrentContainer);
					}
					else
					{
						sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
					}
					break;
				case SqlTokenType.String:
				case SqlTokenType.NationalString:
				case SqlTokenType.BracketQuotedName:
				case SqlTokenType.QuotedString:
				case SqlTokenType.Period:
				case SqlTokenType.Asterisk:
				case SqlTokenType.MonetaryValue:
				case SqlTokenType.Number:
				case SqlTokenType.BinaryValue:
				case SqlTokenType.OtherOperator:
				case SqlTokenType.PseudoName:
					sqlTree.SaveNewElement(GetEquivalentSqlNodeName(token.Type), token.Value);
					break;
				default:
					throw new Exception("Unrecognized element encountered!");
				}
			}
			if (tokenList.HasUnfinishedToken)
			{
				sqlTree.SetError();
			}
			if (!sqlTree.FindValidBatchEnd())
			{
				sqlTree.SetError();
			}
			return sqlTree;
		}

		private static bool ContentStartsWithKeyword(Node providedContainer, string contentToMatch)
		{
			var firstEntryOfProvidedContainer = ((ParseTree)providedContainer.RootContainer()).GetFirstNonWhitespaceNonCommentChildElement(providedContainer);
			// bool targetFound;
			// targetFound = false;
			string keywordUpperValue = null;
			if (firstEntryOfProvidedContainer != null && firstEntryOfProvidedContainer.Name.Equals("OtherKeyword") && firstEntryOfProvidedContainer.TextValue != null)
			{
				keywordUpperValue = firstEntryOfProvidedContainer.TextValue.ToUpperInvariant();
			}
			if (firstEntryOfProvidedContainer != null && firstEntryOfProvidedContainer.Name.Equals("CompoundKeyword"))
			{
				keywordUpperValue = firstEntryOfProvidedContainer.GetAttributeValue("simpleText");
			}
			if (keywordUpperValue != null)
			{
				return keywordUpperValue.Equals(contentToMatch) || keywordUpperValue.StartsWith(contentToMatch + " ");
			}
			return contentToMatch == null;
		}

		private void ProcessCompoundKeywordWithError(ITokenList tokenList, ParseTree sqlTree, Node currentContainerElement, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
		{
			ProcessCompoundKeyword(tokenList, sqlTree, currentContainerElement, ref tokenID, significantTokenPositions, keywordCount);
			sqlTree.SetError();
		}

		private void ProcessCompoundKeyword(ITokenList tokenList, ParseTree sqlTree, Node targetContainer, ref int tokenID, List<int> significantTokenPositions, int keywordCount)
		{
			Node compoundKeyword;
			compoundKeyword = sqlTree.SaveNewElement("CompoundKeyword", "", targetContainer);
			string targetText;
			targetText = ExtractTokensString(tokenList, significantTokenPositions.GetRange(0, keywordCount)).TrimEnd();
			compoundKeyword.SetAttribute("simpleText", targetText);
			AppendNodesWithMapping(sqlTree, tokenList.GetRangeByIndex(significantTokenPositions[0], significantTokenPositions[keywordCount - 1]), "OtherKeyword", compoundKeyword);
			tokenID = significantTokenPositions[keywordCount - 1];
		}

		private void AppendNodesWithMapping(ParseTree sqlTree, IEnumerable<IToken> tokens, string otherTokenMappingName, Node targetContainer)
		{
			foreach (IToken token in tokens)
			{
				string elementName;
				elementName = ((token.Type != SqlTokenType.OtherNode) ? GetEquivalentSqlNodeName(token.Type) : otherTokenMappingName);
				sqlTree.SaveNewElement(elementName, token.Value, targetContainer);
			}
		}

		private string ExtractTokensString(ITokenList tokenList, IList<int> significantTokenPositions)
		{
			StringBuilder keywordSB;
			keywordSB = new StringBuilder();
			foreach (int tokenPos in significantTokenPositions)
			{
				if (tokenList[tokenPos].Type == SqlTokenType.Comma)
				{
					keywordSB.Append(",");
				}
				else
				{
					keywordSB.Append(tokenList[tokenPos].Value.ToUpperInvariant());
				}
				keywordSB.Append(" ");
			}
			return keywordSB.ToString();
		}

		private string GetEquivalentSqlNodeName(SqlTokenType tokenType)
		{
			return tokenType switch
			{
				SqlTokenType.WhiteSpace => "WhiteSpace",
				SqlTokenType.SingleLineComment => "SingleLineComment",
				SqlTokenType.SingleLineCommentCStyle => "SingleLineCommentCStyle",
				SqlTokenType.MultiLineComment => "MultiLineComment",
				SqlTokenType.BracketQuotedName => "BracketQuotedName",
				SqlTokenType.Asterisk => "Asterisk",
				SqlTokenType.EqualsSign => "EqualsSign",
				SqlTokenType.Comma => "Comma",
				SqlTokenType.Period => "Period",
				SqlTokenType.NationalString => "NationalString",
				SqlTokenType.String => "String",
				SqlTokenType.QuotedString => "QuotedString",
				SqlTokenType.OtherOperator => "OtherOperator",
				SqlTokenType.Number => "NumberValue",
				SqlTokenType.MonetaryValue => "MonetaryValue",
				SqlTokenType.BinaryValue => "BinaryValue",
				SqlTokenType.PseudoName => "PseudoName",
				_ => throw new Exception("Mapping not found for provided Token Type"),
			};
		}

		private string GetKeywordMatchPhrase(ITokenList tokenList, int tokenID, ref List<string> rawKeywordParts, ref List<int> tokenCounts, ref List<List<IToken>> overflowNodes)
		{
			string phrase;
			phrase = "";
			int phraseComponentsFound;
			phraseComponentsFound = 0;
			rawKeywordParts = new List<string>();
			overflowNodes = new List<List<IToken>>();
			tokenCounts = new List<int>();
			string precedingWhitespace;
			precedingWhitespace = "";
			int originalTokenID;
			originalTokenID = tokenID;
			while (tokenID < tokenList.Count && phraseComponentsFound < 7 && (tokenList[tokenID].Type == SqlTokenType.OtherNode || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName || tokenList[tokenID].Type == SqlTokenType.Comma))
			{
				phrase = phrase + tokenList[tokenID].Value.ToUpperInvariant() + " ";
				phraseComponentsFound++;
				rawKeywordParts.Add(precedingWhitespace + tokenList[tokenID].Value);
				tokenID++;
				tokenCounts.Add(tokenID - originalTokenID);
				overflowNodes.Add(new List<IToken>());
				precedingWhitespace = "";
				while (tokenID < tokenList.Count && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace || tokenList[tokenID].Type == SqlTokenType.SingleLineComment || tokenList[tokenID].Type == SqlTokenType.MultiLineComment))
				{
					if (tokenList[tokenID].Type == SqlTokenType.WhiteSpace)
					{
						precedingWhitespace += tokenList[tokenID].Value;
					}
					else
					{
						overflowNodes[phraseComponentsFound - 1].Add(tokenList[tokenID]);
					}
					tokenID++;
				}
			}
			return phrase;
		}

		private List<int> GetSignificantTokenPositions(ITokenList tokenList, int tokenID, int searchDistance)
		{
			List<int> significantTokenPositions;
			significantTokenPositions = new List<int>();
			int originalTokenID;
			originalTokenID = tokenID;
			while (tokenID < tokenList.Count && significantTokenPositions.Count < searchDistance && (tokenList[tokenID].Type == SqlTokenType.OtherNode || tokenList[tokenID].Type == SqlTokenType.BracketQuotedName || tokenList[tokenID].Type == SqlTokenType.Comma))
			{
				significantTokenPositions.Add(tokenID);
				tokenID++;
				while (tokenID < tokenList.Count && (tokenList[tokenID].Type == SqlTokenType.WhiteSpace || tokenList[tokenID].Type == SqlTokenType.SingleLineComment || tokenList[tokenID].Type == SqlTokenType.MultiLineComment))
				{
					tokenID++;
				}
			}
			return significantTokenPositions;
		}

		private Node ProcessCompoundKeyword(ParseTree sqlTree, string newElementName, ref int tokenID, Node currentContainerElement, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
		{
			Node newElement;
			newElement = NodeFactory.CreateNode(newElementName, GetCompoundKeyword(ref tokenID, compoundKeywordCount, compoundKeywordTokenCounts, compoundKeywordRawStrings));
			sqlTree.CurrentContainer.AddChild(newElement);
			return newElement;
		}

		private string GetCompoundKeyword(ref int tokenID, int compoundKeywordCount, List<int> compoundKeywordTokenCounts, List<string> compoundKeywordRawStrings)
		{
			tokenID += compoundKeywordTokenCounts[compoundKeywordCount - 1] - 1;
			string outString;
			outString = "";
			for (int i = 0; i < compoundKeywordCount; i++)
			{
				outString += compoundKeywordRawStrings[i];
			}
			return outString;
		}

		private static bool IsStatementStarter(IToken token)
		{
			string uppercaseValue;
			uppercaseValue = token.Value.ToUpperInvariant();
			return token.Type == SqlTokenType.OtherNode && (uppercaseValue.Equals("ALTER") || uppercaseValue.Equals("BACKUP") || uppercaseValue.Equals("BREAK") || uppercaseValue.Equals("CLOSE") || uppercaseValue.Equals("CHECKPOINT") || uppercaseValue.Equals("COMMIT") || uppercaseValue.Equals("CONTINUE") || uppercaseValue.Equals("CREATE") || uppercaseValue.Equals("DBCC") || uppercaseValue.Equals("DEALLOCATE") || uppercaseValue.Equals("DELETE") || uppercaseValue.Equals("DECLARE") || uppercaseValue.Equals("DENY") || uppercaseValue.Equals("DROP") || uppercaseValue.Equals("EXEC") || uppercaseValue.Equals("EXECUTE") || uppercaseValue.Equals("FETCH") || uppercaseValue.Equals("GOTO") || uppercaseValue.Equals("GRANT") || uppercaseValue.Equals("IF") || uppercaseValue.Equals("INSERT") || uppercaseValue.Equals("KILL") || uppercaseValue.Equals("MERGE") || uppercaseValue.Equals("OPEN") || uppercaseValue.Equals("PRINT") || uppercaseValue.Equals("RAISERROR") || uppercaseValue.Equals("RECONFIGURE") || uppercaseValue.Equals("RESTORE") || uppercaseValue.Equals("RETURN") || uppercaseValue.Equals("REVERT") || uppercaseValue.Equals("REVOKE") || uppercaseValue.Equals("SELECT") || uppercaseValue.Equals("SET") || uppercaseValue.Equals("SETUSER") || uppercaseValue.Equals("SHUTDOWN") || uppercaseValue.Equals("TRUNCATE") || uppercaseValue.Equals("UPDATE") || uppercaseValue.Equals("USE") || uppercaseValue.Equals("WAITFOR") || uppercaseValue.Equals("WHILE"));
		}

		private static bool IsClauseStarter(IToken token)
		{
			string uppercaseValue;
			uppercaseValue = token.Value.ToUpperInvariant();
			return token.Type == SqlTokenType.OtherNode && (uppercaseValue.Equals("DELETE") || uppercaseValue.Equals("EXCEPT") || uppercaseValue.Equals("FOR") || uppercaseValue.Equals("FROM") || uppercaseValue.Equals("GROUP") || uppercaseValue.Equals("HAVING") || uppercaseValue.Equals("INNER") || uppercaseValue.Equals("INTERSECT") || uppercaseValue.Equals("INTO") || uppercaseValue.Equals("INSERT") || uppercaseValue.Equals("MERGE") || uppercaseValue.Equals("ORDER") || uppercaseValue.Equals("OUTPUT") || uppercaseValue.Equals("PIVOT") || uppercaseValue.Equals("RETURNS") || uppercaseValue.Equals("SELECT") || uppercaseValue.Equals("UNION") || uppercaseValue.Equals("UNPIVOT") || uppercaseValue.Equals("UPDATE") || uppercaseValue.Equals("USING") || uppercaseValue.Equals("VALUES") || uppercaseValue.Equals("WHERE") || uppercaseValue.Equals("WITH"));
		}

		private bool IsLatestTokenADDLDetailValue(ParseTree sqlTree)
		{
			Node latestContentNode;
			latestContentNode = sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
			if (latestContentNode != null && (latestContentNode.Name.Equals("OtherKeyword") || latestContentNode.Name.Equals("DataTypeKeyword") || latestContentNode.Name.Equals("CompoundKeyword")))
			{
				string uppercaseText;
				uppercaseText = null;
				uppercaseText = ((!latestContentNode.Name.Equals("CompoundKeyword")) ? latestContentNode.TextValue.ToUpperInvariant() : latestContentNode.GetAttributeValue("simpleText"));
				return uppercaseText.Equals("NVARCHAR") || uppercaseText.Equals("VARCHAR") || uppercaseText.Equals("DECIMAL") || uppercaseText.Equals("DEC") || uppercaseText.Equals("NUMERIC") || uppercaseText.Equals("VARBINARY") || uppercaseText.Equals("DEFAULT") || uppercaseText.Equals("IDENTITY") || uppercaseText.Equals("XML") || uppercaseText.EndsWith("VARYING") || uppercaseText.EndsWith("CHAR") || uppercaseText.EndsWith("CHARACTER") || uppercaseText.Equals("FLOAT") || uppercaseText.Equals("DATETIMEOFFSET") || uppercaseText.Equals("DATETIME2") || uppercaseText.Equals("TIME");
			}
			return false;
		}

		private bool IsLatestTokenAComma(ParseTree sqlTree)
		{
			return sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT)
				.LastOrDefault()?.Name.Equals("Comma") ?? false;
		}

		private bool IsLatestTokenAMiscName(ParseTree sqlTree)
		{
			Node latestContent;
			latestContent = sqlTree.CurrentContainer.ChildrenExcludingNames(SqlStructureConstants.ENAMELIST_NONCONTENT).LastOrDefault();
			if (latestContent != null)
			{
				string testValue;
				testValue = latestContent.TextValue.ToUpperInvariant();
				if (latestContent.Name.Equals("BracketQuotedName"))
				{
					return true;
				}
				if ((latestContent.Name.Equals("Other") || latestContent.Name.Equals("FunctionKeyword")) && !testValue.Equals("AND") && !testValue.Equals("OR") && !testValue.Equals("NOT") && !testValue.Equals("BETWEEN") && !testValue.Equals("LIKE") && !testValue.Equals("CONTAINS") && !testValue.Equals("EXISTS") && !testValue.Equals("FREETEXT") && !testValue.Equals("IN") && !testValue.Equals("ALL") && !testValue.Equals("SOME") && !testValue.Equals("ANY") && !testValue.Equals("FROM") && !testValue.Equals("JOIN") && !testValue.EndsWith(" JOIN") && !testValue.Equals("UNION") && !testValue.Equals("UNION ALL") && !testValue.Equals("USING") && !testValue.Equals("AS") && !testValue.EndsWith(" APPLY"))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsLineBreakingWhiteSpaceOrComment(IToken token)
		{
			return (token.Type == SqlTokenType.WhiteSpace && Regex.IsMatch(token.Value, "(\\r|\\n)+")) || token.Type == SqlTokenType.SingleLineComment;
		}

		private bool IsFollowedByLineBreakingWhiteSpaceOrSingleLineCommentOrEnd(ITokenList tokenList, int tokenID)
		{
			for (int currTokenID = tokenID + 1; tokenList.Count >= currTokenID + 1; currTokenID++)
			{
				if (tokenList[currTokenID].Type == SqlTokenType.SingleLineComment)
				{
					return true;
				}
				if (tokenList[currTokenID].Type == SqlTokenType.WhiteSpace)
				{
					if (Regex.IsMatch(tokenList[currTokenID].Value, "(\\r|\\n)+"))
					{
						return true;
					}
					continue;
				}
				return false;
			}
			return true;
		}

		private static void InitializeKeywordList()
		{
			KeywordList = new Dictionary<string, KeywordType>();
			KeywordList.Add("@@CONNECTIONS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@CPU_BUSY", KeywordType.FunctionKeyword);
			KeywordList.Add("@@CURSOR_ROWS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@DATEFIRST", KeywordType.FunctionKeyword);
			KeywordList.Add("@@DBTS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@ERROR", KeywordType.FunctionKeyword);
			KeywordList.Add("@@FETCH_STATUS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@IDENTITY", KeywordType.FunctionKeyword);
			KeywordList.Add("@@IDLE", KeywordType.FunctionKeyword);
			KeywordList.Add("@@IO_BUSY", KeywordType.FunctionKeyword);
			KeywordList.Add("@@LANGID", KeywordType.FunctionKeyword);
			KeywordList.Add("@@LANGUAGE", KeywordType.FunctionKeyword);
			KeywordList.Add("@@LOCK_TIMEOUT", KeywordType.FunctionKeyword);
			KeywordList.Add("@@MAX_CONNECTIONS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@MAX_PRECISION", KeywordType.FunctionKeyword);
			KeywordList.Add("@@NESTLEVEL", KeywordType.FunctionKeyword);
			KeywordList.Add("@@OPTIONS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@PACKET_ERRORS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@PACK_RECEIVED", KeywordType.FunctionKeyword);
			KeywordList.Add("@@PACK_SENT", KeywordType.FunctionKeyword);
			KeywordList.Add("@@PROCID", KeywordType.FunctionKeyword);
			KeywordList.Add("@@REMSERVER", KeywordType.FunctionKeyword);
			KeywordList.Add("@@ROWCOUNT", KeywordType.FunctionKeyword);
			KeywordList.Add("@@SERVERNAME", KeywordType.FunctionKeyword);
			KeywordList.Add("@@SERVICENAME", KeywordType.FunctionKeyword);
			KeywordList.Add("@@SPID", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TEXTSIZE", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TIMETICKS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TOTAL_ERRORS", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TOTAL_READ", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TOTAL_WRITE", KeywordType.FunctionKeyword);
			KeywordList.Add("@@TRANCOUNT", KeywordType.FunctionKeyword);
			KeywordList.Add("@@VERSION", KeywordType.FunctionKeyword);
			KeywordList.Add("ABS", KeywordType.FunctionKeyword);
			KeywordList.Add("ACOS", KeywordType.FunctionKeyword);
			KeywordList.Add("ACTIVATION", KeywordType.OtherKeyword);
			KeywordList.Add("ADD", KeywordType.OtherKeyword);
			KeywordList.Add("ALL", KeywordType.OperatorKeyword);
			KeywordList.Add("ALTER", KeywordType.OtherKeyword);
			KeywordList.Add("AND", KeywordType.OperatorKeyword);
			KeywordList.Add("ANSI_DEFAULTS", KeywordType.OtherKeyword);
			KeywordList.Add("ANSI_NULLS", KeywordType.OtherKeyword);
			KeywordList.Add("ANSI_NULL_DFLT_OFF", KeywordType.OtherKeyword);
			KeywordList.Add("ANSI_NULL_DFLT_ON", KeywordType.OtherKeyword);
			KeywordList.Add("ANSI_PADDING", KeywordType.OtherKeyword);
			KeywordList.Add("ANSI_WARNINGS", KeywordType.OtherKeyword);
			KeywordList.Add("ANY", KeywordType.OperatorKeyword);
			KeywordList.Add("APP_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("ARITHABORT", KeywordType.OtherKeyword);
			KeywordList.Add("ARITHIGNORE", KeywordType.OtherKeyword);
			KeywordList.Add("AS", KeywordType.OtherKeyword);
			KeywordList.Add("ASC", KeywordType.OtherKeyword);
			KeywordList.Add("ASCII", KeywordType.FunctionKeyword);
			KeywordList.Add("ASIN", KeywordType.FunctionKeyword);
			KeywordList.Add("ATAN", KeywordType.FunctionKeyword);
			KeywordList.Add("ATN2", KeywordType.FunctionKeyword);
			KeywordList.Add("AUTHORIZATION", KeywordType.OtherKeyword);
			KeywordList.Add("AVG", KeywordType.FunctionKeyword);
			KeywordList.Add("BACKUP", KeywordType.OtherKeyword);
			KeywordList.Add("BEGIN", KeywordType.OtherKeyword);
			KeywordList.Add("BETWEEN", KeywordType.OperatorKeyword);
			KeywordList.Add("BIGINT", KeywordType.DataTypeKeyword);
			KeywordList.Add("BINARY", KeywordType.DataTypeKeyword);
			KeywordList.Add("BIT", KeywordType.DataTypeKeyword);
			KeywordList.Add("BREAK", KeywordType.OtherKeyword);
			KeywordList.Add("BROWSE", KeywordType.OtherKeyword);
			KeywordList.Add("BULK", KeywordType.OtherKeyword);
			KeywordList.Add("BY", KeywordType.OtherKeyword);
			KeywordList.Add("CALLER", KeywordType.OtherKeyword);
			KeywordList.Add("CASCADE", KeywordType.OtherKeyword);
			KeywordList.Add("CASE", KeywordType.FunctionKeyword);
			KeywordList.Add("CAST", KeywordType.FunctionKeyword);
			KeywordList.Add("CATALOG", KeywordType.OtherKeyword);
			KeywordList.Add("CEILING", KeywordType.FunctionKeyword);
			KeywordList.Add("CHAR", KeywordType.DataTypeKeyword);
			KeywordList.Add("CHARACTER", KeywordType.DataTypeKeyword);
			KeywordList.Add("CHARINDEX", KeywordType.FunctionKeyword);
			KeywordList.Add("CHECK", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKALLOC", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKCATALOG", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKCONSTRAINTS", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKDB", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKFILEGROUP", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKIDENT", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKPOINT", KeywordType.OtherKeyword);
			KeywordList.Add("CHECKSUM", KeywordType.FunctionKeyword);
			KeywordList.Add("CHECKSUM_AGG", KeywordType.FunctionKeyword);
			KeywordList.Add("CHECKTABLE", KeywordType.OtherKeyword);
			KeywordList.Add("CLEANTABLE", KeywordType.OtherKeyword);
			KeywordList.Add("CLOSE", KeywordType.OtherKeyword);
			KeywordList.Add("CLUSTERED", KeywordType.OtherKeyword);
			KeywordList.Add("COALESCE", KeywordType.FunctionKeyword);
			KeywordList.Add("COLLATIONPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("COLLECTION", KeywordType.OtherKeyword);
			KeywordList.Add("COLUMN", KeywordType.OtherKeyword);
			KeywordList.Add("COLUMNPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("COL_LENGTH", KeywordType.FunctionKeyword);
			KeywordList.Add("COL_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("COMMIT", KeywordType.OtherKeyword);
			KeywordList.Add("COMMITTED", KeywordType.OtherKeyword);
			KeywordList.Add("COMPUTE", KeywordType.OtherKeyword);
			KeywordList.Add("CONCAT", KeywordType.OtherKeyword);
			KeywordList.Add("CONCAT_NULL_YIELDS_NULL", KeywordType.OtherKeyword);
			KeywordList.Add("CONCURRENCYVIOLATION", KeywordType.OtherKeyword);
			KeywordList.Add("CONFIRM", KeywordType.OtherKeyword);
			KeywordList.Add("CONSTRAINT", KeywordType.OtherKeyword);
			KeywordList.Add("CONTAINS", KeywordType.OtherKeyword);
			KeywordList.Add("CONTAINSTABLE", KeywordType.FunctionKeyword);
			KeywordList.Add("CONTINUE", KeywordType.OtherKeyword);
			KeywordList.Add("CONTROL", KeywordType.OtherKeyword);
			KeywordList.Add("CONTROLROW", KeywordType.OtherKeyword);
			KeywordList.Add("CONVERT", KeywordType.FunctionKeyword);
			KeywordList.Add("COS", KeywordType.FunctionKeyword);
			KeywordList.Add("COT", KeywordType.FunctionKeyword);
			KeywordList.Add("COUNT", KeywordType.FunctionKeyword);
			KeywordList.Add("COUNT_BIG", KeywordType.FunctionKeyword);
			KeywordList.Add("CREATE", KeywordType.OtherKeyword);
			KeywordList.Add("CROSS", KeywordType.OtherKeyword);
			KeywordList.Add("CURRENT", KeywordType.OtherKeyword);
			KeywordList.Add("CURRENT_DATE", KeywordType.OtherKeyword);
			KeywordList.Add("CURRENT_TIME", KeywordType.OtherKeyword);
			KeywordList.Add("CURRENT_TIMESTAMP", KeywordType.FunctionKeyword);
			KeywordList.Add("CURRENT_USER", KeywordType.FunctionKeyword);
			KeywordList.Add("CURSOR", KeywordType.OtherKeyword);
			KeywordList.Add("CURSOR_CLOSE_ON_COMMIT", KeywordType.OtherKeyword);
			KeywordList.Add("CURSOR_STATUS", KeywordType.FunctionKeyword);
			KeywordList.Add("DATABASE", KeywordType.OtherKeyword);
			KeywordList.Add("DATABASEPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("DATABASEPROPERTYEX", KeywordType.FunctionKeyword);
			KeywordList.Add("DATALENGTH", KeywordType.FunctionKeyword);
			KeywordList.Add("DATEADD", KeywordType.FunctionKeyword);
			KeywordList.Add("DATEDIFF", KeywordType.FunctionKeyword);
			KeywordList.Add("DATEFIRST", KeywordType.OtherKeyword);
			KeywordList.Add("DATEFORMAT", KeywordType.OtherKeyword);
			KeywordList.Add("DATENAME", KeywordType.FunctionKeyword);
			KeywordList.Add("DATE", KeywordType.DataTypeKeyword);
			KeywordList.Add("DATEPART", KeywordType.FunctionKeyword);
			KeywordList.Add("DATETIME", KeywordType.DataTypeKeyword);
			KeywordList.Add("DATETIME2", KeywordType.DataTypeKeyword);
			KeywordList.Add("DATETIMEOFFSET", KeywordType.DataTypeKeyword);
			KeywordList.Add("DAY", KeywordType.FunctionKeyword);
			KeywordList.Add("DBCC", KeywordType.OtherKeyword);
			KeywordList.Add("DBREINDEX", KeywordType.OtherKeyword);
			KeywordList.Add("DBREPAIR", KeywordType.OtherKeyword);
			KeywordList.Add("DB_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("DB_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("DEADLOCK_PRIORITY", KeywordType.OtherKeyword);
			KeywordList.Add("DEALLOCATE", KeywordType.OtherKeyword);
			KeywordList.Add("DEC", KeywordType.DataTypeKeyword);
			KeywordList.Add("DECIMAL", KeywordType.DataTypeKeyword);
			KeywordList.Add("DECLARE", KeywordType.OtherKeyword);
			KeywordList.Add("DEFAULT", KeywordType.OtherKeyword);
			KeywordList.Add("DEFINITION", KeywordType.OtherKeyword);
			KeywordList.Add("DEGREES", KeywordType.FunctionKeyword);
			KeywordList.Add("DELAY", KeywordType.OtherKeyword);
			KeywordList.Add("DELETE", KeywordType.OtherKeyword);
			KeywordList.Add("DENY", KeywordType.OtherKeyword);
			KeywordList.Add("DESC", KeywordType.OtherKeyword);
			KeywordList.Add("DIFFERENCE", KeywordType.FunctionKeyword);
			KeywordList.Add("DISABLE_DEF_CNST_CHK", KeywordType.OtherKeyword);
			KeywordList.Add("DISK", KeywordType.OtherKeyword);
			KeywordList.Add("DISTINCT", KeywordType.OtherKeyword);
			KeywordList.Add("DISTRIBUTED", KeywordType.OtherKeyword);
			KeywordList.Add("DOUBLE", KeywordType.DataTypeKeyword);
			KeywordList.Add("DROP", KeywordType.OtherKeyword);
			KeywordList.Add("DROPCLEANBUFFERS", KeywordType.OtherKeyword);
			KeywordList.Add("DUMMY", KeywordType.OtherKeyword);
			KeywordList.Add("DUMP", KeywordType.OtherKeyword);
			KeywordList.Add("DYNAMIC", KeywordType.OtherKeyword);
			KeywordList.Add("ELSE", KeywordType.OtherKeyword);
			KeywordList.Add("ENCRYPTION", KeywordType.OtherKeyword);
			KeywordList.Add("ERRLVL", KeywordType.OtherKeyword);
			KeywordList.Add("ERROREXIT", KeywordType.OtherKeyword);
			KeywordList.Add("ESCAPE", KeywordType.OtherKeyword);
			KeywordList.Add("EXCEPT", KeywordType.OtherKeyword);
			KeywordList.Add("EXEC", KeywordType.OtherKeyword);
			KeywordList.Add("EXECUTE", KeywordType.OtherKeyword);
			KeywordList.Add("EXISTS", KeywordType.OperatorKeyword);
			KeywordList.Add("EXIT", KeywordType.OtherKeyword);
			KeywordList.Add("EXP", KeywordType.FunctionKeyword);
			KeywordList.Add("EXPAND", KeywordType.OtherKeyword);
			KeywordList.Add("EXTERNAL", KeywordType.OtherKeyword);
			KeywordList.Add("FAST", KeywordType.OtherKeyword);
			KeywordList.Add("FAST_FORWARD", KeywordType.OtherKeyword);
			KeywordList.Add("FASTFIRSTROW", KeywordType.OtherKeyword);
			KeywordList.Add("FETCH", KeywordType.OtherKeyword);
			KeywordList.Add("FILE", KeywordType.OtherKeyword);
			KeywordList.Add("FILEGROUPPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("FILEGROUP_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("FILEGROUP_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("FILEPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("FILE_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("FILE_IDEX", KeywordType.FunctionKeyword);
			KeywordList.Add("FILE_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("FILLFACTOR", KeywordType.OtherKeyword);
			KeywordList.Add("FIPS_FLAGGER", KeywordType.OtherKeyword);
			KeywordList.Add("FLOAT", KeywordType.DataTypeKeyword);
			KeywordList.Add("FLOOR", KeywordType.FunctionKeyword);
			KeywordList.Add("FLOPPY", KeywordType.OtherKeyword);
			KeywordList.Add("FMTONLY", KeywordType.OtherKeyword);
			KeywordList.Add("FOR", KeywordType.OtherKeyword);
			KeywordList.Add("FORCE", KeywordType.OtherKeyword);
			KeywordList.Add("FORCED", KeywordType.OtherKeyword);
			KeywordList.Add("FORCEPLAN", KeywordType.OtherKeyword);
			KeywordList.Add("FOREIGN", KeywordType.OtherKeyword);
			KeywordList.Add("FORMATMESSAGE", KeywordType.FunctionKeyword);
			KeywordList.Add("FORWARD_ONLY", KeywordType.OtherKeyword);
			KeywordList.Add("FREEPROCCACHE", KeywordType.OtherKeyword);
			KeywordList.Add("FREESESSIONCACHE", KeywordType.OtherKeyword);
			KeywordList.Add("FREESYSTEMCACHE", KeywordType.OtherKeyword);
			KeywordList.Add("FREETEXT", KeywordType.OtherKeyword);
			KeywordList.Add("FREETEXTTABLE", KeywordType.FunctionKeyword);
			KeywordList.Add("FROM", KeywordType.OtherKeyword);
			KeywordList.Add("FULL", KeywordType.OtherKeyword);
			KeywordList.Add("FULLTEXT", KeywordType.OtherKeyword);
			KeywordList.Add("FULLTEXTCATALOGPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("FULLTEXTSERVICEPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("FUNCTION", KeywordType.OtherKeyword);
			KeywordList.Add("GEOGRAPHY", KeywordType.DataTypeKeyword);
			KeywordList.Add("GETANCESTOR", KeywordType.FunctionKeyword);
			KeywordList.Add("GETANSINULL", KeywordType.FunctionKeyword);
			KeywordList.Add("GETDATE", KeywordType.FunctionKeyword);
			KeywordList.Add("GETDESCENDANT", KeywordType.FunctionKeyword);
			KeywordList.Add("GETLEVEL", KeywordType.FunctionKeyword);
			KeywordList.Add("GETREPARENTEDVALUE", KeywordType.FunctionKeyword);
			KeywordList.Add("GETROOT", KeywordType.FunctionKeyword);
			KeywordList.Add("GLOBAL", KeywordType.OtherKeyword);
			KeywordList.Add("GO", KeywordType.OtherKeyword);
			KeywordList.Add("GOTO", KeywordType.OtherKeyword);
			KeywordList.Add("GRANT", KeywordType.OtherKeyword);
			KeywordList.Add("GROUP", KeywordType.OtherKeyword);
			KeywordList.Add("GROUPING", KeywordType.FunctionKeyword);
			KeywordList.Add("HASH", KeywordType.OtherKeyword);
			KeywordList.Add("HAVING", KeywordType.OtherKeyword);
			KeywordList.Add("HELP", KeywordType.OtherKeyword);
			KeywordList.Add("HIERARCHYID", KeywordType.DataTypeKeyword);
			KeywordList.Add("HOLDLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("HOST_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("HOST_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("IDENTITY", KeywordType.FunctionKeyword);
			KeywordList.Add("IDENTITYCOL", KeywordType.OtherKeyword);
			KeywordList.Add("IDENTITY_INSERT", KeywordType.OtherKeyword);
			KeywordList.Add("IDENT_CURRENT", KeywordType.FunctionKeyword);
			KeywordList.Add("IDENT_INCR", KeywordType.FunctionKeyword);
			KeywordList.Add("IDENT_SEED", KeywordType.FunctionKeyword);
			KeywordList.Add("IF", KeywordType.OtherKeyword);
			KeywordList.Add("IGNORE_CONSTRAINTS", KeywordType.OtherKeyword);
			KeywordList.Add("IGNORE_TRIGGERS", KeywordType.OtherKeyword);
			KeywordList.Add("IMAGE", KeywordType.DataTypeKeyword);
			KeywordList.Add("IMPLICIT_TRANSACTIONS", KeywordType.OtherKeyword);
			KeywordList.Add("IN", KeywordType.OperatorKeyword);
			KeywordList.Add("INDEX", KeywordType.OtherKeyword);
			KeywordList.Add("INDEXDEFRAG", KeywordType.OtherKeyword);
			KeywordList.Add("INDEXKEY_PROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("INDEXPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("INDEX_COL", KeywordType.FunctionKeyword);
			KeywordList.Add("INNER", KeywordType.OtherKeyword);
			KeywordList.Add("INPUTBUFFER", KeywordType.OtherKeyword);
			KeywordList.Add("INSENSITIVE", KeywordType.DataTypeKeyword);
			KeywordList.Add("INSERT", KeywordType.OtherKeyword);
			KeywordList.Add("INT", KeywordType.DataTypeKeyword);
			KeywordList.Add("INTEGER", KeywordType.DataTypeKeyword);
			KeywordList.Add("INTERSECT", KeywordType.OtherKeyword);
			KeywordList.Add("INTO", KeywordType.OtherKeyword);
			KeywordList.Add("IO", KeywordType.OtherKeyword);
			KeywordList.Add("IS", KeywordType.OtherKeyword);
			KeywordList.Add("ISDATE", KeywordType.FunctionKeyword);
			KeywordList.Add("ISDESCENDANTOF", KeywordType.FunctionKeyword);
			KeywordList.Add("ISNULL", KeywordType.FunctionKeyword);
			KeywordList.Add("ISNUMERIC", KeywordType.FunctionKeyword);
			KeywordList.Add("ISOLATION", KeywordType.OtherKeyword);
			KeywordList.Add("IS_MEMBER", KeywordType.FunctionKeyword);
			KeywordList.Add("IS_SRVROLEMEMBER", KeywordType.FunctionKeyword);
			KeywordList.Add("JOIN", KeywordType.OtherKeyword);
			KeywordList.Add("KEEP", KeywordType.OtherKeyword);
			KeywordList.Add("KEEPDEFAULTS", KeywordType.OtherKeyword);
			KeywordList.Add("KEEPFIXED", KeywordType.OtherKeyword);
			KeywordList.Add("KEEPIDENTITY", KeywordType.OtherKeyword);
			KeywordList.Add("KEY", KeywordType.OtherKeyword);
			KeywordList.Add("KEYSET", KeywordType.OtherKeyword);
			KeywordList.Add("KILL", KeywordType.OtherKeyword);
			KeywordList.Add("LANGUAGE", KeywordType.OtherKeyword);
			KeywordList.Add("LEFT", KeywordType.FunctionKeyword);
			KeywordList.Add("LEN", KeywordType.FunctionKeyword);
			KeywordList.Add("LEVEL", KeywordType.OtherKeyword);
			KeywordList.Add("LIKE", KeywordType.OperatorKeyword);
			KeywordList.Add("LINENO", KeywordType.OtherKeyword);
			KeywordList.Add("LOAD", KeywordType.OtherKeyword);
			KeywordList.Add("LOCAL", KeywordType.OtherKeyword);
			KeywordList.Add("LOCK_TIMEOUT", KeywordType.OtherKeyword);
			KeywordList.Add("LOG", KeywordType.FunctionKeyword);
			KeywordList.Add("LOG10", KeywordType.FunctionKeyword);
			KeywordList.Add("LOGIN", KeywordType.OtherKeyword);
			KeywordList.Add("LOOP", KeywordType.OtherKeyword);
			KeywordList.Add("LOWER", KeywordType.FunctionKeyword);
			KeywordList.Add("LTRIM", KeywordType.FunctionKeyword);
			KeywordList.Add("MATCHED", KeywordType.OtherKeyword);
			KeywordList.Add("MAX", KeywordType.FunctionKeyword);
			KeywordList.Add("MAX_QUEUE_READERS", KeywordType.OtherKeyword);
			KeywordList.Add("MAXDOP", KeywordType.OtherKeyword);
			KeywordList.Add("MAXRECURSION", KeywordType.OtherKeyword);
			KeywordList.Add("MERGE", KeywordType.OtherKeyword);
			KeywordList.Add("MIN", KeywordType.FunctionKeyword);
			KeywordList.Add("MIRROREXIT", KeywordType.OtherKeyword);
			KeywordList.Add("MODIFY", KeywordType.FunctionKeyword);
			KeywordList.Add("MONEY", KeywordType.DataTypeKeyword);
			KeywordList.Add("MONTH", KeywordType.FunctionKeyword);
			KeywordList.Add("MOVE", KeywordType.OtherKeyword);
			KeywordList.Add("NAMES", KeywordType.OtherKeyword);
			KeywordList.Add("NATIONAL", KeywordType.DataTypeKeyword);
			KeywordList.Add("NCHAR", KeywordType.DataTypeKeyword);
			KeywordList.Add("NEWID", KeywordType.FunctionKeyword);
			KeywordList.Add("NEXT", KeywordType.OtherKeyword);
			KeywordList.Add("NOCHECK", KeywordType.OtherKeyword);
			KeywordList.Add("NOCOUNT", KeywordType.OtherKeyword);
			KeywordList.Add("NODES", KeywordType.FunctionKeyword);
			KeywordList.Add("NOEXEC", KeywordType.OtherKeyword);
			KeywordList.Add("NOEXPAND", KeywordType.OtherKeyword);
			KeywordList.Add("NOLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("NONCLUSTERED", KeywordType.OtherKeyword);
			KeywordList.Add("NOT", KeywordType.OperatorKeyword);
			KeywordList.Add("NOWAIT", KeywordType.OtherKeyword);
			KeywordList.Add("NTEXT", KeywordType.DataTypeKeyword);
			KeywordList.Add("NTILE", KeywordType.FunctionKeyword);
			KeywordList.Add("NULL", KeywordType.OtherKeyword);
			KeywordList.Add("NULLIF", KeywordType.FunctionKeyword);
			KeywordList.Add("NUMERIC", KeywordType.DataTypeKeyword);
			KeywordList.Add("NUMERIC_ROUNDABORT", KeywordType.OtherKeyword);
			KeywordList.Add("NVARCHAR", KeywordType.DataTypeKeyword);
			KeywordList.Add("OBJECTPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("OBJECTPROPERTYEX", KeywordType.FunctionKeyword);
			KeywordList.Add("OBJECT", KeywordType.OtherKeyword);
			KeywordList.Add("OBJECT_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("OBJECT_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("OF", KeywordType.OtherKeyword);
			KeywordList.Add("OFF", KeywordType.OtherKeyword);
			KeywordList.Add("OFFSETS", KeywordType.OtherKeyword);
			KeywordList.Add("ON", KeywordType.OtherKeyword);
			KeywordList.Add("ONCE", KeywordType.OtherKeyword);
			KeywordList.Add("ONLY", KeywordType.OtherKeyword);
			KeywordList.Add("OPEN", KeywordType.OtherKeyword);
			KeywordList.Add("OPENDATASOURCE", KeywordType.OtherKeyword);
			KeywordList.Add("OPENQUERY", KeywordType.FunctionKeyword);
			KeywordList.Add("OPENROWSET", KeywordType.FunctionKeyword);
			KeywordList.Add("OPENTRAN", KeywordType.OtherKeyword);
			KeywordList.Add("OPTIMIZE", KeywordType.OtherKeyword);
			KeywordList.Add("OPTIMISTIC", KeywordType.OtherKeyword);
			KeywordList.Add("OPTION", KeywordType.OtherKeyword);
			KeywordList.Add("OR", KeywordType.OperatorKeyword);
			KeywordList.Add("ORDER", KeywordType.OtherKeyword);
			KeywordList.Add("OUTER", KeywordType.OtherKeyword);
			KeywordList.Add("OUT", KeywordType.OtherKeyword);
			KeywordList.Add("OUTPUT", KeywordType.OtherKeyword);
			KeywordList.Add("OUTPUTBUFFER", KeywordType.OtherKeyword);
			KeywordList.Add("OVER", KeywordType.OtherKeyword);
			KeywordList.Add("OWNER", KeywordType.OtherKeyword);
			KeywordList.Add("PAGLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("PARAMETERIZATION", KeywordType.OtherKeyword);
			KeywordList.Add("PARSE", KeywordType.FunctionKeyword);
			KeywordList.Add("PARSENAME", KeywordType.FunctionKeyword);
			KeywordList.Add("PARSEONLY", KeywordType.OtherKeyword);
			KeywordList.Add("PARTITION", KeywordType.OtherKeyword);
			KeywordList.Add("PATINDEX", KeywordType.FunctionKeyword);
			KeywordList.Add("PERCENT", KeywordType.OtherKeyword);
			KeywordList.Add("PERM", KeywordType.OtherKeyword);
			KeywordList.Add("PERMANENT", KeywordType.OtherKeyword);
			KeywordList.Add("PERMISSIONS", KeywordType.FunctionKeyword);
			KeywordList.Add("PI", KeywordType.FunctionKeyword);
			KeywordList.Add("PINTABLE", KeywordType.OtherKeyword);
			KeywordList.Add("PIPE", KeywordType.OtherKeyword);
			KeywordList.Add("PLAN", KeywordType.OtherKeyword);
			KeywordList.Add("POWER", KeywordType.FunctionKeyword);
			KeywordList.Add("PREPARE", KeywordType.OtherKeyword);
			KeywordList.Add("PRIMARY", KeywordType.OtherKeyword);
			KeywordList.Add("PRINT", KeywordType.OtherKeyword);
			KeywordList.Add("PRIVILEGES", KeywordType.OtherKeyword);
			KeywordList.Add("PROC", KeywordType.OtherKeyword);
			KeywordList.Add("PROCCACHE", KeywordType.OtherKeyword);
			KeywordList.Add("PROCEDURE", KeywordType.OtherKeyword);
			KeywordList.Add("PROCEDURE_NAME", KeywordType.OtherKeyword);
			KeywordList.Add("PROCESSEXIT", KeywordType.OtherKeyword);
			KeywordList.Add("PROCID", KeywordType.OtherKeyword);
			KeywordList.Add("PROFILE", KeywordType.OtherKeyword);
			KeywordList.Add("PUBLIC", KeywordType.OtherKeyword);
			KeywordList.Add("QUERY", KeywordType.FunctionKeyword);
			KeywordList.Add("QUERY_GOVERNOR_COST_LIMIT", KeywordType.OtherKeyword);
			KeywordList.Add("QUEUE", KeywordType.OtherKeyword);
			KeywordList.Add("QUOTED_IDENTIFIER", KeywordType.OtherKeyword);
			KeywordList.Add("QUOTENAME", KeywordType.FunctionKeyword);
			KeywordList.Add("RADIANS", KeywordType.FunctionKeyword);
			KeywordList.Add("RAISERROR", KeywordType.OtherKeyword);
			KeywordList.Add("RAND", KeywordType.FunctionKeyword);
			KeywordList.Add("READ", KeywordType.OtherKeyword);
			KeywordList.Add("READCOMMITTED", KeywordType.OtherKeyword);
			KeywordList.Add("READCOMMITTEDLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("READPAST", KeywordType.OtherKeyword);
			KeywordList.Add("READTEXT", KeywordType.OtherKeyword);
			KeywordList.Add("READUNCOMMITTED", KeywordType.OtherKeyword);
			KeywordList.Add("READ_ONLY", KeywordType.OtherKeyword);
			KeywordList.Add("REAL", KeywordType.DataTypeKeyword);
			KeywordList.Add("RECOMPILE", KeywordType.OtherKeyword);
			KeywordList.Add("RECONFIGURE", KeywordType.OtherKeyword);
			KeywordList.Add("REFERENCES", KeywordType.OtherKeyword);
			KeywordList.Add("REMOTE_PROC_TRANSACTIONS", KeywordType.OtherKeyword);
			KeywordList.Add("REPEATABLE", KeywordType.OtherKeyword);
			KeywordList.Add("REPEATABLEREAD", KeywordType.OtherKeyword);
			KeywordList.Add("REPLACE", KeywordType.FunctionKeyword);
			KeywordList.Add("REPLICATE", KeywordType.FunctionKeyword);
			KeywordList.Add("REPLICATION", KeywordType.OtherKeyword);
			KeywordList.Add("RESTORE", KeywordType.OtherKeyword);
			KeywordList.Add("RESTRICT", KeywordType.OtherKeyword);
			KeywordList.Add("RETURN", KeywordType.OtherKeyword);
			KeywordList.Add("RETURNS", KeywordType.OtherKeyword);
			KeywordList.Add("REVERSE", KeywordType.FunctionKeyword);
			KeywordList.Add("REVERT", KeywordType.OtherKeyword);
			KeywordList.Add("REVOKE", KeywordType.OtherKeyword);
			KeywordList.Add("RIGHT", KeywordType.FunctionKeyword);
			KeywordList.Add("ROBUST", KeywordType.OtherKeyword);
			KeywordList.Add("ROLE", KeywordType.OtherKeyword);
			KeywordList.Add("ROLLBACK", KeywordType.OtherKeyword);
			KeywordList.Add("ROUND", KeywordType.FunctionKeyword);
			KeywordList.Add("ROWCOUNT", KeywordType.OtherKeyword);
			KeywordList.Add("ROWGUIDCOL", KeywordType.OtherKeyword);
			KeywordList.Add("ROWLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("ROWVERSION", KeywordType.DataTypeKeyword);
			KeywordList.Add("RTRIM", KeywordType.FunctionKeyword);
			KeywordList.Add("RULE", KeywordType.OtherKeyword);
			KeywordList.Add("SAVE", KeywordType.OtherKeyword);
			KeywordList.Add("SCHEMA", KeywordType.OtherKeyword);
			KeywordList.Add("SCHEMA_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("SCHEMA_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("SCOPE_IDENTITY", KeywordType.FunctionKeyword);
			KeywordList.Add("SCROLL", KeywordType.OtherKeyword);
			KeywordList.Add("SCROLL_LOCKS", KeywordType.OtherKeyword);
			KeywordList.Add("SELECT", KeywordType.OtherKeyword);
			KeywordList.Add("SELF", KeywordType.OtherKeyword);
			KeywordList.Add("SERIALIZABLE", KeywordType.OtherKeyword);
			KeywordList.Add("SERVER", KeywordType.OtherKeyword);
			KeywordList.Add("SERVERPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("SESSIONPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("SESSION_USER", KeywordType.FunctionKeyword);
			KeywordList.Add("SET", KeywordType.OtherKeyword);
			KeywordList.Add("SETUSER", KeywordType.OtherKeyword);
			KeywordList.Add("SHOWCONTIG", KeywordType.OtherKeyword);
			KeywordList.Add("SHOWPLAN_ALL", KeywordType.OtherKeyword);
			KeywordList.Add("SHOWPLAN_TEXT", KeywordType.OtherKeyword);
			KeywordList.Add("SHOW_STATISTICS", KeywordType.OtherKeyword);
			KeywordList.Add("SHRINKDATABASE", KeywordType.OtherKeyword);
			KeywordList.Add("SHRINKFILE", KeywordType.OtherKeyword);
			KeywordList.Add("SHUTDOWN", KeywordType.OtherKeyword);
			KeywordList.Add("SIGN", KeywordType.FunctionKeyword);
			KeywordList.Add("SIMPLE", KeywordType.OtherKeyword);
			KeywordList.Add("SIN", KeywordType.FunctionKeyword);
			KeywordList.Add("SMALLDATETIME", KeywordType.DataTypeKeyword);
			KeywordList.Add("SMALLINT", KeywordType.DataTypeKeyword);
			KeywordList.Add("SMALLMONEY", KeywordType.DataTypeKeyword);
			KeywordList.Add("SOME", KeywordType.OperatorKeyword);
			KeywordList.Add("SOUNDEX", KeywordType.FunctionKeyword);
			KeywordList.Add("SPACE", KeywordType.FunctionKeyword);
			KeywordList.Add("SQLPERF", KeywordType.OtherKeyword);
			KeywordList.Add("SQL_VARIANT", KeywordType.DataTypeKeyword);
			KeywordList.Add("SQL_VARIANT_PROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("SQRT", KeywordType.FunctionKeyword);
			KeywordList.Add("SQUARE", KeywordType.FunctionKeyword);
			KeywordList.Add("STATE", KeywordType.OtherKeyword);
			KeywordList.Add("STATISTICS", KeywordType.OtherKeyword);
			KeywordList.Add("STATIC", KeywordType.OtherKeyword);
			KeywordList.Add("STATS_DATE", KeywordType.FunctionKeyword);
			KeywordList.Add("STATUS", KeywordType.OtherKeyword);
			KeywordList.Add("STDEV", KeywordType.FunctionKeyword);
			KeywordList.Add("STDEVP", KeywordType.FunctionKeyword);
			KeywordList.Add("STOPLIST", KeywordType.OtherKeyword);
			KeywordList.Add("STR", KeywordType.FunctionKeyword);
			KeywordList.Add("STUFF", KeywordType.FunctionKeyword);
			KeywordList.Add("SUBSTRING", KeywordType.FunctionKeyword);
			KeywordList.Add("SUM", KeywordType.FunctionKeyword);
			KeywordList.Add("SUSER_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("SUSER_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("SUSER_SID", KeywordType.FunctionKeyword);
			KeywordList.Add("SUSER_SNAME", KeywordType.FunctionKeyword);
			KeywordList.Add("SYNONYM", KeywordType.OtherKeyword);
			KeywordList.Add("SYSNAME", KeywordType.DataTypeKeyword);
			KeywordList.Add("SYSTEM_USER", KeywordType.FunctionKeyword);
			KeywordList.Add("TABLE", KeywordType.OtherKeyword);
			KeywordList.Add("TABLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("TABLOCKX", KeywordType.OtherKeyword);
			KeywordList.Add("TAN", KeywordType.FunctionKeyword);
			KeywordList.Add("TAPE", KeywordType.OtherKeyword);
			KeywordList.Add("TEMP", KeywordType.OtherKeyword);
			KeywordList.Add("TEMPORARY", KeywordType.OtherKeyword);
			KeywordList.Add("TEXT", KeywordType.DataTypeKeyword);
			KeywordList.Add("TEXTPTR", KeywordType.FunctionKeyword);
			KeywordList.Add("TEXTSIZE", KeywordType.OtherKeyword);
			KeywordList.Add("TEXTVALID", KeywordType.FunctionKeyword);
			KeywordList.Add("THEN", KeywordType.OtherKeyword);
			KeywordList.Add("TIME", KeywordType.DataTypeKeyword);
			KeywordList.Add("TIMESTAMP", KeywordType.DataTypeKeyword);
			KeywordList.Add("TINYINT", KeywordType.DataTypeKeyword);
			KeywordList.Add("TO", KeywordType.OtherKeyword);
			KeywordList.Add("TOP", KeywordType.OtherKeyword);
			KeywordList.Add("TOSTRING", KeywordType.FunctionKeyword);
			KeywordList.Add("TRACEOFF", KeywordType.OtherKeyword);
			KeywordList.Add("TRACEON", KeywordType.OtherKeyword);
			KeywordList.Add("TRACESTATUS", KeywordType.OtherKeyword);
			KeywordList.Add("TRAN", KeywordType.OtherKeyword);
			KeywordList.Add("TRANSACTION", KeywordType.OtherKeyword);
			KeywordList.Add("TRIGGER", KeywordType.OtherKeyword);
			KeywordList.Add("TRUNCATE", KeywordType.OtherKeyword);
			KeywordList.Add("TSEQUAL", KeywordType.OtherKeyword);
			KeywordList.Add("TYPEPROPERTY", KeywordType.FunctionKeyword);
			KeywordList.Add("TYPE_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("TYPE_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("TYPE_WARNING", KeywordType.OtherKeyword);
			KeywordList.Add("UNCOMMITTED", KeywordType.OtherKeyword);
			KeywordList.Add("UNICODE", KeywordType.FunctionKeyword);
			KeywordList.Add("UNION", KeywordType.OtherKeyword);
			KeywordList.Add("UNIQUE", KeywordType.OtherKeyword);
			KeywordList.Add("UNIQUEIDENTIFIER", KeywordType.DataTypeKeyword);
			KeywordList.Add("UNKNOWN", KeywordType.OtherKeyword);
			KeywordList.Add("UNPINTABLE", KeywordType.OtherKeyword);
			KeywordList.Add("UPDATE", KeywordType.OtherKeyword);
			KeywordList.Add("UPDATETEXT", KeywordType.OtherKeyword);
			KeywordList.Add("UPDATEUSAGE", KeywordType.OtherKeyword);
			KeywordList.Add("UPDLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("UPPER", KeywordType.FunctionKeyword);
			KeywordList.Add("USE", KeywordType.OtherKeyword);
			KeywordList.Add("USER", KeywordType.FunctionKeyword);
			KeywordList.Add("USEROPTIONS", KeywordType.OtherKeyword);
			KeywordList.Add("USER_ID", KeywordType.FunctionKeyword);
			KeywordList.Add("USER_NAME", KeywordType.FunctionKeyword);
			KeywordList.Add("USING", KeywordType.OtherKeyword);
			KeywordList.Add("VALUE", KeywordType.FunctionKeyword);
			KeywordList.Add("VALUES", KeywordType.OtherKeyword);
			KeywordList.Add("VAR", KeywordType.FunctionKeyword);
			KeywordList.Add("VARBINARY", KeywordType.DataTypeKeyword);
			KeywordList.Add("VARCHAR", KeywordType.DataTypeKeyword);
			KeywordList.Add("VARP", KeywordType.FunctionKeyword);
			KeywordList.Add("VARYING", KeywordType.OtherKeyword);
			KeywordList.Add("VIEW", KeywordType.OtherKeyword);
			KeywordList.Add("VIEWS", KeywordType.OtherKeyword);
			KeywordList.Add("WAITFOR", KeywordType.OtherKeyword);
			KeywordList.Add("WHEN", KeywordType.OtherKeyword);
			KeywordList.Add("WHERE", KeywordType.OtherKeyword);
			KeywordList.Add("WHILE", KeywordType.OtherKeyword);
			KeywordList.Add("WITH", KeywordType.OtherKeyword);
			KeywordList.Add("WORK", KeywordType.OtherKeyword);
			KeywordList.Add("WRITE", KeywordType.FunctionKeyword);
			KeywordList.Add("WRITETEXT", KeywordType.OtherKeyword);
			KeywordList.Add("XACT_ABORT", KeywordType.OtherKeyword);
			KeywordList.Add("XLOCK", KeywordType.OtherKeyword);
			KeywordList.Add("XML", KeywordType.DataTypeKeyword);
			KeywordList.Add("YEAR", KeywordType.FunctionKeyword);
		}
	}
}
