using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class UIFontSelect : UIViewController
	{
		static private List<System.WeakReference> all = new List<System.WeakReference>();
		static private string all_font_name;
		static private float all_scale;
		static private int remove_counter = 0;

		static public string get_font() {
			return all_font_name;
		}
		static public void set_font(string na)
		{
			all_font_name = na;
			fix_font_scale ();
			if (String.IsNullOrEmpty (all_font_name))
				return;
			foreach(var w in all) {
				Delegate f = w.Target as Delegate;
				if (f != null)
					f.on_font_changed (all_font_name, all_scale);
			}
		}
		static private void fix_font_scale()
		{
			all_scale = 1;
			if (all_font_name == "HelveticaNeue")
				all_scale = 1f;
			if (all_font_name == "HelveticaNeue-Bold")
				all_scale = 1f;
		}
		static public void AddDelegate(Delegate d)
		{
			if( ++remove_counter > 100){ // TODO - better idea?
				remove_counter = 0;
				all.RemoveAll ((w)=>!w.IsAlive);
			}
			all.Add ( new WeakReference(d) );
			if (String.IsNullOrEmpty (all_font_name))
				return;
			d.on_font_changed (all_font_name, all_scale);
		}
		public interface Delegate {
			void on_font_changed(string font_name, float scale);
		};
		static public void load_settings()
		{
			string na = NSUserDefaults.StandardUserDefaults.StringForKey ("SelectedFont");
			if( String.IsNullOrEmpty(na) )
				na = "Noteworthy-Bold";
			set_font (na);
		}

		private List<string> available_fonts = new List<String>(){
			"MarkerFelt-Thin", "Thonburi-Bold", 
			"HelveticaNeue", "HelveticaNeue-Bold",
			"Noteworthy-Light", "Noteworthy-Bold"};

		private UITableView table;
		public UIFontSelect ()// : base ("UIFontSelect", null)
		{
			NavigationItem.Title = i18n.get ("FontTitle");
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				ContentSizeForViewInPopover = new SizeF(320, 460);
		}
		~UIFontSelect()
		{
			Console.WriteLine ("~UIFontSelect");
		}
		private static Utility.ReuseVC<UIFontSelect> reuse = new Utility.ReuseVC<UIFontSelect> ();
		public static UIFontSelect create_or_reuse()
		{
			UIFontSelect result = reuse.create_or_reuse();
			return result;
		}
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			if (!IsViewLoaded) {
				table = Utility.free_view (table);
			}		
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;

			table = AppDelegate.create_table_and_background (View, 0);

			table.Source = new TableSource (this);
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			int ind = available_fonts.IndexOf (UIFontSelect.get_font());
			if( ind != -1 )
				table.ScrollToRow (NSIndexPath.FromRowSection(ind, 0), UITableViewScrollPosition.Middle, false);
		}
		public class TableSource : UITableViewSource {
			UIFontSelect parent_vc;
			public TableSource (UIFontSelect parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return 1;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return parent_vc.available_fonts.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				UiFontSelectCell cell = (UiFontSelectCell)tableView.DequeueReusableCell (UiFontSelectCell.Key);
				if (cell == null)
					cell = UiFontSelectCell.Create(tableView);
				cell.setFontName(parent_vc.available_fonts[indexPath.Row]);
				cell.configAccessory (true, parent_vc.available_fonts[indexPath.Row] == UIFontSelect.get_font() );
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				string fn = parent_vc.available_fonts[indexPath.Row];
				UIFontSelect.set_font (fn);
				NSUserDefaults.StandardUserDefaults.SetString (fn, "SelectedFont");
				FlurryAnalytics.Flurry.LogEvent("Font", NSDictionary.FromObjectsAndKeys(new object[]{fn}, new object[]{"0"}));
				updateVisibleCellCheckmarks (tableView);
				AppDelegate.app.docs.send_anything_changed (null);
				Utility.dismiss_or_pop (parent_vc.NavigationController, true);
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
			private void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (var ii in tableView.IndexPathsForVisibleRows) {
					UiFontSelectCell cell = tableView.CellAt (ii) as UiFontSelectCell;
					if (cell == null)
						continue;
					cell.configAccessory (true, parent_vc.available_fonts[ii.Row] == UIFontSelect.get_font());
				}
			}
		}
	}
}

