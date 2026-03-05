using System;
using System.Drawing;
using Foundation;
using UIKit;
using System.Collections.Generic;
using SmartBudgetCommon;

namespace SmartBudgetiOS
{
	public class ImageSelectVC : UIViewController
	{
		private UITableView table;
		//private TableSource ts;
		List<string> all_images;
		List<byte[]> all_image_data = new List<byte[]>();

		string image;
		byte[] image_data;

		Action<ImageSelectVC,string,byte[]> save_action;

		public ImageSelectVC ()// : base ("ImageSelectVC", null)
		{
			NavigationItem.Title = i18n.get("ImageTitle");

			all_images = new List<string> ();
			all_images.Add( "CatApartment.png");
			all_images.Add( "CatFoodHome.png");
			all_images.Add( "CatFoodRestaurant.png");
			all_images.Add( "CatCarPlanned.png");
			all_images.Add( "CatCarEmergency.png");
			all_images.Add( "CatSport.png");
			all_images.Add( "CatCat.png");
			all_images.Add( "CatDog.png");
			all_images.Add( "CatPet.png");
			all_images.Add( "CatGifts.png");
			all_images.Add( "CatTransportPublic.png");
			all_images.Add( "CatFishing.png");
			all_images.Add( "CatHunting.png");
			all_images.Add( "CatGambling.png");
			all_images.Add( "CatFamily.png");
			all_images.Add( "CatBaby.png");
			all_images.Add( "CatMedicine.png");
			all_images.Add( "CatGadgets.png");
			all_images.Add( "CatCinema.png");
			all_images.Add( "CatTheaters.png");
			all_images.Add( "CatRoleplaying.png");

			all_images.Add( "CatSalary.png");
			all_images.Add( "CatOverwork.png");
			all_images.Add( "CatBonus.png");
			all_images.Add( "CatApple.png");
			all_images.Add( "CatFound.png");

			all_images.Add( "CatGovernment.png");
			all_images.Add( "CatClothes.png");
			all_images.Add( "CatSpirits.png");
			all_images.Add( "CatCommunications.png");
			all_images.Add( "CatTransportTravel.png");
			all_images.Add( "CatHotel.png");
			all_images.Add( "CatEducation.png");
			all_images.Add( "CatUnknown.png");
			all_images.Add( "CatCelebration.png");
			//all_images.Add( "CatFamily.png"); // Took from family, gave to family - same picture

			all_images.Add( "CatUser.png");
		}
		void construct(string initial_image, byte[] initial_image_data, Action<ImageSelectVC, string, byte[]> save_action)
		{
			this.save_action = save_action;
			image = initial_image;
			image_data = initial_image_data;
		}
		~ImageSelectVC()
		{
			Console.WriteLine ("~ImageSelectVC");
		}
		private static Utility.ReuseVC<ImageSelectVC> reuse = new Utility.ReuseVC<ImageSelectVC> ();
		public static ImageSelectVC create_or_reuse(string initial_image, byte[] initial_image_data, Action<ImageSelectVC, string, byte[]> save_action)
		{
			ImageSelectVC result = reuse.create_or_reuse();
			result.construct(initial_image, initial_image_data, save_action);
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
			table.RowHeight = 80;
			table.Source = new TableSource (this);;

			// We ignore anything_changed here.
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear (animated);

			anything_change ();

			int columns = Image4Cell.get_columns();

			int ind = String.IsNullOrEmpty(image) ? -1 : all_images.IndexOf(image);
			if (ind != -1)
				table.ScrollToRow (NSIndexPath.FromRowSection(ind/columns, 0), UITableViewScrollPosition.Middle, false);
			else {
				for(ind = 0; ind < all_image_data.Count; ++ind){
					if (DBCategory.ByteArrayEqualityComparer.SEquals(image_data, all_image_data[ind])){
						table.ScrollToRow (NSIndexPath.FromRowSection(ind/columns, 1), UITableViewScrollPosition.Middle, false);
						break;
					}
				}
			}
		}
		void anything_change()
		{
			HashSet<byte[]> list = new HashSet<byte[]> (new DBCategory.ByteArrayEqualityComparer());

			foreach (Document doc in AppDelegate.app.docs.documents) {
				for (int sign = -1; sign <= 1; sign +=2) {
					List<DBCategory> ec = doc.get_signed_categories (sign);
					foreach(var c in ec){
						//DBCategory cat = doc.get_category (c.id);
						if (c.image_data != null)
							list.Add (c.image_data);
					}
				}
			}
			all_image_data = new List<byte[]> (list);
		}
		public class TableSource : UITableViewSource {
			public List<string> section_names;
			public List< List<DBCategory> > categories_section;
			public const int SPECIAL = 0;
			public const int RECENT = 1;
			public const int OLD = 2;
			public const int NEVER_USED = 3;
			public const int BUTTONS = 4;
			public int columns;
			private ImageSelectVC parent_vc;
			public TableSource (ImageSelectVC parent_vc)
			{
				this.parent_vc = parent_vc;
				columns = Image4Cell.get_columns();
			}
			public void updateVisibleCellCheckmarks(UITableView tableView)
			{
				foreach (NSIndexPath ii in tableView.IndexPathsForVisibleRows) {
					Image4Cell c4 = tableView.CellAt(ii) as Image4Cell;
					if (c4 == null)
						continue;
					for(int i = 0; i != columns; ++i)
					{
						ImageSquareView csv = c4.square_views [i];
						int rr = ii.Row * columns + i;
						if(ii.Section == 0){
							if( rr < parent_vc.all_images.Count)
								csv.set_selection (parent_vc.image == parent_vc.all_images[rr]);
						}
						else{
							if( rr < parent_vc.all_image_data.Count)
								csv.set_selection (DBCategory.ByteArrayEqualityComparer.SEquals(parent_vc.image_data, parent_vc.all_image_data[rr]));
						}
					}
				}
			}
			public override nint NumberOfSections(UITableView tableview)
			{
				return 2;
			}
			public override UIView GetViewForHeader (UITableView tableView, nint section)
			{
				if (section == 0)
					return null;
				UILabel label;
				UITableViewHeaderFooterView hfv = SectionHeader2.deque_header(tableView, out label);
				//SectionHeader sh = SectionHeader.create_or_get_header ();
				label.Text = i18n.get ("CustomImagesSection");
				return hfv;
			}
			public override nfloat GetHeightForHeader (UITableView tableView, nint section)
			{
				if (section == 0)
					return 0;
				return SectionHeader2.categories_height;
			}
			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (section == 0)
					return (parent_vc.all_images.Count + columns - 1)/columns;
				return (parent_vc.all_image_data.Count + columns - 1)/columns;
			}
			/*public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return measure_4cell.Frame.Height;
			}*/
			private void on_image_click(ImageSquareView cc, int sec, int pos)
			{
				if( sec == 0 ){
					parent_vc.image = parent_vc.all_images[pos];
					parent_vc.image_data = null;
				}else{
					parent_vc.image = null;
					parent_vc.image_data = parent_vc.all_image_data[pos];
				}
				updateVisibleCellCheckmarks(parent_vc.table);
				parent_vc.save_action.Invoke(parent_vc, parent_vc.image, parent_vc.image_data);
				Utility.dismiss_or_pop(parent_vc.NavigationController, true);
			}
			public override UITableViewCell GetCell (UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				Image4Cell cell = (Image4Cell)tableView.DequeueReusableCell (Image4Cell.Key);
				if (cell == null) {
					WeakReference weak_this = new WeakReference (this);
					cell = Image4Cell.Create( tableView, (cc,sec,pos)=>{
						TableSource strong_this = weak_this.Target as TableSource;
						if( strong_this == null )
							return;
						strong_this.on_image_click(cc, sec, pos);
					});
				}
				for(int i = 0; i != columns; ++i)
				{
					int rr = indexPath.Row * columns + i;
					ImageSquareView csv = cell.square_views [i];
					csv.set_section_row (indexPath.Section, rr);
					if( indexPath.Section == 0 ) {
						if (rr < parent_vc.all_images.Count) {
							csv.set_image (AppDelegate.app.get_category_image(parent_vc.all_images[rr], null), parent_vc.image == parent_vc.all_images[rr]);
						} else {
							csv.set_image (null, false);
						}
					}else{
						if (rr < parent_vc.all_image_data.Count) {
							csv.set_image (AppDelegate.app.get_category_image("", parent_vc.all_image_data[rr]), DBCategory.ByteArrayEqualityComparer.SEquals(parent_vc.image_data, parent_vc.all_image_data[rr]));
						} else {
							csv.set_image (null, false);
						}
					}
				}
				return cell;
			}
			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				tableView.DeselectRow (indexPath, true); // normal iOS behaviour is to remove the blue highlight
			}
		}
	}
}

