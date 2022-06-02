﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SAModel.SAEditorCommon
{
	public static class StructConversion
	{
		public enum TextType
		{
			CStructs,
			NJA,
			JSON
		}

		/// <summary>
		/// Exports a single level, model or animation file as text.
		/// </summary>
		/// <param name="source">Source pathname.</param>
		/// <param name="type">Type of text conversion.</param>
		/// <param name="destination">Destination pathname. Leave blank to export in the same folder with a swapped extension.</param>
		/// <param name="basicDX">Use the SADX2004 format for Basic models.</param>
		public static void ConvertFileToText(string source, TextType type, string destination = "", bool basicDX = true, bool overwrite = true)
		{
			string outext = ".c";
			string extension = Path.GetExtension(source);
			switch (extension.ToLowerInvariant())
			{
				case ".sa2lvl":
				case ".sa1lvl":
					if (type == TextType.CStructs)
					{
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						LandTable land = LandTable.LoadFromFile(source);
						List<string> labels = new List<string>() { land.Name };
						using (StreamWriter sw = File.CreateText(destination))
						{
							sw.Write("/* Sonic Adventure ");
							LandTableFormat fmt = land.Format;
							switch (land.Format)
							{
								case LandTableFormat.SA1:
								case LandTableFormat.SADX:
									if (basicDX)
									{
										sw.Write("DX");
										fmt = LandTableFormat.SADX;
									}
									else
									{
										sw.Write("1");
										fmt = LandTableFormat.SA1;
									}
									break;
								case LandTableFormat.SA2:
									sw.Write("2");
									fmt = LandTableFormat.SA2;
									break;
								case LandTableFormat.SA2B:
									sw.Write("2 Battle");
									fmt = LandTableFormat.SA2B;
									break;
							}
							sw.WriteLine(" LandTable");
							sw.WriteLine(" * ");
							sw.WriteLine(" * Generated by DataToolbox");
							sw.WriteLine(" * ");
							if (!string.IsNullOrEmpty(land.Description))
							{
								sw.Write(" * Description: ");
								sw.WriteLine(land.Description);
								sw.WriteLine(" * ");
							}
							if (!string.IsNullOrEmpty(land.Author))
							{
								sw.Write(" * Author: ");
								sw.WriteLine(land.Author);
								sw.WriteLine(" * ");
							}
							sw.WriteLine(" */");
							sw.WriteLine();
							land.ToStructVariables(sw, fmt, labels, null);
						}
					}
					break;
				case ".sa1mdl":
				case ".sa2mdl":
					ModelFile modelFile = new ModelFile(source);
					NJS_OBJECT model = modelFile.Model;
					List<NJS_MOTION> animations = new List<NJS_MOTION>(modelFile.Animations);
					if (type == TextType.CStructs)
					{
						outext = ".c";
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						using (StreamWriter sw = File.CreateText(destination))
						{
							sw.Write("/* NINJA ");
							switch (modelFile.Format)
							{
								case ModelFormat.Basic:
								case ModelFormat.BasicDX:
									if (basicDX)
									{
										sw.Write("Basic (with Sonic Adventure DX additions)");
									}
									else
									{
										sw.Write("Basic");
									}
									break;
								case ModelFormat.Chunk:
									sw.Write("Chunk");
									break;
								case ModelFormat.GC:
									sw.Write("GC");
									break;
							}
							sw.WriteLine(" model");
							sw.WriteLine(" * ");
							sw.WriteLine(" * Generated by DataToolbox");
							sw.WriteLine(" * ");
							if (modelFile != null)
							{
								if (!string.IsNullOrEmpty(modelFile.Description))
								{
									sw.Write(" * Description: ");
									sw.WriteLine(modelFile.Description);
									sw.WriteLine(" * ");
								}
								if (!string.IsNullOrEmpty(modelFile.Author))
								{
									sw.Write(" * Author: ");
									sw.WriteLine(modelFile.Author);
									sw.WriteLine(" * ");
								}
							}
							sw.WriteLine(" */");
							sw.WriteLine();
							List<string> labels_m = new List<string>() { model.Name };
							model.ToStructVariables(sw, basicDX, labels_m, null);
							foreach (NJS_MOTION anim in animations)
							{
								anim.ToStructVariables(sw);
							}
						}
					}
					else if (type == TextType.NJA)
					{
						outext = ".nja";
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						using (StreamWriter sw2 = File.CreateText(destination))
						{
							List<string> labels_nj = new List<string>() { model.Name };
							model.ToNJA(sw2, basicDX, labels_nj, null);
						}
					}
					break;
				case ".saanim":
					NJS_MOTION animation = NJS_MOTION.Load(source);
					if (type == TextType.CStructs)
					{
						outext = ".c";
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						using (StreamWriter sw = File.CreateText(destination))
						{
							sw.WriteLine("/* NINJA Motion");
							sw.WriteLine(" * ");
							sw.WriteLine(" * Generated by DataToolbox");
							sw.WriteLine(" * ");
							sw.WriteLine(" */");
							sw.WriteLine();
							animation.ToStructVariables(sw);

						}
					}
					else if (type == TextType.JSON)
					{
						outext = ".json";
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						JsonSerializer js = new JsonSerializer() { Culture = System.Globalization.CultureInfo.InvariantCulture };
						using (TextWriter tw = File.CreateText(destination))
						using (JsonTextWriter jtw = new JsonTextWriter(tw) { Formatting = Formatting.Indented })
							js.Serialize(jtw, animation);
					}
					else if (type == TextType.NJA)
					{
						outext = animation.IsShapeMotion() ? ".nas" : ".nam";
						if (destination == "")
						{
							destination = Path.Combine(Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source) + outext);
						}
						if (!overwrite && File.Exists(destination))
						{
							while (File.Exists(destination))
							{
								destination = destination = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination) + "_" + outext);
							}
						}
						using (StreamWriter sw2 = File.CreateText(destination))
						{
							animation.ToNJA(sw2);
						}
					}
					break;
			}
		}
	}
}