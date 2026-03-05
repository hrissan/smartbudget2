using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

namespace SmartBudgetCommon
{
	public class SimpleZipPacker
	{
		class Info {
			public int offset;
			public byte[] filename_utf8;
			public byte[] header;
		}
		List<Info> infos = new List<Info>();
		MemoryStream stream = new MemoryStream();

		static void fill2(byte[] dst, int offset, int value)
		{
			dst[offset+0] = (byte)(value & 0xFF);
			dst[offset+1] = (byte)((value >> 8) & 0xFF);
		}
		static void fill4(byte[] dst, int offset, int value)
		{
			dst[offset+0] = (byte)(value & 0xFF);
			dst[offset+1] = (byte)((value >> 8) & 0xFF);
			dst[offset+2] = (byte)((value >> 16) & 0xFF);
			dst[offset+3] = (byte)((value >> 24) & 0xFF);
		}

		public void add_file(string filename, byte[] data)
		{
			//		local file header signature        4 bytes  00	0	504B0304
			//		version needed to extract          2 bytes  04	4	1400
			//      general purpose bit flag           2 bytes  06	6	0200
			//      compression method                 2 bytes  08	8	0000
			//      last mod file time                 2 bytes	0A	10	34B5
			//      last mod file date                 2 bytes	0C	12	F43E
			//      crc-32                             4 bytes	0E	14	1C30A1C7
			//      compressed size                    4 bytes	12	18	18000000	(24)
			//      uncompressed size                  4 bytes	16	22	18000000	(24)
			//      filename length                    2 bytes	1A	26	0E00		(13)
			//      extra field length                 2 bytes	1C	28	0000		(0)
			Info info = new Info (){offset=(int)stream.Length,filename_utf8=Encoding.UTF8.GetBytes (filename)};
			infos.Add (info);
			byte[] crc32 = new Crc32 ().ComputeHash (data);
			byte[] header = new byte[30]{ 0x50, 0x4B, 0x03, 0x04, 0x14, 0, 0x02, 0, 0, 0, 0x34, 0xB5, 0xF4, 0x3E,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };

			for (int i = 0; i != 4; ++i)
				header [14+i] = crc32 [i];
			fill4( header, 18, data.Length );
			fill4( header, 22, data.Length );
			fill2( header, 26, info.filename_utf8.Length );

			stream.Write (header, 0, header.Length);
			stream.Write (info.filename_utf8, 0, info.filename_utf8.Length);
			stream.Write (data, 0, data.Length);

			info.header = header;
		}
		public byte[] finish()
		{
			int dataoffset = (int)stream.Length;

			for ( int i = 0; i < infos.Count; i++ )
			{
				Info info = infos[i];
				//central file header signature      4 bytes  	00 	0	504B0102
				//version made by                    2 bytes	04	4	1400
				//version needed to extract          2 bytes	06	6	1400
				//general purpose bit flag           2 bytes		8	0200
				//compression method                 2 bytes		10	0000
				//last mod file time                 2 bytes		12	799B
				//last mod file date                 2 bytes		14	CC32
				//crc-32                             4 bytes		16	40FBCA2A
				//compressed size                    4 bytes		20	49000000
				//uncompressed size                  4 bytes		24	49000000
				//filename length                    2 bytes		28	0900
				//extra field length                 2 bytes		30	0000
				//file comment length                2 bytes		32	0000
				//disk number start                  2 bytes		34	0000
				//internal file attributes           2 bytes		36	0000
				//external file attributes           4 bytes		38	20000000
				//relative offset of local header    4 bytes		42	44000000

				byte[] header = new byte[46]{ 0x50, 0x4B, 0x01, 0x02, 0x14, 0x00,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
				Array.Copy (info.header, 4, header, 6, 26);
				//std::copy(info.header.begin(), info.header.end(), header + 6);
				header[38] = 0x20;
				fill4(header, 42, info.offset);

				stream.Write(header, 0, header.Length);
				stream.Write(info.filename_utf8, 0, info.filename_utf8.Length);
			}

			int dataoffset2 = (int)stream.Length;

			//end of central dir signature       4 bytes		0	504B0506
			//number of this disk                2 bytes		4	0000
			//number of the disk with the	
			//start of the central directory     2 bytes		6	0000
			//total number of entries in
			//   the central dir on this disk    2 bytes		8	0100
			//total number of entries in
			//   the central dir                 2 bytes		10	0100
			//size of the central directory      4 bytes		12	13000000
			//offset of start of central
			//   directory with respect to
			//   the starting disk number        4 bytes		16	70000000
			//zipfile comment length             2 bytes		20	0000
			//zipfile comment (variable size)
			byte[] header2 = new byte[22]{ 0x50, 0x4B, 0x05, 0x06,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
			fill2(header2, 8, infos.Count);
			fill2(header2, 10, infos.Count);
			fill4(header2, 12, dataoffset2 - dataoffset);
			fill4(header2, 16, dataoffset);
			stream.Write (header2, 0, header2.Length);
			return stream.ToArray ();
		}
	}
	public class XLSXStringsExporter
	{
		private StringBuilder strings_data = new StringBuilder();
		private StringBuilder sheet_data = new StringBuilder();
		private int st = 0;
		private int row = 1;
		public XLSXStringsExporter ()
		{
		}
		public void start_row()
		{
			sheet_data.AppendFormat ("<row r=\"{0}\" spans=\"1:13\">", row);
		}
		public void add_value(string letter, string value)
		{
			if (String.IsNullOrEmpty(value))
				return;
			value = value.Replace ("&", "&amp;").Replace ("<", "&lt;").Replace (">", "&gt;");
			strings_data.AppendFormat ("<si><t>{0}</t></si>", value);
			sheet_data.AppendFormat ("<c r=\"{0}{1}\" t=\"s\"><v>{2}</v></c>", letter, row, st);
			st += 1;
		}
		public void add_value(string letter, decimal value)
		{
			sheet_data.AppendFormat("<c r=\"{0}{1}\"><v>{2}</v></c>", letter, row, value.ToString(CultureInfo.InvariantCulture));
		}
		public void finish_row()
		{
			sheet_data.Append ("</row>");
			row += 1;
		}
		public static string path_to_xlsx_template;
		public byte[] finish()
		{
			string dim_str = row.ToString(CultureInfo.InvariantCulture);
			string sst_count_str = st.ToString(CultureInfo.InvariantCulture);

			SimpleZipPacker packer = new SimpleZipPacker();

			string [] files = new string[]{
				"_rels/.rels",
				"[Content_Types].xml",
				"docProps/app.xml",
				"docProps/core.xml",
				"xl/_rels/workbook.xml.rels",
				//        @"xl/sharedStrings.xml",
				"xl/styles.xml",
				"xl/theme/theme1.xml",
				"xl/workbook.xml"
				//        @"xl/worksheets/sheet1.xml"
			};

			string sha_path = Path.Combine(path_to_xlsx_template, "xl/sharedStringsStripped.xml");
			string sha_str = File.ReadAllText (sha_path, Encoding.UTF8);
			sha_str = sha_str.Replace ("%COUNT%", sst_count_str).Replace ("%BODY%", strings_data.ToString());

			string sheet_path = Path.Combine(path_to_xlsx_template, "xl/worksheets/sheet1Stripped.xml");
			string sheet_str = File.ReadAllText (sheet_path, Encoding.UTF8);
			sheet_str = sheet_str.Replace("%DIM%",dim_str).Replace("%BODY%",sheet_data.ToString());

			packer.add_file("xl/sharedStrings.xml", Encoding.UTF8.GetBytes(sha_str));
			packer.add_file("xl/worksheets/sheet1.xml", Encoding.UTF8.GetBytes(sheet_str));

			for(int i = 0; i != files.Length; ++i)
			{
				string file_path = Path.Combine(path_to_xlsx_template, files[i]);
				packer.add_file(files[i], File.ReadAllBytes(file_path));
			}

			return packer.finish();
		}
	}
}

