using System;
using SQLite;
using System.Json;
using System.Collections.Generic;

namespace SmartBudgetCommon
{
	public class DBCategory
	{
		[PrimaryKey]
		public string id { get; set; }
		public int sign { get; set; }
		[MaxLength(128)]
		public string name { get; set; }
		[MaxLength(32)]
		public string name_key { get; set; }
		[MaxLength(32)]
		public string image_name { get; set; }
		public byte[] image_data { get; set; }
		public bool removed { get; set; } // for expense resurrector

		public string get_loc_name()
		{
			if( !String.IsNullOrEmpty(name_key) )
				return i18n.get (name_key);
			return name;
		}
		public DBCategory()
		{
		}
		public DBCategory(JsonObject dic)
		{
			//name = "";
			update_from_diff (dic);
		}
		public DBCategory Clone()
		{
			return new DBCategory () { id=id,sign=sign,name=name,
				name_key=name_key,image_name=image_name,image_data=image_data,
				removed=removed
			};
		}
		public void update_from_diff(JsonObject dic)
		{
			name = JSONHelper.read_string (dic, "name", name);
			sign = JSONHelper.read_int (dic, "sign", sign);
			name_key = JSONHelper.read_string (dic, "name_key", name_key);
			image_name = JSONHelper.read_string (dic, "image_name", image_name);
			try{
				string image_data_b64 = dic["image_data_b64"];
				image_data = (image_data_b64.Length == 0) ? null : Convert.FromBase64String(image_data_b64);
			}catch(Exception){
				// Not found - keep image_data
			}
		}
		public void save(JsonObject dic)
		{
			if (!String.IsNullOrEmpty(name))
				dic.Add ("name",name); 
			dic.Add ("sign",sign); 
			if (!String.IsNullOrEmpty(name_key))
				dic.Add ("name_key",name_key); 
			if (!String.IsNullOrEmpty(image_name))
				dic.Add ("image_name",image_name); 
			if(image_data!=null)
				dic.Add ("image_data_b64", Convert.ToBase64String (image_data));
		}
		public void save_diff(JsonObject dic, DBCategory was)
		{
			if( name != was.name )
				dic.Add ("name", String.IsNullOrEmpty(name) ? "" : name); 
			if( sign != was.sign)
				dic.Add ("sign",sign); 
			if ( name_key != was.name_key)
				dic.Add ("name_key", String.IsNullOrEmpty(name_key) ? "" : name_key); 
			if ( image_name != was.image_name )
				dic.Add ("image_name", String.IsNullOrEmpty(image_name) ? "" : image_name); 
			if( !ByteArrayEqualityComparer.SEquals(image_data, was.image_data) )
				dic.Add ("image_data_b64", image_data == null ? "" : Convert.ToBase64String (image_data));
		}
		public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
		{
			static public bool SIEquals(byte[] x, byte[] y)
			{
				if (ReferenceEquals (x, y))
					return true;
				if (x == null || y == null)
					return false;
				if( x.Length != y.Length )
					return false;
				for (int i = 0; i != x.Length; ++i)
					if (x [i] != y [i])
						return false;
				return true;
			}
			static public bool SEquals(byte[] x, byte[] y)
			{
				if (SGetHashCode (x) != SGetHashCode (y))
					return false;
				return SIEquals (x, y);
			}
			public bool Equals(byte[] x, byte[] y)
			{
				return SIEquals (x, y);
			}

			static public int SGetHashCode(byte[] obj)
			{
				if (obj == null)
					return 0;
				int code = obj.Length;
				if (obj.Length > 0) {
					code += 100 * obj [0];
					code += 1000 * obj [obj.Length-1];
					code += 10000 * obj [obj.Length/2];
				}
				return code;
			}
			public int GetHashCode(byte[] obj)
			{
				return SGetHashCode (obj);
			}
		}
	}
}

