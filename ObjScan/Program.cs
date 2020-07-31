﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SonicRetro.SAModel;
using SA_Tools;

namespace ObjScan
{
	class Program
	{
		static bool CheckNJSObject(byte[] datafile, int address, uint imageBase, string dir, string model_extension, bool recursive)
		{
			int flags = 0;
			uint attach = 0;
			uint child = 0;
			uint sibling = 0;
			uint vertices = 0;
			uint normals = 0;
			uint vert_count = 0;
			uint meshlists = 0;
			short mesh_count = 0;
			short mat_count = 0;
			Vertex pos;
			Vertex scl;
			flags = ByteConverter.ToInt32(datafile, address);
			if (flags > 0x3FFF || flags < 0) return false;
			attach = ByteConverter.ToUInt32(datafile, address + 4);
			if (attach != 0)
			{
				if (attach < imageBase) return false;
				if (attach > (uint)datafile.Length + imageBase - 52) return false;
				vertices = ByteConverter.ToUInt32(datafile, (int)(attach - imageBase));
				if (vertices > (uint)datafile.Length + imageBase - 52) return false;
				if (vertices < imageBase) return false;
				normals = ByteConverter.ToUInt32(datafile, (int)(attach - imageBase) + 4);
				if (normals != 0 && normals < imageBase) return false;
				if (normals > (uint)datafile.Length + imageBase - 52) return false;
				vert_count = ByteConverter.ToUInt32(datafile, (int)(attach - imageBase) + 8);
				if (vert_count > 2048 || vert_count == 0) return false;
				meshlists = ByteConverter.ToUInt32(datafile, (int)(attach - imageBase) + 0xC);
				if (meshlists != 0 && meshlists < imageBase) return false;
				if (meshlists > (uint)datafile.Length + imageBase - 52) return false;
				mesh_count = ByteConverter.ToInt16(datafile, (int)(attach - imageBase) + 0x14);
				if (mesh_count > 2048 || mesh_count < 0) return false;
				mat_count = ByteConverter.ToInt16(datafile, (int)(attach - imageBase) + 0x16);
				if (mat_count > 2048 || mat_count < 0) return false;
			}
			pos = new Vertex(datafile, address + 8);
			if (pos.X < -100000 || pos.X > 100000) return false;
			if (pos.Y < -100000 || pos.Y > 100000) return false;
			if (pos.Z < -100000 || pos.Z > 100000) return false;
			scl = new Vertex(datafile, address + 0x20);
			if (scl.X <= 0 || scl.X > 10000) return false;
			if (scl.Y <= 0 || scl.Y > 10000) return false;
			if (scl.Z <= 0 || scl.Z > 10000) return false;
			child = ByteConverter.ToUInt32(datafile, address + 0x2C);
			sibling = ByteConverter.ToUInt32(datafile, address + 0x30);
			if (child > (uint)address + imageBase) return false;
			if (sibling > (uint)address + imageBase) return false;
			if (child > (uint)datafile.Length + imageBase - 52) return false;
			if (sibling > (uint)datafile.Length + imageBase - 52) return false;
			if (child != 0 && child < imageBase) return false;
			if (sibling != 0 && sibling < imageBase) return false;
			if (recursive && child != 0 && !CheckNJSObject(datafile, (int)(child - imageBase), imageBase, dir, model_extension, false)) return false;
			if (recursive && sibling != 0 && !CheckNJSObject(datafile, (int)(sibling - imageBase), imageBase, dir, model_extension, false)) return false;
			if (attach == 0 && flags == 0) return false;
			if (recursive) Console.WriteLine("Model at {0}", address.ToString("X"));
			if (child != 0) File.Delete(dir + "\\" + (child - imageBase).ToString("X") + model_extension);
			if (sibling != 0) File.Delete(dir + "\\" + (sibling - imageBase).ToString("X") + model_extension);
			return true;
		}

		static int[] LandtableAddr = {
			0x23C7BCC,
			0xA99CB8,
			0xC39E9C,
			0xC386B4,
			0x8051E0,
			0x8046C0,
			0x80433C,
			0x22B975C,
			0x22B867C,
			0x22B6B34,
			0x22B1E98,
			0x22ADBC8,
			0x22ACF40,
			0x1E405E0,
			0x20C8B58,
			0x20C6F14,
			0x1E39ADC,
			0x1E369A0,
			0x1E34800,
			0x1C38B60,
			0x1C37C9C,
			0x1C34D14,
			0xA41D2C,
			0xA414BC,
			0xA409C4,
			0xA3E024,
			0x198A9D0,
			0x19887EC,
			0x1986A1C,
			0x1985C08,
			0x16600D0,
			0x15C8ED0,
			0x165CFA8,
			0x13D09C0,
			0x13CF288,
			0x13C9B48,
			0xD2136C,
			0xD90930,
			0xDEDE38,
			0xDED6F0,
			0x102478C,
			0x1170B1C,
			0x11EC454,
			0x125E990,
			0x12B4D38,
			0x10FCEE8,
			0x1122578,
			0x5C99A4,
			0x5C8170,
			0x5C6C30,
			0x5C585C,
			0x5C453C,
			0x5C3534,
			0x133EB64,
			0x300E738,
			0x3005E54,
			0x3023700,
			0x3024C58,
			0x2FCAC58,
		};

		static void Main(string[] args)
		{
			string[] arguments = Environment.GetCommandLineArgs();
			Game game;
			string filename;
			string dir;
			int address;
			bool bigendian = false;
			bool reverse = false;
			if (args.Length == 0)
			{
				Console.WriteLine("Object Scanner is a tool that scans a binary file or memory dump and extracts models from it.\nOnly SA1/SADX models are supported at the moment.");
				Console.WriteLine("Usage: objscan <GAME> <FILENAME> <KEY> <TYPE>\n");
				Console.WriteLine("Argument description:");
				Console.WriteLine("<GAME>: SA1, SADX. Add '_b' (e.g. SADX_b) to switch to Big Endian, use SADX_x to scan the X360 version.");
				Console.WriteLine("<FILENAME>: The name of the binary file, e.g. sonic.exe.");
				Console.WriteLine("<KEY>: Binary key, e.g. 400000 for sonic.exe or C900000 for SA1 STG file.");
				Console.WriteLine("<TYPE>: model, basicmodel, basicdxmodel\n");
				Console.WriteLine("Press ENTER to exit");
				Console.ReadLine();
				return;
			}
			switch (args[0].ToLowerInvariant())
			{
				case "sa1":
					game = Game.SA1;
					break;
				case "sa1_b":
					game = Game.SA1;
					bigendian = true;
					break;
				case "sadx":
					game = Game.SADX;
					break;
				case "sadx_b":
					game = Game.SADX;
					bigendian = true;
					break;
				case "sadx_x":
					game = Game.SADX;
					bigendian = true;
					reverse = true;
					break;
				/*case "sa2":
					game = Game.SA2;
					break;
				case "sa2_b":
					game = Game.SA2;
					bigendian = true;
					break;
				case "sa2b":
					game = Game.SA2B;
					break;
				case "sa2b_b":
					game = Game.SA2B;
					bigendian = true;
					break;
				*/
				default:
					Console.WriteLine("Error parsing game type.\nCorrect game types are: SA1, SADX, SA1_b, SADX_b");
					Console.WriteLine("Press ENTER to exit.");
					Console.ReadLine();
					return;
			}
			string model_extension = ".sa1mdl";
			//string landtable_extension = ".sa1lvl";
			ByteConverter.BigEndian = SonicRetro.SAModel.ByteConverter.BigEndian = bigendian;
			ByteConverter.Reverse = SonicRetro.SAModel.ByteConverter.Reverse = reverse;
			filename = args[1];
			byte[] datafile = File.ReadAllBytes(filename);
			if (Path.GetExtension(filename).ToLowerInvariant() == ".prs") datafile = FraGag.Compression.Prs.Decompress(datafile);
			Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(filename));
			uint imageBase = uint.Parse(args[2], NumberStyles.AllowHexSpecifier);
			string type = args[3];
			//bool SA2 = game == Game.SA2 | game == Game.SA2B;
			ModelFormat modelfmt = ModelFormat.BasicDX;
			//LandTableFormat landfmt = LandTableFormat.SADX;
			switch (game)
			{
				case Game.SA1:
					modelfmt = ModelFormat.Basic;
					//landfmt = LandTableFormat.SA1;
					model_extension = ".sa1mdl";
					//landtable_extension = ".sa1lvl";
					break;
				case Game.SADX:
					modelfmt = ModelFormat.BasicDX;
					//landfmt = LandTableFormat.SADX;
					model_extension = ".sa1mdl";
					//landtable_extension = ".sa1lvl";
					break;
				/*case Game.SA2:
					modelfmt = ModelFormat.Chunk;
					landfmt = LandTableFormat.SA2;
					model_extension = ".sa2mdl";
					landtable_extension = ".sa2lvl";
					break;
				case Game.SA2B:
					modelfmt = ModelFormat.Chunk;
					landfmt = LandTableFormat.SA2B;
					model_extension = ".sa2mdl";
					landtable_extension = ".sa2blvl";
					break;*/
			}
			string fileOutputPath;
			Console.WriteLine("Game: {0}, file: {1}, key: 0x{2}, scanning for {3}", game.ToString(), filename, imageBase.ToString("X"), type);
			dir = Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(filename);
			Directory.CreateDirectory(dir);
			for (int u = 0; u < datafile.Length - 52; u += 4)
			{
				//address = int.Parse(args[4], NumberStyles.AllowHexSpecifier);
				address = u;
				//Console.WriteLine("Address: {0}", address.ToString("X"));
				fileOutputPath = dir + "\\" + address.ToString("X8");
				if (!CheckNJSObject(datafile, address, imageBase, dir, model_extension, true)) continue;
				try
				{
					switch (type.ToLowerInvariant())
					{
						/*case "landtable":
							new LandTable(datafile, address, imageBase, landfmt).SaveToFile(fileOutputPath + landtable_extension, landfmt);
							break;
						*/
						case "model":
							{
								NJS_OBJECT mdl = new NJS_OBJECT(datafile, address, imageBase, modelfmt, new Dictionary<int, Attach>());
								ModelFile.CreateFile(fileOutputPath + model_extension, mdl, null, null, null, null, modelfmt);
								if (mdl.Children.Count > 0)
								{
									foreach (NJS_OBJECT child in mdl.Children)
									{
										Console.WriteLine("Deleting file {0}", dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
										File.Delete(dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
									}
								}
								if (mdl.Sibling != null)
								{
									Console.WriteLine("Deleting file {0}", dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
									File.Delete(dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
								}
							}
							break;
						case "basicmodel":
							{
								NJS_OBJECT mdl = new NJS_OBJECT(datafile, address, imageBase, ModelFormat.Basic, new Dictionary<int, Attach>());
								ModelFile.CreateFile(fileOutputPath + ".sa1mdl", mdl, null, null, null, null, ModelFormat.Basic);
								if (mdl.Children.Count > 0)
								{
									foreach (NJS_OBJECT child in mdl.Children)
									{
										Console.WriteLine("Deleting file {0}", dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
										File.Delete(dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
									}
								}
								if (mdl.Sibling != null)
								{
									Console.WriteLine("Deleting file {0}", dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
									File.Delete(dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
								}
							}
							break;
						case "basicdxmodel":
							{
								NJS_OBJECT mdl = new NJS_OBJECT(datafile, address, imageBase, ModelFormat.BasicDX, new Dictionary<int, Attach>());
								ModelFile.CreateFile(fileOutputPath + ".sa1mdl", mdl, null, null, null, null, ModelFormat.BasicDX);
								if (mdl.Children.Count > 0)
								{
									foreach (NJS_OBJECT child in mdl.Children)
									{
										Console.WriteLine("Deleting file {0}", dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
										File.Delete(dir + "\\" + child.Name.Substring(7, child.Name.Length - 7) + model_extension);
									}
								}
								if (mdl.Sibling != null)
								{
									Console.WriteLine("Deleting file {0}", dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
									File.Delete(dir + "\\" + mdl.Sibling.Name.Substring(7, mdl.Sibling.Name.Length - 7) + model_extension);
								}
							}
							break;
						/*case "chunkmodel":
							{
								NJS_OBJECT mdl = new NJS_OBJECT(datafile, address, imageBase, ModelFormat.Chunk, new Dictionary<int, Attach>());
								ModelFile.CreateFile(fileOutputPath + ".sa2mdl", mdl, null, null, null, null, ModelFormat.Chunk);
							}
							break;
						case "gcmodel":
							{
								NJS_OBJECT mdl = new NJS_OBJECT(datafile, address, imageBase, ModelFormat.GC, new Dictionary<int, Attach>());
								ModelFile.CreateFile(fileOutputPath + ".sa2mdl", mdl, null, null, null, null, ModelFormat.GC);
							}
							break;*/
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("{0} at {1} extraction failed: {2}", type, address.ToString("X8"), ex.ToString());
				}
			}
			//Filter out landtable stuff
			if (Path.GetFileName(filename) == "sonic.exe")
			{
				Console.WriteLine("Removing landtable items...");
				for (int i = 0; i < LandtableAddr.Length; i++)
				{
					LandTable land = new LandTable(datafile, LandtableAddr[i], imageBase, LandTableFormat.SADX);
					if (land.COL.Count > 0)
					{
						foreach (COL col in land.COL)
						{
							File.Delete(dir + "\\" + col.Model.Name.Substring(7, col.Model.Name.Length-7) + model_extension);
							Console.WriteLine("Deleting file {0}", dir + "\\" + col.Model.Name.Substring(7, col.Model.Name.Length - 7) + model_extension);
						}
					}
					if (land.Anim.Count > 0)
					{
						foreach (GeoAnimData anim in land.Anim)
						{
							File.Delete(dir + "\\" + anim.Model.Name.Substring(7, anim.Model.Name.Length-7) + model_extension);
							Console.WriteLine("Deleting file {0}", dir + "\\" + anim.Model.Name.Substring(7, anim.Model.Name.Length - 7) + model_extension);
						}
					}
				}
			}
		}
	}
}