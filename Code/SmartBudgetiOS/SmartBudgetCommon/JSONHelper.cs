using System;
using System.Json;

namespace SmartBudgetCommon
{
	public static class JSONHelper
	{
		public static JsonArray read_array(JsonObject obj, string key)
		{
			try{
				return (JsonArray)obj[key];
			}catch(Exception){
			}
			return null;
		}
		public static JsonObject read_object(JsonObject obj, string key)
		{
			try{
				return (JsonObject)obj[key];
			}catch(Exception){
			}
			return null;
		}
		public static long read_long(JsonObject obj, string key, long def)
		{
			try{
				return obj[key];
			}catch(Exception){
			}
			return def;
		}
		public static int read_int(JsonObject obj, string key, int def)
		{
			try{
				return obj[key];
			}catch(Exception){
			}
			return def;
		}
		public static bool read_bool(JsonObject obj, string key, bool def)
		{
			return read_int (obj, key, def ? 1 : 0) != 0;
		}
		public static string read_string(JsonObject obj, string key, string def)
		{
			try{
				return obj[key];
			}catch(Exception){
			}
			return def;
		}
		public static double read_double(JsonObject obj, string key, double def)
		{
			try{
				return obj[key];
			}catch(Exception){
			}
			return def;
		}
		public static decimal read_decimal(JsonObject obj, string key, decimal def)
		{
			try{
				return obj[key];
			}catch(Exception){
			}
			return def;
		}
		public static long read_decimal_1000(JsonObject obj, string key, long def_1000)
		{
			try{
				return DBExpense.from_decimal(obj[key]);
			}catch(Exception){
			}
			return def_1000;
		}
	}
}

