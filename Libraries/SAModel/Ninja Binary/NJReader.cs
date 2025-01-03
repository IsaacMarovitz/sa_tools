﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

// Dreamcast Ninja Binary (.nj) format and its platform variations (.gj, .xj)
namespace SAModel
{
	public class NinjaBinaryFile
	{
		enum NinjaBinaryChunkType
		{
			BasicModel,
			ChunkModel,
			Texlist,
			Motion,
			SimpleShapeMotion,
			POF0,
			Unimplemented,
			Invalid
		}

		public List<NJS_OBJECT> Models; // In NJBM or NJCM
		public List<NJS_MOTION> Motions; // In NMDM
		public List<string[]> Texnames; // In NJTL

		private class NinjaDataChunk
		{
			public NinjaBinaryChunkType Type;
			public int ImageBase;
			public byte[] Data;

			public NinjaDataChunk(NinjaBinaryChunkType type, byte[] data)
			{
				Type = type;
				Data = data;
			}
		}

		public enum BigEndianResult
		{
			LittleEndian,
			BigEndian,
			CantTell
		}

		public BigEndianResult CheckPointerBigEndian(byte[] data, int address)
		{
			BigEndianResult result = BigEndianResult.CantTell;
			// Back up Big Endian mode
			bool bk = ByteConverter.BigEndian;
			// Set Big Endian mode
			ByteConverter.BigEndian = true;
			// Get Little Endian version
			uint pnt_little = BitConverter.ToUInt32(data, address);
			// Get Big Endian version
			uint png_big = ByteConverter.ToUInt32(data, address);
			// If Little is bigger, it's likely Big Endian
			if (pnt_little > png_big)
				result = BigEndianResult.BigEndian;
			// If Big is bigger, it's likely Little Endian
			else if (pnt_little < png_big)
				result = BigEndianResult.LittleEndian;
			// Restore Big Endian mode
			ByteConverter.BigEndian = bk;
			return result;
		}

		public NinjaBinaryFile(byte[] data, ModelFormat format)
		{
			Models = new List<NJS_OBJECT>();
			Motions = new List<NJS_MOTION>();
			Texnames = new List<string[]>();
			int startoffset = 0; // Current reading position.
			int modelcount = 0; // This is used to keep track of the model added last to get data for motions.
			int currentchunk = 0; // Keep track of current data chunk in case a POF0 chunk is found.
			int imgBase = 0; // Key added to pointers.
			bool sizeIsLittleEndian = true; // In Gamecube games, size can be either Big or Little Endian.
			List<NinjaDataChunk> chunks = new List<NinjaDataChunk>();
			// Back up Big Endian mode
			bool bigEndianBk = ByteConverter.BigEndian;
			// Read the file until the end
			while (startoffset < data.Length - 8) // 8 is the size of chunk ID + chunk size
			{
				// Skip padding and unrecognized data
				if (IdentifyChunk(data, startoffset) == NinjaBinaryChunkType.Invalid)
				{
					while (IdentifyChunk(data, startoffset) == NinjaBinaryChunkType.Invalid)
					{
						// Stop if reached the end of file
						if (startoffset >= data.Length - 4)
							break;
						startoffset += 1;
					}
				}
				// Stop if reached the end of file
				if (startoffset >= data.Length - 4)
					break;
				// Get Ninja data chunk type
				NinjaBinaryChunkType idtype = IdentifyChunk(data, startoffset);
				// Endianness checks for the first chunk
				if (currentchunk == 0)
				{
					// This check is done because in PSO GC chunk size is in Little Endian despite the rest of the data being Big Endian.
					// First, determine whether size is Big Endian or not.
					ByteConverter.BigEndian = true;
					sizeIsLittleEndian = BitConverter.ToUInt32(data, startoffset + 4) < ByteConverter.ToUInt32(data, startoffset + 4);
					// Then, check if the actual data is Big Endian. Unfortunately this is just guessing so it may not always work.
					// startoffset + 8 is where the data begins
					switch (idtype)
					{
						case NinjaBinaryChunkType.BasicModel:
						case NinjaBinaryChunkType.ChunkModel:
							// Check attach pointer
							BigEndianResult res_endian = CheckPointerBigEndian(data, startoffset + 8 + 4);
							if (res_endian == BigEndianResult.CantTell)
							{
								// Check child pointer
								res_endian = CheckPointerBigEndian(data, startoffset + 8 + 0x2C);
								if (res_endian == BigEndianResult.CantTell)
								{
									// Check sibling pointer
									res_endian = CheckPointerBigEndian(data, startoffset + 8 + 0x30);
								}
							}
							ByteConverter.BigEndian = res_endian == BigEndianResult.BigEndian;
							break;
						case NinjaBinaryChunkType.Motion: // Number of frames
						case NinjaBinaryChunkType.SimpleShapeMotion: // Number of frames
						case NinjaBinaryChunkType.Texlist: // Number of texnames
							ByteConverter.BigEndian = BitConverter.ToUInt32(data, startoffset + 12) > ByteConverter.ToUInt32(data, startoffset + 12);
							break;
						default: // Old check
							ByteConverter.BigEndian = BitConverter.ToUInt32(data, startoffset + 8) > ByteConverter.ToUInt32(data, startoffset + 8);
							break;							
					}
					//MessageBox.Show(ByteConverter.BigEndian.ToString());
				}
				int size = sizeIsLittleEndian ? BitConverter.ToInt32(data, startoffset + 4) : ByteConverter.ToInt32(data, startoffset + 4);
				//MessageBox.Show(idtype.ToString() + " chunk at " + (startoffset + 8).ToString("X8") + " size " + size.ToString());
				// Add the chunk to the list to process
				chunks.Add(new NinjaDataChunk(idtype, new byte[size]));
				Array.Copy(data, startoffset + 8, chunks[currentchunk].Data, 0, chunks[currentchunk].Data.Length);
				// If a POF0 chunk is reached, fix up the previous chunk's pointers
				if (idtype == NinjaBinaryChunkType.POF0)
				{
					List<int> offs = POF0Helper.GetPointerListFromPOF(chunks[currentchunk].Data);
					//MessageBox.Show("POF at " + (startoffset + 8).ToString("X") + " imgBase: " + imgBase.ToString("X") + " size " + chunks[currentchunk].Data.Length.ToString());
					POF0Helper.FixPointersWithPOF(chunks[currentchunk - 1].Data, offs, imgBase);
					chunks[currentchunk - 1].ImageBase = imgBase;
					//System.IO.File.WriteAllBytes("C:\\Users\\PkR\\Desktop\\chunk\\" + currentchunk.ToString("D3") + "_pof.bin", chunks[currentchunk].Data);
					//System.IO.File.WriteAllBytes("C:\\Users\\PkR\\Desktop\\chunk\\" + currentchunk.ToString("D3") + ".bin", chunks[currentchunk - 1].Data);
					startoffset += chunks[currentchunk].Data.Length + 8;
				}
				// Otherwise advance the reading position and pointer image base
				else
				{
					imgBase += startoffset;
					startoffset += chunks[currentchunk].Data.Length + 8;
				}
				currentchunk++;
			}
			// Go over the fixed chunks and add final data
			foreach (NinjaDataChunk chunk in chunks)
			{
				switch (chunk.Type)
				{
					case NinjaBinaryChunkType.BasicModel:
						//MessageBox.Show("Basic model at " + chunk.ImageBase.ToString("X") + " size " + chunk.Data.Length.ToString());
						// Add a label so that all models aren't called "object_00000000"
						Dictionary<int, string> labelb = new Dictionary<int, string>();
						labelb.Add(0, "object_" + chunk.ImageBase.ToString("X8"));
						Models.Add(new NJS_OBJECT(chunk.Data, 0, (uint)chunk.ImageBase, ModelFormat.Basic, labelb, new Dictionary<int, Attach>()));
						modelcount++;
						break;
					case NinjaBinaryChunkType.ChunkModel:
						//MessageBox.Show(format.ToString() + " model at " + chunk.ImageBase.ToString("X") + " size " + chunk.Data.Length.ToString());
						// Add a label so that all models aren't called "object_00000000"
						Dictionary<int, string> labelc = new Dictionary<int, string>();
						labelc.Add(0, "object_" + chunk.ImageBase.ToString("X8"));
						// NJCM can be Chunk (NJ file, Big or Little Endian), Ginja (GJ file) or XJ (XJ file)
						Models.Add(new NJS_OBJECT(chunk.Data, 0, (uint)chunk.ImageBase, format, labelc, new Dictionary<int, Attach>()));
						modelcount++;
						break;
					case NinjaBinaryChunkType.Texlist:
						//MessageBox.Show("Texlist at " + chunk.ImageBase.ToString("X") + " size " + chunk.Data.Length.ToString());
						int firstEntry = ByteConverter.ToInt32(chunk.Data, 0) - chunk.ImageBase; // Prooobably, seems to be 8 always
						int numTextures = ByteConverter.ToInt32(chunk.Data, 0x4);
						List<string> texNames = new List<string>();
						// Add texture names
						for (int i = 0; i < numTextures; i++)
						{
							int textAddress = ByteConverter.ToInt32(chunk.Data, firstEntry + i * 0xC) - chunk.ImageBase; // 0xC is the size of NJS_TEXNAME
							// Read the null terminated string
							List<byte> namestring = new List<byte>();
							byte namechar = (chunk.Data[textAddress]);
							int j = 0;
							while (namechar != 0)
							{
								namestring.Add(namechar);
								j++;
								namechar = (chunk.Data[textAddress + j]);
							}
							texNames.Add(Encoding.ASCII.GetString(namestring.ToArray()));
						}
						Texnames.Add(texNames.ToArray());
						break;
					case NinjaBinaryChunkType.Motion:
						//MessageBox.Show("Motion with ImgBase " + chunk.ImageBase.ToString("X") + " size " + chunk.Data.Length.ToString());
						try
						{
							// Add a label so that all motions aren't called "motion_00000000"
							Dictionary<int, string> labelm = new Dictionary<int, string>();
							labelm.Add(0, "motion_" + chunk.ImageBase.ToString("X8"));
							Motions.Add(new NJS_MOTION(chunk.Data, 0, (uint)chunk.ImageBase, Models.Count > 0 ? Models[modelcount - 1].CountAnimated() : -1, labelm, objectName: Models.Count > 0 ? Models[modelcount - 1].Name : ""));
						}
						catch (Exception ex)
						{
							MessageBox.Show("Error adding motion at 0x" + chunk.ImageBase.ToString("X") + ": " + ex.Message.ToString());
						}
						break;
					case NinjaBinaryChunkType.SimpleShapeMotion:
						//MessageBox.Show("Shape Motion with ImgBase " + chunk.ImageBase.ToString("X") + " size " + chunk.Data.Length.ToString());
						try
						{
							// Add a label so that all motions aren't called "motion_00000000"
							Dictionary<int, string> labels = new Dictionary<int, string>();
							labels.Add(0, "shape_" + chunk.ImageBase.ToString("X8"));
							Motions.Add(new NJS_MOTION(chunk.Data, 0, (uint)chunk.ImageBase, Models.Count > 0 ? Models[modelcount - 1].CountAnimated() : -1, labels, numverts: Models[modelcount].GetVertexCounts()));
						}
						catch (Exception ex)
						{
							MessageBox.Show("Error adding shape motion at 0x" + chunk.ImageBase.ToString("X") + ": " + ex.Message.ToString());
						}
						break;

				}
			}
			//MessageBox.Show("Models: " + Models.Count.ToString() + " Animations: " + Motions.Count.ToString() + " Texlists: " + Texnames.Count.ToString() + ", Texture arrays: " + Textures.Count.ToString());
			// Restore Big Endian mode
			ByteConverter.BigEndian = bigEndianBk;
		}

		private NinjaBinaryChunkType IdentifyChunk(byte[] data, int offset)
		{
			if (offset >= data.Length - 8)
				return NinjaBinaryChunkType.Invalid;
			if (BitConverter.ToUInt32(data, offset + 4) == 0)
				return NinjaBinaryChunkType.Invalid;
			switch (System.Text.Encoding.ASCII.GetString(data, offset, 4))
			{
				// Implemented chunk types
				case "NJBM":
				case "GJBM":
					return NinjaBinaryChunkType.BasicModel;
				case "NJCM":
				case "GJCM":
					return NinjaBinaryChunkType.ChunkModel;
				case "NMDM":
					return NinjaBinaryChunkType.Motion;
				case "NSSM":
					return NinjaBinaryChunkType.SimpleShapeMotion;
				case "NJTL":
				case "GJTL":
					return NinjaBinaryChunkType.Texlist;
				case "POF0":
					return NinjaBinaryChunkType.POF0;
				// Unimplemented types. These have to be accounted for because they are followed by POF0.
				case "NJLI": // Ninja Light
				case "NJCA": // Ninja Camera
				case "NLIM": // Ninja Light Motion
				case "NJIN": // Ninja Metadata
				case "N2CM": // Ninja2 Chunk Model
				case "NJSP": // Ninja Cell Sprite
				case "NJCS": // Ninja Cell Stream
				case "NCSM": // Ninja Cell Sprite Motion
				case "CPSM": // Ninja Compact Shape Motion
				case "NJSL": // Ninja Compact Shape List
				case "CGCL": // Illbleed
				case "CGLC": // Illbleed
				case "CGMP": // Illbleed
				case "CGSP": // Illbleed
				case "CGAL": // Illbleed
				case "CGAM": // Illbleed
				case "NCAM": // Illbleed
				case "CMCK": // Illbleed
					return NinjaBinaryChunkType.Unimplemented;
				// Invalid/unknown chunks. These can be ignored as they aren't followed by POF0.
				case "POF1": // Pointer Offset List (absolute)
				case "POF2": // Pointer Offset List (unknown)
				case "GRND": // Skies of Arcadia
				case "GOBJ": // Skies of Arcadia
				default:
					return NinjaBinaryChunkType.Invalid;
			}
		}
	}
}