// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ColorSelector
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSScrollView scr_Table { get; set; }

		[Outlet]
		AppKit.NSTableView tbl_Main { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (scr_Table != null) {
				scr_Table.Dispose ();
				scr_Table = null;
			}

			if (tbl_Main != null) {
				tbl_Main.Dispose ();
				tbl_Main = null;
			}
		}
	}
}
