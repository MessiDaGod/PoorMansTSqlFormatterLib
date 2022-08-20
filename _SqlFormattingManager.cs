using System;
using System.Runtime.InteropServices;

namespace PoorMansTSqlFormatterLib
{
	[Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2A")]
	[CLSCompliant(false)]
	[ComVisible(true)]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface _SqlFormattingManager
	{
		string Format(string inputSQL);
	}
}
