using System;
using System.Drawing;
using Foundation;
using UIKit;
using AddressBookUI;
using SmartBudgetCommon;
using System.Collections.Generic;
using System.Globalization;
using CoreGraphics;

namespace SmartBudgetiOS
{
	public partial class NameVC : UIViewController, Utility.KeyboardListener
	{
		bool from_contacts;
		bool allow_empty;
		bool in_chain;
		string name_placeholder;
		string text;
		DBCategory category;
		Action<NameVC, string> on_save;
		private List< KeyValuePair<string,int> > suggestions = new List<KeyValuePair<string,int>>();
		int cat_border; // Vefore it suggestions refer to category
		private Document doc;
		UIBarButtonItem next_button;
		UIBarButtonItem cancel_button;
		UIBarButtonItem save_button;
		public NameVC () : base ("NameVC", null)
		{
			Utility.add_keyboard_listener (this);
			next_button = new UIBarButtonItem (i18n.get ("Next"), UIBarButtonItemStyle.Plain, (sender, e) => {
				FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"Next"}, new object[]{"action"}));
				on_save.Invoke (this, this.text);
			});
			save_button = new UIBarButtonItem (UIBarButtonSystemItem.Save, (sender, e) => {
				on_save.Invoke (this, this.text);
				FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"Save"}, new object[]{"action"}));
				Utility.dismiss_or_pop(this.NavigationController, true);
			});
			cancel_button = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, (sender, e) => {
				Utility.dismiss_or_pop(this.NavigationController, true);
			});
		}
		private void construct(Document doc, string title, string name_placeholder, bool from_contacts, bool allow_empty, bool in_chain, string initial_text, string category, Action<NameVC, string> on_save)
		{
			this.doc = doc;
			this.name_placeholder = name_placeholder;
			this.from_contacts = from_contacts;
			this.allow_empty = allow_empty;
			this.in_chain = in_chain;
			this.text = String.IsNullOrEmpty(initial_text) ? "" : initial_text;
			this.category = String.IsNullOrEmpty(category) ? null : doc.get_category(category);
			this.on_save = on_save;
			NavigationItem.Title = title;
			if (in_chain) {
				NavigationItem.RightBarButtonItem = next_button;
				NavigationItem.LeftBarButtonItem = null;
			}
			else {
				NavigationItem.RightBarButtonItem = save_button;
				NavigationItem.LeftBarButtonItem = cancel_button;
			}
		}
		~NameVC()
		{
			Console.WriteLine ("~NameVC");
		}
		private static Utility.ReuseVC<NameVC> reuse = new Utility.ReuseVC<NameVC> ();
		public static NameVC create_or_reuse(Document doc, string title, string name_placeholder, bool from_contacts, bool allow_empty, bool in_chain, string initial_text, string category, Action<NameVC, string> on_save)
		{
			NameVC result = reuse.create_or_reuse();
			result.construct(doc, title, name_placeholder, from_contacts, allow_empty, in_chain, initial_text, category, on_save);
			return result;
		}
		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();

			if (!IsViewLoaded) {
				name = Utility.free_view (name);
				table = Utility.free_view (table);
				//Utility.free_token (ref keyboard_did_show_token);
				//Utility.free_token (ref keyboard_did_hide_token);
			}
		}
		public void on_keyboard_show (CGRect endFrame)
		{
			if (!IsViewLoaded)
				return;
			endFrame = tableParagon.Superview.ConvertRectFromView(endFrame, null);
			CGRect rect = tableParagon.Frame;
			rect.Height = endFrame.Y - rect.Y;
			tablePlace.Frame = rect;
		}
		public void on_keyboard_hide ()
		{
			if (!IsViewLoaded)
				return;
			tablePlace.Frame = tableParagon.Frame;
		}
		partial void nameChanged (UIKit.UITextField sender)
		{
			this.text = name.Text;
			update_suggestions();
		}
/*		class TextFieldDelegate : UITextFieldDelegate {
			NameVC parent_vc;
			public TextFieldDelegate(NameVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override bool ShouldReturn (UITextField textField)
			{
				parent_vc.on_save.Invoke(parent_vc, parent_vc.text);
				if(!parent_vc.in_chain)
					Utility.dismiss_or_pop(parent_vc.NavigationController, true);
				Console.WriteLine ("ShouldReturn");
				return false;
			}
		};*/
		private bool name_should_return(UITextField tf)
		{
			FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"Return"}, new object[]{"action"}));
			this.on_save.Invoke(this, this.text);
			if(!this.in_chain)
				Utility.dismiss_or_pop(this.NavigationController, true);
			return false;
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = AppDelegate.app.dark_background_color;
			tablePlace.BackgroundColor = AppDelegate.app.dark_background_color;
			AppDelegate.create_table_background(View, 0, tablePlace.Frame.Y, UIViewAutoresizing.FlexibleBottomMargin);
			table = AppDelegate.create_table_and_background (tablePlace, from_contacts ? 1 : 0, false);

			btnContacts = AppDelegate.create_flat_bottom_button(tablePlace, i18n.get("SelectFromContacts"), 0);

			btnContacts.TouchUpInside += (sender, e) => {
				FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"SelectContactStart"}, new object[]{"action"}));
				ABPeoplePickerNavigationController abp = new ABPeoplePickerNavigationController();
				abp.SelectPerson2 += (sender2, e2) => {
					FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"SelectContact"}, new object[]{"action"}));
					//e2. = false;
					this.name.Text = this.text = ExpenseVC.format_person(e2.Person);
					this.update_suggestions ();
					this.DismissViewController(true, delegate {
						on_save.Invoke (this, this.text);
						if( !in_chain )
							Utility.dismiss_or_pop(this.NavigationController, true);
					}); // dismiss people picker
				};
				abp.Cancelled += (sender2, e2) => {
					this.DismissViewController(true, null); // dismiss people picker
				};
				this.PresentViewController(abp, true, null);
			};
			btnContacts.Hidden = !from_contacts;
			if (nameZlo != null)
				nameZlo.InputView = new UIView (new RectangleF(0,0,40,40));

			/*name.EditingChanged += (sender, e) => {
				this.update_suggestions();
			};*/
			//name.Delegate = new TextFieldDelegate (this);
			//WeakReference weak_this = new WeakReference (this);
/*			(tf) => {
				NameVC strong_this = weak_this.Target as NameVC;
				if( strong_this == null )
					return false;
				Console.WriteLine ("ShouldReturn");
				return false;
			};*/
			name.Placeholder = name_placeholder;
			name.ReturnKeyType = in_chain ? UIReturnKeyType.Next : UIReturnKeyType.Done;

			table.Source = new TableSource (this);

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
				nameZlo = new UITextField(new RectangleF(100,20, 100, 40));
				nameZlo.Hidden = true;
				nameZlo.InputView = new UIView (new RectangleF(0,0,40,40));
				View.InsertSubview (nameZlo, 0);
			}

			Utility.fix_rtl_textfield (name);
			Utility.fix_rtl_view (imgSeparator);
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);
			name.ShouldReturn += name_should_return;
			name.Text = text;
			update_suggestions ();
			name.BecomeFirstResponder ();
		}
		public override void ViewWillDisappear(bool animated)
		{
			name.ShouldReturn -= name_should_return;
			if (nameZlo != null)
				nameZlo.BecomeFirstResponder ();
			else
				name.ResignFirstResponder ();
		}
		static int sort_by_value_key(KeyValuePair<string,int> p1, KeyValuePair<string,int> p2)
		{
			int dif = -p1.Value.CompareTo(p2.Value); // more first
			if (dif != 0)
				return dif;
			return p1.Key.CompareTo(p2.Key); // by alphabet
		}
		void update_suggestions()
		{
			NavigationItem.RightBarButtonItem.Enabled = allow_empty || text.Length > 0;
			suggestions = new List<KeyValuePair<string,int>>();
			cat_border = 0;
			if (category != null) {
				Dictionary<string, HashSet<string>> cat_sug;
				Dictionary<string, HashSet<string>> all_sug;
				if (!from_contacts ) {
					cat_sug = doc.fill_suggestion (category.id, text);
					all_sug = doc.fill_suggestion ("", text);
				} else {
					cat_sug = doc.fill_suggestion (category.id, text);
					all_sug = doc.fill_suggestion (category.id == Document.LOAN_CATEGORY ? Document.LOAN_BACK_CATEGORY : Document.LOAN_CATEGORY, text);
				}
				foreach (var sug in cat_sug) {
					HashSet<string> val;
					if( all_sug.TryGetValue (sug.Key, out val) ) {
						sug.Value.UnionWith (val);
						all_sug.Remove (sug.Key);
					}
					suggestions.Add(new KeyValuePair<string, int>(sug.Key, sug.Value.Count));
				}
				List<KeyValuePair<string,int>> all_suggestions = new List<KeyValuePair<string,int>>();
				foreach (var sug in all_sug) {
					all_suggestions.Add(new KeyValuePair<string, int>(sug.Key, sug.Value.Count));
				}
				suggestions.Sort (sort_by_value_key);
				all_suggestions.Sort (sort_by_value_key);
				cat_border = suggestions.Count;
				suggestions.AddRange (all_suggestions);
				if (from_contacts ) // For user there is no border between loan and loan back
					cat_border = suggestions.Count;
			}
			table.UserInteractionEnabled = suggestions.Count != 0;
			table.ReloadData ();
		}
		public class TableSource : UITableViewSource {
			NameVC parent_vc;
			public TableSource (NameVC parent_vc)
			{
				this.parent_vc = parent_vc;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return parent_vc.suggestions.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCell cell = (SimpleCell)tableView.DequeueReusableCell (SimpleCell.Key);
				if (cell == null)
					cell = SimpleCell.Create(tableView);
				UIImage img = indexPath.Row < parent_vc.cat_border ? AppDelegate.app.get_category_image(parent_vc.category) : null;
				cell.setImageName (img, parent_vc.suggestions [indexPath.Row].Key, parent_vc.suggestions [indexPath.Row].Value.ToString()); // Culture ok
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				FlurryAnalytics.Flurry.LogEvent("Name", NSDictionary.FromObjectsAndKeys(new object[]{"SuggestionClick"}, new object[]{"action"}));
				parent_vc.name.Text = parent_vc.text = parent_vc.suggestions [indexPath.Row].Key;
				parent_vc.update_suggestions ();
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

