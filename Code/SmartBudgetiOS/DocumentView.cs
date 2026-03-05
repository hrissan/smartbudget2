using System;
using System.Drawing;
using Foundation;
using UIKit;
using SmartBudgetCommon;
using System.Globalization;
using CoreGraphics;

namespace SmartBudgetiOS
{
	public partial class DocumentView : UIView
	{
		public static readonly UINib Nib = UINib.FromName ("DocumentView", NSBundle.MainBundle);
		public Document doc { get; private set; }
		private Action<Document> select_action;
		public UIView notebook_header;
		// TODO - make font much larger
		public DocumentView (IntPtr handle) : base (handle)
		{
		}
		~DocumentView()
		{
			Console.WriteLine ("~DocumentView");
		}
		public void setDocument(Document doc)
		{
			this.doc = doc;
			anything_changed ();
		}
		public UIView getTablePlace()
		{
			return table;
		}
		static private UIImage select_image = UIImage.FromBundle("new_design/Tap_to_select.png");
		static private UIImage new_image = UIImage.FromBundle("new_design/Tap_to_add_new.png");
		public void anything_changed()
		{
			table.ReloadData();
			//table.Hidden = (doc == null);
			if (doc != null)
				btnSelect.SetImage (select_image, UIControlState.Normal);
			else
				btnSelect.SetImage (new_image, UIControlState.Normal);
			panelNew.Hidden = (doc == null) || AppDelegate.app.docs.new_documents.IndexOf (doc.file_name) == -1;
		}
		public static DocumentView Create (Action<Document> select_action)
		{
			DocumentView dv = (DocumentView)Nib.Instantiate (null, null) [0];
			//dv.tablePlace.BackgroundColor = AppDelegate.table_background_color;
			dv.notebook_header = AppDelegate.create_notebook_header (dv);
			CGRect rr_cdv = dv.Bounds;
			rr_cdv.Y += AppDelegate.NOTEBOOK_HEADER_HEIGHT;
			rr_cdv.Height -= AppDelegate.NOTEBOOK_HEADER_HEIGHT;
			dv.table = AppDelegate.create_table_and_background (dv, rr_cdv.Y, rr_cdv.Height, true);
			dv.table.ScrollsToTop = false;
			dv.table.RowHeight = 50;
			dv.select_action = select_action;
			dv.btnSelect.TouchUpInside += (sender, e) => {
				dv.select_action(dv.doc);
			};
			dv.btnSelect.SetTitle ("", UIControlState.Normal);
			dv.btnSelect.TitleLabel.TextAlignment = UITextAlignment.Center;
			dv.btnSelect.ContentEdgeInsets = new UIEdgeInsets (80, 0, 0, 0);
			dv.table.Source = new TableSource (dv);
			dv.labelNew.Text = i18n.get ("NewDatabaseMark");
			dv.labelNew.Transform = CGAffineTransform.MakeRotation ((float)(Math.PI * -45 / 180));
			//PointF ce = dv.labelNew.Center;
			/*UILabelSelectFont la = new UILabelSelectFont (18, dv.imgNew.Frame);
			la.Text = i18n.get ("NewDatabaseMark");
			la.TextColor = AppDelegate.app.neutral_color;
			la.TextAlignment = UITextAlignment.Center;
			la.BackgroundColor = AppDelegate.app.positive_color;
			CGAffineTransform tr = CGAffineTransform.MakeIdentity ();
			tr.Rotate ((float)(Math.PI * -45 / 180));
			la.Transform = tr;// CGAffineTransform.MakeRotation ((float)(Math.PI * 45 / 180));
			la.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
			dv.AddSubview(la);*/
			//dv.labelNew.Center = ce;
//			AppDelegate.app.docs.anything_changed += (docs, e) => {
//				dv.anything_changed();
//			};
			return dv;
		}
		public class TableSource : UITableViewSource {
			DocumentView document_view;
			public TableSource (DocumentView document_view)
			{
				this.document_view = document_view;
			}
			public override nint NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (document_view.doc == null)
					return 0;
				return document_view.doc.sorted_sheets.Count;
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				SimpleCellBig cell = (SimpleCellBig)tableView.DequeueReusableCell (SimpleCellBig.Key);
				if (cell == null)
					cell = SimpleCellBig.Create(tableView);
				DBSheet sh = document_view.doc.sorted_sheets [indexPath.Row];
				int expense_count;
				long date;
				document_view.doc.get_sheet_count_date(sh.id, out expense_count, out date);
				string cou = expense_count.ToString (); // Culture ok
				long planned_date;
				document_view.doc.get_sheet_next_planned_date (sh.id, out planned_date, true);

				cell.setNameValue(Documents.planned_soon(planned_date) ? AppDelegate.get_attention_icon_small() : null, sh.get_loc_name(), cou);
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}	
	}
}

