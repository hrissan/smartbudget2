using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Collections.Generic;

namespace SmartBudgetiOS
{
	public class CategoryVC : UIViewController
	{
		private int allowed_sign;
		private string category;
		private bool in_chain;
		public List< List<DBCategory> > categories_section;
		public int columns;
		public const int SPECIAL = 0;
		public const int RECENT = 1;
		public const int OLD = 2;
		public const int NEVER_USED = 3;
		public const int BUTTONS = 4;
		//private TableSource ts;
		private Action<CategoryVC, string> on_save;
		UITableView table;
		UIView panelButtons;
		UIButton btnHelp;
		UIButton btnPlus;
		private Document doc;
		UIBarButtonItem button_next;
		public CategoryVC ()// : base ("CategoryVC", null)
		{
			columns = Category4Cell.get_columns();

			NavigationItem.Title = i18n.get ("CategoryTitle");
			button_next = new UIBarButtonItem (i18n.get ("Next"), UIBarButtonItemStyle.Plain, (sender, e) => {
				on_save.Invoke (this, category);
			});
		}
		void construct(Document doc, int allowed_sign, string initial_category, bool in_chain, Action<CategoryVC, string> on_save)
		{
			this.doc = doc;
			this.allowed_sign = allowed_sign;
			this.category = initial_category;
			this.in_chain = in_chain;
			this.on_save = on_save;
			NavigationItem.RightBarButtonItem = in_chain ? button_next : null;
		}
		~CategoryVC()
		{
			Console.WriteLine ("~CategoryVC");
		}
		private static Utility.ReuseVC<CategoryVC> reuse = new Utility.ReuseVC<CategoryVC> ();
		public static CategoryVC create_or_reuse(Document doc, int allowed_sign, string initial_category, bool in_chain, Action<CategoryVC, string> on_save)
		{
			CategoryVC result = reuse.create_or_reuse();
			result.construct (doc, allowed_sign, initial_category, in_chain, on_save);
			return result;
		}
		private void category_taphold(string cat_id, UIView origin)
		{
			if (cat_id == Document.CONVERT_CATEGORY || cat_id == Document.TRANSFER_CATEGORY || cat_id == Document.LOAN_CATEGORY || cat_id == Document.LOAN_BACK_CATEGORY) { // Special
				new UIAlertView(i18n.get ("BuiltInCategoryTitle"), i18n.get ("BuiltInCategoryText"), null, i18n.get ("OK"), null).Show();
				return;
			}
			DBCategory category = doc.get_category (cat_id);
			int expenses_count;
			long recent_expense_date;
			doc.get_category_count_date (cat_id, out expenses_count, out recent_expense_date);
			string del_title = expenses_count != 0 ? null : i18n.get ("Delete");
			UIActionSheet ash = new UIActionSheet(i18n.get ("UserCategorySheetTitle").Replace("%@", category.get_loc_name()).Replace("%d", expenses_count.ToString()), null, i18n.get ("Cancel"), del_title, // ToString - ok
			                                      i18n.get ("EditName"), i18n.get ("SelectImage"), i18n.get ("DrawImage"));
			ash.Clicked += (sender, e) => {
				if( e.ButtonIndex == ash.DestructiveButtonIndex ){
					FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"Delete"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change(new ChangeCategoryRemove(doc, category.id));
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("EditName") ){
					NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("CategoryNameTitle"), i18n.get ("CategoryNamePlaceholder"),false,false,false,category.get_loc_name(), "", (nvc, str) => {
						DBCategory new_cat = category.Clone();
						new_cat.name_key = "";
						new_cat.name = str;
						FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"EditName"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change(new ChangeCategoryUpdate(doc, new_cat));
					}), true);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("SelectImage") ){
					FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"SelectImageStart"}, new object[]{"action"}));
					NavigationController.PushViewController(ImageSelectVC.create_or_reuse(category.image_name, category.image_data, (isvc,img,img_data)=>{
						DBCategory new_cat = category.Clone();
						new_cat.image_name = img;
						new_cat.image_data = img_data;
						FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"SelectImage"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change(new ChangeCategoryUpdate(doc, new_cat));
					}), true);
				}
				if( ash.ButtonTitle(e.ButtonIndex) == i18n.get ("DrawImage") ){
					FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"DrawImageStart"}, new object[]{"action"}));
					NavigationController.PushViewController(DrawVC.create_or_reuse(AppDelegate.app.get_category_hires_image(category), (dvc,img)=>{
						DBCategory new_cat = category.Clone();
						new_cat.image_name = "";

						NSData data = img.AsPNG();
						new_cat.image_data = Utility.from_nsdata(data);
						FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"DrawImage"}, new object[]{"action"}));
						AppDelegate.app.docs.execute_change(new ChangeCategoryUpdate(doc, new_cat));
					}), true);
				}
		    };
			Utility.show_action_sheet (ash, View, origin);
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

			table = AppDelegate.create_table_and_background (View, 1);
			table.RowHeight = 80; // CONSTANT in code

			panelButtons = BottomPanelView.create_bottom_panel (View);

			btnHelp = BottomPanelView.create_help_button( panelButtons, "help");
			btnPlus = BottomPanelView.create_bottom_button( panelButtons, "plus");

			btnPlus.TouchUpInside += (sender, e) => {
				NavigationController.PushViewController(NameVC.create_or_reuse(doc, i18n.get ("CategoryTitleNew"), i18n.get ("CategoryNamePlaceholder"),false,false,false,"", "", (nvc, str) => {
					FlurryAnalytics.Flurry.LogEvent("Category", NSDictionary.FromObjectsAndKeys(new object[]{"New"}, new object[]{"action"}));
					AppDelegate.app.docs.execute_change(new ChangeCategoryCreate(doc, new DBCategory(){name=str, name_key="", sign=allowed_sign, image_name="CatUser.png"}));
				}), true);
			};

			btnHelp.TouchUpInside += (sender, e) => {
				LayoutForHelp lh = new LayoutForHelp(NavigationController, table.Frame.Height);
				// From top
/*				if( in_chain ) {
					table.SetContentOffset(new PointF(0, 0), false);
					Category4Cell cell = table.CellAt( NSIndexPath.FromRowSection(0, 0) ) as Category4Cell;
					if( cell != null) {
						UIView bubble = lh.create_help_label(LayoutForHelp.LARGE_WIDTH, cell.square_views[0], 0, "Use special categories to record loans, transfers and exchanges", LayoutForHelp.BubleType.BUTTON);
						foreach(var v in cell.square_views){
							if( !v.Hidden )
								lh.create_tail(v, 0, bubble);
						}
					}
				}*/

				// From bottom
				Category4Cell help_cell = null;
				foreach (var ip in table.IndexPathsForVisibleRows) {
					Category4Cell c4 = table.CellAt(ip) as Category4Cell;
					if (c4 != null) {
						if( lh.view_y_to_dark_view_y(c4, 0) < lh.last_pos_from_top )
							continue;
						if( ip.Section == 0 ) // Special section has no tap&hold :)
							continue;
						if( help_cell == null || c4.Frame.Y < help_cell.Frame.Y )
							help_cell = c4;
					}
				}
				if( help_cell != null )
					lh.create_help_label(LayoutForHelp.MEDIUM_WIDTH, help_cell.square_views[0], 0, i18n.get ("HelpTapHold"), LayoutForHelp.BubleType.HOLD);
				lh.show ();
			};

			table.Source = new TableSource (this);
			//AppDelegate.app.docs.anything_changed += (docs, e) => {
			//	ts.update();
			//};
		}
		private void anything_changed (Documents doc, Documents.DocumentChangeEventArgs e)
		{
			anything_changed ();
		}
		private NSIndexPath anything_changed()
		{
			categories_section = new List< List<DBCategory> >(){
				new List<DBCategory>(),new List<DBCategory>(),
				new List<DBCategory>(),new List<DBCategory>()
			};
			if( in_chain )	{
				if( doc.sorted_accounts.Count > 1 )
					categories_section[SPECIAL].Add(doc.get_category(Document.TRANSFER_CATEGORY));
				categories_section[SPECIAL].Add(doc.get_category(Document.CONVERT_CATEGORY));
				categories_section[SPECIAL].Add(doc.get_category(Document.LOAN_CATEGORY));
				categories_section[SPECIAL].Add(doc.get_category(Document.LOAN_BACK_CATEGORY));
			}
			DateTime now = DateTime.Now;
			List<DBCategory> sc = doc.get_signed_categories(allowed_sign);
			foreach(DBCategory cat in sc)
			{
				int expenses_count;
				long recent_expense_date;
				doc.get_category_count_date (cat.id, out expenses_count, out recent_expense_date);
				if( expenses_count == 0 )
				{
					categories_section[NEVER_USED].Add(cat);
					continue;
				}
				if (recent_expense_date == 0) { // The only operations with this category are without date, safe assumption it is old
					categories_section[OLD].Add(cat);
					continue;
				}
				DateTime dt = new DateTime(recent_expense_date, DateTimeKind.Local);
				if( now.Subtract(dt).TotalDays < 40 )
					categories_section[RECENT].Add(cat);
				else
					categories_section[OLD].Add(cat);
			}
			table.ReloadData ();
			NSIndexPath sel_path = null;
			for (int s = 0; s != 4; ++s) {
				int ind = categories_section [s].FindIndex (c => c.id == category);
				if(ind != -1) {
					sel_path = NSIndexPath.FromRowSection(ind/columns, s);
					break;
				}
			}
			if(NavigationItem.RightBarButtonItem != null)
				NavigationItem.RightBarButtonItem.Enabled = sel_path != null;
			return sel_path;
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			AppDelegate.app.docs.anything_changed += anything_changed;
			NSIndexPath sel_path = anything_changed ();
			if (sel_path != null)
				table.ScrollToRow (sel_path, UITableViewScrollPosition.Middle, false);
//			else
//				table.SetContentOffset( PointF.Empty, false);
		}
		public override void ViewWillDisappear (bool animated)
		{
			AppDelegate.app.docs.anything_changed -= anything_changed;
			base.ViewWillDisappear (animated);
		}
		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();
			BottomPanelView.layout (panelButtons, btnHelp);
		}
		// TODO - hide sections which contain no items
		public class TableSource : UITableViewSource {
			public List<string> section_names;
			private CategoryVC parent_vc;
			//private Category4Cell measure_4cell;
			//private ButtonCell measure_bcell;
			//private HintCell measure_hcell;
			public TableSource (CategoryVC parent_vc)
			{
				//measure_4cell = Category4Cell.Create(null, null);
				//measure_bcell = ButtonCell.Create(null);
				//measure_hcell = HintCell.Create();
				this.parent_vc = parent_vc;
				section_names = new List<string>(){null,
					i18n.get ("CategoriesRecent"),
					i18n.get ("CategoriesNotRecent"),
					i18n.get ("CategoriesNeverUsed")
				};
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (UITableViewCell c in tableView.VisibleCells) {
					Category4Cell c4 = c as Category4Cell;
					if (c4 != null) {
						for(int i = 0; i != parent_vc.columns; ++i)
						{
							CategorySquareView csv = c4.square_views [i];
							csv.set_selection (csv.cat_id == parent_vc.category);
						}
					}
				}
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return section_names.Count;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				if (section_names [(int)section] == null)
					return null;
				if( RowsInSection(tableView, section) == 0 )
					return null;
				UILabel label;
				UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);
				//SectionHeader sh = SectionHeader.create_or_get_header ();
				label.Text = section_names [(int)section];
				return hfv;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				if (section_names [(int)section] == null)
					return 0f;
				if( RowsInSection(tableView, section) == 0 )
					return 0f;
				return SectionHeader2.categories_height;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return (parent_vc.categories_section[(int)section].Count + parent_vc.columns - 1)/parent_vc.columns;
			}
			void category_click(string cat_id)
			{
				parent_vc.category = cat_id;
				updateVisibleCellCheckmarks(parent_vc.table);
				parent_vc.on_save.Invoke(parent_vc, parent_vc.category);
				if( !parent_vc.in_chain )
					Utility.dismiss_or_pop(parent_vc.NavigationController, true);
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				Category4Cell cell = (Category4Cell)tableView.DequeueReusableCell (Category4Cell.Key);
				if (cell == null) {
					WeakReference weak_this = new WeakReference (this);
					cell = Category4Cell.Create( tableView, (cat_id) =>{
						TableSource strong_this = weak_this.Target as TableSource;
						if( strong_this == null )
							return;
						strong_this.category_click(cat_id);
					},(cat_id, origin) =>{
						TableSource strong_this = weak_this.Target as TableSource;
						if( strong_this == null )
							return;
						strong_this.parent_vc.category_taphold(cat_id, origin);
					});
				}
				List<DBCategory> cats = parent_vc.categories_section [indexPath.Section];
				for(int i = 0; i != parent_vc.columns; ++i)
				{
					int rr = indexPath.Row * parent_vc.columns + i;
					CategorySquareView csv = cell.square_views [i];
					if( rr < cats.Count ) {
						string cat_id = cats [rr].id;
						DBCategory category = parent_vc.doc.get_category (cat_id);
						csv.set_category(category, cat_id == parent_vc.category);
					}else{
						csv.set_category (null, false);
					}
				}
				nint ris = RowsInSection (tableView, indexPath.Section);
				cell.separator.Hidden = indexPath.Row == ris - 1;
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

