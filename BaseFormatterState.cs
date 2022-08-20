using System;
using System.Text;

namespace PoorMansTSqlFormatterLib
{
	internal class BaseFormatterState
	{
		protected StringBuilder _outBuilder = new StringBuilder();

		protected bool HtmlOutput { get; set; }

		public BaseFormatterState(bool htmlOutput)
		{
			HtmlOutput = htmlOutput;
		}

		public virtual void AddOutputContent(string content)
		{
			AddOutputContent(content, null);
		}

		public virtual void AddOutputContent(string content, string htmlClassName)
		{
			if (HtmlOutput)
			{
				if (!string.IsNullOrEmpty(htmlClassName))
				{
					_outBuilder.Append("<span class=\"" + htmlClassName + "\">");
				}
				_outBuilder.Append(Utils.HtmlEncode(content));
				if (!string.IsNullOrEmpty(htmlClassName))
				{
					_outBuilder.Append("</span>");
				}
			}
			else
			{
				_outBuilder.Append(content);
			}
		}

		public virtual void OpenClass(string htmlClassName)
		{
			if (htmlClassName == null)
			{
				throw new ArgumentNullException("htmlClassName");
			}
			if (HtmlOutput)
			{
				_outBuilder.Append("<span class=\"" + htmlClassName + "\">");
			}
		}

		public virtual void CloseClass()
		{
			if (HtmlOutput)
			{
				_outBuilder.Append("</span>");
			}
		}

		public virtual void AddOutputContentRaw(string content)
		{
			_outBuilder.Append(content);
		}

		public virtual void AddOutputLineBreak()
		{
			_outBuilder.Append(Environment.NewLine);
		}

		public string DumpOutput()
		{
			return _outBuilder.ToString();
		}
	}
}
