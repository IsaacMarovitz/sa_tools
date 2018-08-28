﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D9;
using SonicRetro.SAModel.Direct3D;
using SonicRetro.SAModel.Direct3D.TextureSystem;
using SonicRetro.SAModel.SAEditorCommon;
using SonicRetro.SAModel.SAEditorCommon.UI;
using Color = System.Drawing.Color;
using Mesh = SonicRetro.SAModel.Direct3D.Mesh;
using Point = System.Drawing.Point;

namespace SonicRetro.SAModel.SAMDL
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			Application.ThreadException += Application_ThreadException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			File.WriteAllText("SAMDL.log", e.Exception.ToString());
			if (MessageBox.Show("Unhandled " + e.Exception.GetType().Name + "\nLog file has been saved.\n\nDo you want to try to continue running?", "SAMDL Fatal Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
				Close();
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			File.WriteAllText("SAMDL.log", e.ExceptionObject.ToString());
			MessageBox.Show("Unhandled Exception: " + e.ExceptionObject.GetType().Name + "\nLog file has been saved.", "SAMDL Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		internal Device d3ddevice;
		EditorCamera cam = new EditorCamera(EditorOptions.RenderDrawDistance);
		bool loaded;
		int interval = 1;
		NJS_OBJECT model;
		Animation[] animations;
		Animation animation;
		ModelFile modelFile;
		ModelFormat outfmt;
		int animnum = -1;
		int animframe = 0;
		Mesh[] meshes;
		string TexturePackName;
		BMPInfo[] TextureInfo;
		Texture[] Textures;
		ModelFileDialog modelinfo = new ModelFileDialog();
		NJS_OBJECT selectedObject;
		Dictionary<NJS_OBJECT, TreeNode> nodeDict;
		Mesh sphereMesh;

		private void MainForm_Load(object sender, EventArgs e)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
			SharpDX.Direct3D9.Direct3D d3d = new SharpDX.Direct3D9.Direct3D();
			d3ddevice = new Device(d3d, 0, DeviceType.Hardware, panel1.Handle, CreateFlags.HardwareVertexProcessing,
				new PresentParameters
				{
					Windowed = true,
					SwapEffect = SwapEffect.Discard,
					EnableAutoDepthStencil = true,
					AutoDepthStencilFormat = Format.D24X8
				});

			EditorOptions.Initialize(d3ddevice);
			sphereMesh = Mesh.Sphere(d3ddevice, 0.0625f, 10, 10);
			if (Program.Arguments.Length > 0)
				LoadFile(Program.Arguments[0]);
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (loaded)
				switch (MessageBox.Show(this, "Do you want to save?", "SAMDL", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						saveToolStripMenuItem_Click(this, EventArgs.Empty);
						break;
					case DialogResult.Cancel:
						return;
				}
			using (OpenFileDialog a = new OpenFileDialog()
			{
				DefaultExt = "sa1mdl",
				Filter = "Model Files|*.sa1mdl;*.sa2mdl;*.exe;*.dll;*.bin;*.prs;*.rel|All Files|*.*"
			})
				if (a.ShowDialog(this) == DialogResult.OK)
					LoadFile(a.FileName);
		}

		private void LoadFile(string filename)
		{
			loaded = false;
			Environment.CurrentDirectory = Path.GetDirectoryName(filename);
			timer1.Stop();
			modelFile = null;
			animation = null;
			animations = null;
			animnum = -1;
			animframe = 0;
			if (ModelFile.CheckModelFile(filename))
			{
				modelFile = new ModelFile(filename);
				outfmt = modelFile.Format;
				model = modelFile.Model;
				animations = new Animation[modelFile.Animations.Count];
				modelFile.Animations.CopyTo(animations, 0);
			}
			else
			{
				byte[] file = File.ReadAllBytes(filename);
				if (Path.GetExtension(filename).Equals(".prs", StringComparison.OrdinalIgnoreCase))
					file = FraGag.Compression.Prs.Decompress(file);
				SA_Tools.ByteConverter.BigEndian = false;
				uint? baseaddr = SA_Tools.HelperFunctions.SetupEXE(ref file);
				if (baseaddr.HasValue)
				{
					modelinfo.numericUpDown2.Value = baseaddr.Value;
					modelinfo.numericUpDown2.Enabled = false;
					modelinfo.ComboBox1.Enabled = false;
					modelinfo.checkBox2.Checked = modelinfo.checkBox2.Enabled = false;
					LoadBinFile(file);
				}
				else if (Path.GetExtension(filename).Equals(".rel", StringComparison.OrdinalIgnoreCase))
				{
					SA_Tools.ByteConverter.BigEndian = true;
					SA_Tools.HelperFunctions.FixRELPointers(file);
					modelinfo.numericUpDown2.Value = 0;
					modelinfo.numericUpDown2.Enabled = false;
					modelinfo.ComboBox1.Enabled = false;
					modelinfo.checkBox2.Enabled = false;
					modelinfo.checkBox2.Checked = true;
					LoadBinFile(file);
				}
				else
					using (FileTypeDialog ftd = new FileTypeDialog())
					{
						if (ftd.ShowDialog(this) != DialogResult.OK)
							return;
						if (ftd.typBinary.Checked)
						{
							modelinfo.numericUpDown2.Enabled = true;
							modelinfo.ComboBox1.Enabled = true;
							modelinfo.checkBox2.Enabled = true;
							LoadBinFile(file);
						}
						else if (ftd.typSA2MDL.Checked | ftd.typSA2BMDL.Checked)
						{
							ModelFormat fmt = outfmt = ModelFormat.Chunk;
							ByteConverter.BigEndian = ftd.typSA2BMDL.Checked;
							using (SA2MDLDialog dlg = new SA2MDLDialog())
							{
								int address = 0;
								SortedDictionary<int, NJS_OBJECT> sa2models = new SortedDictionary<int, NJS_OBJECT>();
								int i = ByteConverter.ToInt32(file, address);
								while (i != -1)
								{
									sa2models.Add(i, new NJS_OBJECT(file, ByteConverter.ToInt32(file, address + 4), 0, fmt));
									address += 8;
									i = ByteConverter.ToInt32(file, address);
								}
								foreach (KeyValuePair<int, NJS_OBJECT> item in sa2models)
									dlg.modelChoice.Items.Add(item.Key + ": " + item.Value.Name);
								dlg.ShowDialog(this);
								i = 0;
								foreach (KeyValuePair<int, NJS_OBJECT> item in sa2models)
								{
									if (i == dlg.modelChoice.SelectedIndex)
									{
										model = item.Value;
										break;
									}
									i++;
								}
								if (dlg.checkBox1.Checked)
								{
									using (OpenFileDialog anidlg = new OpenFileDialog()
									{
										DefaultExt = "bin",
										Filter = "Motion Files|*MTN.BIN;*MTN.PRS|All Files|*.*"
									})
									{
										if (anidlg.ShowDialog(this) == DialogResult.OK)
										{
											byte[] anifile = File.ReadAllBytes(anidlg.FileName);
											if (Path.GetExtension(anidlg.FileName).Equals(".prs", StringComparison.OrdinalIgnoreCase))
												anifile = FraGag.Compression.Prs.Decompress(anifile);
											address = 0;
											SortedDictionary<int, Animation> anis = new SortedDictionary<int, Animation>();
											i = ByteConverter.ToInt32(file, address);
											while (i != -1)
											{
												anis.Add(i, new Animation(file, ByteConverter.ToInt32(file, address + 4), 0, model.CountAnimated()));
												address += 8;
												i = ByteConverter.ToInt32(file, address);
											}
											animations = new List<Animation>(anis.Values).ToArray();
										}
									}
								}
							}
						}
					}
			}
			if (model.HasWeight)
				meshes = model.ProcessWeightedModel(d3ddevice).ToArray();
			else
			{
				model.ProcessVertexData();
				NJS_OBJECT[] models = model.GetObjects();
				meshes = new Mesh[models.Length];
				for (int i = 0; i < models.Length; i++)
					if (models[i].Attach != null)
						try { meshes[i] = models[i].Attach.CreateD3DMesh(d3ddevice); }
						catch { }
			}
			treeView1.Nodes.Clear();
			nodeDict = new Dictionary<NJS_OBJECT, TreeNode>();
			AddTreeNode(model, treeView1.Nodes);
			loaded = saveToolStripMenuItem.Enabled = exportToolStripMenuItem.Enabled = findToolStripMenuItem.Enabled = true;
			selectedObject = model;
			SelectedItemChanged();
		}

		private void LoadBinFile(byte[] file)
		{
			modelinfo.ShowDialog(this);
			ByteConverter.BigEndian = modelinfo.checkBox2.Checked;
			if (modelinfo.checkBox1.Checked)
				animations = new Animation[] { Animation.ReadHeader(file, (int)modelinfo.numericUpDown3.Value, (uint)modelinfo.numericUpDown2.Value, (ModelFormat)modelinfo.comboBox2.SelectedIndex) };
			model = new NJS_OBJECT(file, (int)modelinfo.NumericUpDown1.Value, (uint)modelinfo.numericUpDown2.Value, (ModelFormat)modelinfo.comboBox2.SelectedIndex);
			switch ((ModelFormat)modelinfo.comboBox2.SelectedIndex)
			{
				case ModelFormat.Basic:
				case ModelFormat.BasicDX:
					outfmt = ModelFormat.Basic;
					break;
				case ModelFormat.Chunk:
					outfmt = ModelFormat.Chunk;
					break;
			}
		}

		private void AddTreeNode(NJS_OBJECT model, TreeNodeCollection nodes)
		{
			int index = 0;
			AddTreeNode(model, ref index, nodes);
		}

		private void AddTreeNode(NJS_OBJECT model, ref int index, TreeNodeCollection nodes)
		{
			TreeNode node = nodes.Add($"{index++}: {model.Name}");
			node.Tag = model;
			nodeDict[model] = node;
			foreach (NJS_OBJECT child in model.Children)
				AddTreeNode(child, ref index, node.Nodes);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (loaded)
				switch (MessageBox.Show(this, "Do you want to save?", "SAMDL", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						saveToolStripMenuItem_Click(this, EventArgs.Empty);
						break;
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog()
			{
				DefaultExt = (outfmt == ModelFormat.Chunk ? "sa2" : "sa1") + "mdl",
				Filter = (outfmt == ModelFormat.Chunk ? "SA2" : "SA1") + "MDL Files|*." + (outfmt == ModelFormat.Chunk ? "sa2" : "sa1") + "mdl|All Files|*.*"
			})
				if (a.ShowDialog(this) == DialogResult.OK)
					if (modelFile != null)
					{
						modelFile.Tool = "SAMDL";
						modelFile.SaveToFile(a.FileName);
					}
					else
						ModelFile.CreateFile(a.FileName, model, null, null, null, null, "SAMDL", null, outfmt);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		internal void DrawLevel()
		{
			if (!loaded) return;
			d3ddevice.SetTransform(TransformState.Projection, Matrix.PerspectiveFovRH((float)(Math.PI / 4), panel1.Width / (float)panel1.Height, 1, cam.DrawDistance));
			d3ddevice.SetTransform(TransformState.View, cam.ToMatrix());
			Text = "X=" + cam.Position.X + " Y=" + cam.Position.Y + " Z=" + cam.Position.Z + " Pitch=" + cam.Pitch.ToString("X") + " Yaw=" + cam.Yaw.ToString("X") + " Interval=" + interval + (cam.mode == 1 ? " Distance=" + cam.Distance : "") + (animation != null ? " Animation=" + animation.Name + " Frame=" + animframe : "");
			d3ddevice.SetRenderState(RenderState.FillMode, EditorOptions.RenderFillMode);
			d3ddevice.SetRenderState(RenderState.CullMode, EditorOptions.RenderCullMode);
			d3ddevice.Material = new Material { Ambient = Color.White.ToRawColor4() };
			d3ddevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black.ToRawColorBGRA(), 1, 0);
			d3ddevice.SetRenderState(RenderState.ZEnable, true);
			d3ddevice.BeginScene();
			//all drawings after this line
			EditorOptions.RenderStateCommonSetup(d3ddevice);

			MatrixStack transform = new MatrixStack();
			if (showModelToolStripMenuItem.Checked)
			{
				if (model.HasWeight)
					RenderInfo.Draw(model.DrawModelTreeWeighted(d3ddevice, transform.Top, Textures, meshes), d3ddevice, cam);
				else if (animation != null)
					RenderInfo.Draw(model.DrawModelTreeAnimated(d3ddevice, transform, Textures, meshes, animation, animframe), d3ddevice, cam);
				else
					RenderInfo.Draw(model.DrawModelTree(d3ddevice, transform, Textures, meshes), d3ddevice, cam);

				if (selectedObject != null)
				{
					if (model.HasWeight)
					{
						NJS_OBJECT[] objs = model.GetObjects();
						if (selectedObject.Attach != null)
							for (int j = 0; j < selectedObject.Attach.MeshInfo.Length; j++)
							{
								Color col = selectedObject.Attach.MeshInfo[j].Material == null ? Color.White : selectedObject.Attach.MeshInfo[j].Material.DiffuseColor;
								col = Color.FromArgb(255 - col.R, 255 - col.G, 255 - col.B);
								NJS_MATERIAL mat = new NJS_MATERIAL
								{
									DiffuseColor = col,
									IgnoreLighting = true,
									UseAlpha = false
								};
								new RenderInfo(meshes[Array.IndexOf(objs, selectedObject)], j, transform.Top, mat, null, FillMode.Wireframe, selectedObject.Attach.CalculateBounds(j, transform.Top)).Draw(d3ddevice);
							}
					}
					else
						DrawSelectedObject(model, transform);
				}
			}

			d3ddevice.SetRenderState(RenderState.AlphaBlendEnable, false);
			d3ddevice.SetRenderState(RenderState.FillMode, FillMode.Solid);
			d3ddevice.SetRenderState(RenderState.Lighting, false);
			d3ddevice.SetRenderState(RenderState.ZEnable, false);
			d3ddevice.SetTexture(0, null);
			if (showNodesToolStripMenuItem.Checked)
				DrawNodes(model, transform);

			if (showNodeConnectionsToolStripMenuItem.Checked)
				DrawNodeConnections(model, transform);

			d3ddevice.EndScene(); //all drawings before this line
			d3ddevice.Present();
		}

		private void DrawSelectedObject(NJS_OBJECT obj, MatrixStack transform)
		{
			int modelnum = -1;
			int animindex = -1;
			DrawSelectedObject(obj, transform, ref modelnum, ref animindex);
		}

		private bool DrawSelectedObject(NJS_OBJECT obj, MatrixStack transform, ref int modelindex, ref int animindex)
		{
			transform.Push();
			modelindex++;
			if (obj.Animate) animindex++;
			if (animation != null && animation.Models.ContainsKey(animindex))
				obj.ProcessTransforms(animation.Models[animindex], animframe, transform);
			else
				obj.ProcessTransforms(transform);
			if (obj == selectedObject)
			{
				if (obj.Attach != null)
					for (int j = 0; j < obj.Attach.MeshInfo.Length; j++)
					{
						Color col = obj.Attach.MeshInfo[j].Material == null ? Color.White : obj.Attach.MeshInfo[j].Material.DiffuseColor;
						col = Color.FromArgb(255 - col.R, 255 - col.G, 255 - col.B);
						NJS_MATERIAL mat = new NJS_MATERIAL
						{
							DiffuseColor = col,
							IgnoreLighting = true,
							UseAlpha = false
						};
						new RenderInfo(meshes[modelindex], j, transform.Top, mat, null, FillMode.Wireframe, obj.Attach.CalculateBounds(j, transform.Top)).Draw(d3ddevice);
					}
				transform.Pop();
				return true;
			}
			foreach (NJS_OBJECT child in obj.Children)
				if (DrawSelectedObject(child, transform, ref modelindex, ref animindex))
				{
					transform.Pop();
					return true;
				}
			transform.Pop();
			return false;
		}

		private void DrawNodes(NJS_OBJECT obj, MatrixStack transform)
		{
			int modelnum = -1;
			int animindex = -1;
			DrawNodes(obj, transform, ref modelnum, ref animindex);
		}

		private void DrawNodes(NJS_OBJECT obj, MatrixStack transform, ref int modelindex, ref int animindex)
		{
			transform.Push();
			modelindex++;
			if (obj.Animate) animindex++;
			if (animation != null && animation.Models.ContainsKey(animindex))
				obj.ProcessTransforms(animation.Models[animindex], animframe, transform);
			else
				obj.ProcessTransforms(transform);
			d3ddevice.SetTransform(TransformState.World, Matrix.Translation(Vector3.TransformCoordinate(new Vector3(), transform.Top)));
			sphereMesh.DrawSubset(0);
			foreach (NJS_OBJECT child in obj.Children)
				DrawNodes(child, transform, ref modelindex, ref animindex);
			transform.Pop();
		}

		private void DrawNodeConnections(NJS_OBJECT obj, MatrixStack transform)
		{
			int modelnum = -1;
			int animindex = -1;
			List<Vector3> points = new List<Vector3>();
			List<short> indexes = new List<short>();
			DrawNodeConnections(obj, transform, points, indexes, -1, ref modelnum, ref animindex);
			d3ddevice.SetTransform(TransformState.World, Matrix.Identity);
			d3ddevice.VertexFormat = VertexFormat.Position;
			d3ddevice.DrawIndexedUserPrimitives(PrimitiveType.LineList, 0, points.Count, indexes.Count / 2, indexes.ToArray(), Format.Index16, points.ToArray());
		}

		private void DrawNodeConnections(NJS_OBJECT obj, MatrixStack transform, List<Vector3> points, List<short> indexes, short parentidx, ref int modelindex, ref int animindex)
		{
			transform.Push();
			modelindex++;
			if (obj.Animate) animindex++;
			if (animation != null && animation.Models.ContainsKey(animindex))
				obj.ProcessTransforms(animation.Models[animindex], animframe, transform);
			else
				obj.ProcessTransforms(transform);
			short newidx = (short)points.Count;
			points.Add(Vector3.TransformCoordinate(new Vector3(), transform.Top));
			if (parentidx != -1)
			{
				indexes.Add(parentidx);
				indexes.Add(newidx);
			}
			foreach (NJS_OBJECT child in obj.Children)
				DrawNodeConnections(child, transform, points, indexes, newidx, ref modelindex, ref animindex);
			transform.Pop();
		}

		private void UpdateWeightedModel()
		{
			if (model.HasWeight)
			{
				if (animation != null)
					meshes = model.ProcessWeightedModelAnimated(d3ddevice, animation, animframe).ToArray();
				else
					meshes = model.ProcessWeightedModel(d3ddevice).ToArray();
			}
		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{
			DrawLevel();
		}

		private void panel1_KeyDown(object sender, KeyEventArgs e)
		{
			if (!loaded) return;
			if (cam.mode == 0)
			{
				if (e.KeyCode == Keys.Down)
					if (e.Shift)
						cam.Position += cam.Up * -interval;
					else
						cam.Position += cam.Look * interval;
				if (e.KeyCode == Keys.Up)
					if (e.Shift)
						cam.Position += cam.Up * interval;
					else
						cam.Position += cam.Look * -interval;
				if (e.KeyCode == Keys.Left)
					cam.Position += cam.Right * -interval;
				if (e.KeyCode == Keys.Right)
					cam.Position += cam.Right * interval;
				if (e.KeyCode == Keys.K)
					cam.Yaw = unchecked((ushort)(cam.Yaw - 0x100));
				if (e.KeyCode == Keys.J)
					cam.Yaw = unchecked((ushort)(cam.Yaw + 0x100));
				if (e.KeyCode == Keys.H)
					cam.Yaw = unchecked((ushort)(cam.Yaw + 0x4000));
				if (e.KeyCode == Keys.L)
					cam.Yaw = unchecked((ushort)(cam.Yaw - 0x4000));
				if (e.KeyCode == Keys.M)
					cam.Pitch = unchecked((ushort)(cam.Pitch - 0x100));
				if (e.KeyCode == Keys.I)
					cam.Pitch = unchecked((ushort)(cam.Pitch + 0x100));
				if (e.KeyCode == Keys.E)
					cam.Position = new Vector3();
				if (e.KeyCode == Keys.R)
				{
					cam.Pitch = 0;
					cam.Yaw = 0;
				}
			}
			else
			{
				if (e.KeyCode == Keys.Down)
					if (e.Shift)
						cam.Pitch = unchecked((ushort)(cam.Pitch - 0x100));
					else
						cam.Distance += interval;
				if (e.KeyCode == Keys.Up)
					if (e.Shift)
						cam.Pitch = unchecked((ushort)(cam.Pitch + 0x100));
					else
					{
						cam.Distance -= interval;
						cam.Distance = Math.Max(cam.Distance, interval);
					}
				if (e.KeyCode == Keys.Left)
					cam.Yaw = unchecked((ushort)(cam.Yaw + 0x100));
				if (e.KeyCode == Keys.Right)
					cam.Yaw = unchecked((ushort)(cam.Yaw - 0x100));
				if (e.KeyCode == Keys.K)
					cam.Yaw = unchecked((ushort)(cam.Yaw - 0x100));
				if (e.KeyCode == Keys.J)
					cam.Yaw = unchecked((ushort)(cam.Yaw + 0x100));
				if (e.KeyCode == Keys.H)
					cam.Yaw = unchecked((ushort)(cam.Yaw + 0x4000));
				if (e.KeyCode == Keys.L)
					cam.Yaw = unchecked((ushort)(cam.Yaw - 0x4000));
				if (e.KeyCode == Keys.M)
					cam.Pitch = unchecked((ushort)(cam.Pitch - 0x100));
				if (e.KeyCode == Keys.I)
					cam.Pitch = unchecked((ushort)(cam.Pitch + 0x100));
				if (e.KeyCode == Keys.R)
				{
					cam.Pitch = 0;
					cam.Yaw = 0;
				}
			}
			if (e.KeyCode == Keys.X)
				cam.mode = (cam.mode + 1) % 2;
			if (e.KeyCode == Keys.Q)
				interval += 1;
			if (e.KeyCode == Keys.W)
				interval -= 1;
			if (e.KeyCode == Keys.OemQuotes & animations != null)
			{
				animnum++;
				animframe = 0;
				if (animnum == animations.Length) animnum = -1;
				if (animnum > -1)
					animation = animations[animnum];
				else
					animation = null;
				UpdateWeightedModel();
			}
			if (e.KeyCode == Keys.OemSemicolon & animations != null)
			{
				animnum--;
				animframe = 0;
				if (animnum == -2) animnum = animations.Length - 1;
				if (animnum > -1)
					animation = animations[animnum];
				else
					animation = null;
				UpdateWeightedModel();
			}
			if (e.KeyCode == Keys.OemOpenBrackets & animation != null)
			{
				animframe--;
				if (animframe < 0) animframe = animation.Frames - 1;
				UpdateWeightedModel();
			}
			if (e.KeyCode == Keys.OemCloseBrackets & animation != null)
			{
				animframe++;
				if (animframe == animation.Frames) animframe = 0;
				UpdateWeightedModel();
			}
			if (e.KeyCode == Keys.P & animation != null)
				timer1.Enabled = !timer1.Enabled;
			if (e.KeyCode == Keys.N)
				if (EditorOptions.RenderFillMode == FillMode.Solid)
					EditorOptions.RenderFillMode = FillMode.Point;
				else
					EditorOptions.RenderFillMode++;
			DrawLevel();
		}

		private void panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
					e.IsInputKey = true;
					break;
			}
		}

		Point lastmouse;
		private void Panel1_MouseMove(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			Point evloc = e.Location;
			if (lastmouse == Point.Empty)
			{
				lastmouse = evloc;
				return;
			}
			Point chg = evloc - (Size)lastmouse;
			if (e.Button == MouseButtons.Middle)
			{
				cam.Yaw = unchecked((ushort)(cam.Yaw - chg.X * 0x10));
				cam.Pitch = unchecked((ushort)(cam.Pitch - chg.Y * 0x10));
				DrawLevel();
			}
			lastmouse = evloc;
		}

		private void loadTexturesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog a = new OpenFileDialog() { DefaultExt = "pvm", Filter = "Texture Files|*.pvm;*.gvm;*.prs" })
			{
				if (a.ShowDialog() == DialogResult.OK)
				{
					TextureInfo = TextureArchive.GetTextures(a.FileName);

					TexturePackName = Path.GetFileNameWithoutExtension(a.FileName);
					Textures = new Texture[TextureInfo.Length];
					for (int j = 0; j < TextureInfo.Length; j++)
						Textures[j] = TextureInfo[j].Image.ToTexture(d3ddevice);

					DrawLevel();
				}
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (animation == null) return;
			animframe++;
			if (animframe == animation.Frames) animframe = 0;
			UpdateWeightedModel();
			DrawLevel();
		}

		private void colladaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog sd = new SaveFileDialog() { DefaultExt = "dae", Filter = "DAE Files|*.dae" })
				if (sd.ShowDialog(this) == DialogResult.OK)
				{
					model.ToCollada(TextureInfo?.Select((item) => item.Name).ToArray()).Save(sd.FileName);
					string p = Path.GetDirectoryName(sd.FileName);
					if (TextureInfo != null)
						foreach (BMPInfo img in TextureInfo)
							img.Image.Save(Path.Combine(p, img.Name + ".png"));
				}
		}

		private void cStructsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog sd = new SaveFileDialog() { DefaultExt = "c", Filter = "C Files|*.c" })
				if (sd.ShowDialog(this) == DialogResult.OK)
				{
					bool dx = false;
					if (outfmt == ModelFormat.Basic)
						dx = MessageBox.Show(this, "Do you want to export in SADX format?", "SAMDL", MessageBoxButtons.YesNo) == DialogResult.Yes;
					List<string> labels = new List<string>() { model.Name };
					using (StreamWriter sw = File.CreateText(sd.FileName))
					{
						sw.Write("/* NINJA ");
						switch (outfmt)
						{
							case ModelFormat.Basic:
							case ModelFormat.BasicDX:
								if (dx)
									sw.Write("Basic (with Sonic Adventure DX additions)");
								else
									sw.Write("Basic");
								break;
							case ModelFormat.Chunk:
								sw.Write("Chunk");
								break;
						}
						sw.WriteLine(" model");
						sw.WriteLine(" * ");
						sw.WriteLine(" * Generated by SAMDL");
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
						string[] texnames = null;
						if (TexturePackName != null)
						{
							texnames = new string[TextureInfo.Length];
							for (int i = 0; i < TextureInfo.Length; i++)
								texnames[i] = string.Format("{0}TexName_{1}", TexturePackName, TextureInfo[i].Name);
							sw.Write("enum {0}TexName", TexturePackName);
							sw.WriteLine();
							sw.WriteLine("{");
							sw.WriteLine("\t" + string.Join("," + Environment.NewLine + "\t", texnames));
							sw.WriteLine("};");
							sw.WriteLine();
						}
						model.ToStructVariables(sw, dx, labels, texnames);
					}
				}
		}

		private void panel1_MouseDown(object sender, MouseEventArgs e)
		{
			if (!loaded) return;
			HitResult dist;
			Vector3 mousepos = new Vector3(e.X, e.Y, 0);
			Viewport viewport = d3ddevice.Viewport;
			Matrix proj = d3ddevice.GetTransform(TransformState.Projection);
			Matrix view = d3ddevice.GetTransform(TransformState.View);
			Vector3 Near, Far;
			Near = mousepos;
			Near.Z = 0;
			Far = Near;
			Far.Z = -1;
			dist = model.CheckHit(Near, Far, viewport, proj, view, new MatrixStack(), meshes);
			if (dist.IsHit)
			{
				selectedObject = dist.Model;
				SelectedItemChanged();
			}
			if (e.Button == MouseButtons.Right)
				contextMenuStrip1.Show(panel1, e.Location);
		}

		internal Type GetAttachType()
		{
			return outfmt == ModelFormat.Chunk ? typeof(ChunkAttach) : typeof(BasicAttach);
		}

		bool suppressTreeEvent;
		internal void SelectedItemChanged()
		{
			suppressTreeEvent = true;
			treeView1.SelectedNode = nodeDict[selectedObject];
			suppressTreeEvent = false;
			propertyGrid1.SelectedObject = selectedObject;
			copyModelToolStripMenuItem.Enabled = selectedObject.Attach != null;
			pasteModelToolStripMenuItem.Enabled = Clipboard.ContainsData(GetAttachType().AssemblyQualifiedName);
			editMaterialsToolStripMenuItem.Enabled = selectedObject.Attach is BasicAttach && TextureInfo != null;
			importOBJToolStripMenuItem.Enabled = outfmt == ModelFormat.Basic;
			exportOBJToolStripMenuItem.Enabled = selectedObject.Attach != null;
			DrawLevel();
		}

		private void objToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog
			{
				DefaultExt = "obj",
				Filter = "OBJ Files|*.obj"
			})
				if (a.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter objstream = new StreamWriter(a.FileName, false))
					using (StreamWriter mtlstream = new StreamWriter(Path.ChangeExtension(a.FileName, "mtl"), false))
					{
						#region Material Exporting
						string materialPrefix = model.Name;

						objstream.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(a.FileName) + ".mtl");

						// This is admittedly not an accurate representation of the materials used in the model - HOWEVER, it makes the materials more managable in MAX
						// So we're doing it this way. In the future we should come back and add an option to do it this way or the original way.
						for (int texIndx = 0; texIndx < TextureInfo.Length; texIndx++)
						{
							mtlstream.WriteLine(String.Format("newmtl {0}_material_{1}", materialPrefix, texIndx));
							mtlstream.WriteLine("Ka 1 1 1");
							mtlstream.WriteLine("Kd 1 1 1");
							mtlstream.WriteLine("Ks 0 0 0");
							mtlstream.WriteLine("illum 1");

							if (!string.IsNullOrEmpty(TextureInfo[texIndx].Name))
							{
								mtlstream.WriteLine("Map_Kd " + TextureInfo[texIndx].Name + ".png");

								// save texture
								string mypath = Path.GetDirectoryName(a.FileName);
								TextureInfo[texIndx].Image.Save(Path.Combine(mypath, TextureInfo[texIndx].Name + ".png"));
							}
						}
						#endregion

						int totalVerts = 0;
						int totalNorms = 0;
						int totalUVs = 0;

						bool errorFlag = false;

						Direct3D.Extensions.WriteModelAsObj(objstream, model, materialPrefix, new MatrixStack(), ref totalVerts, ref totalNorms, ref totalUVs, ref errorFlag);

						if (errorFlag) MessageBox.Show("Error(s) encountered during export. Inspect the output file for more details.");
					}
				}
		}

		private void multiObjToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dlg = new FolderBrowserDialog()
			{
				SelectedPath = Environment.CurrentDirectory,
				ShowNewFolderButton = true
			})
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					
					using (StreamWriter mtlstream = new StreamWriter(Path.Combine(dlg.SelectedPath, TexturePackName + ".mtl"), false))
					{
						// This is admittedly not an accurate representation of the materials used in the model - HOWEVER, it makes the materials more managable in MAX
						// So we're doing it this way. In the future we should come back and add an option to do it this way or the original way.
						for (int texIndx = 0; texIndx < TextureInfo.Length; texIndx++)
						{
							mtlstream.WriteLine(String.Format("newmtl {0}_material_{1}", TexturePackName, texIndx));
							mtlstream.WriteLine("Ka 1 1 1");
							mtlstream.WriteLine("Kd 1 1 1");
							mtlstream.WriteLine("Ks 0 0 0");
							mtlstream.WriteLine("illum 1");

							if (!string.IsNullOrEmpty(TextureInfo[texIndx].Name))
							{
								mtlstream.WriteLine("Map_Kd " + TextureInfo[texIndx].Name + ".png");

								// save texture
								TextureInfo[texIndx].Image.Save(Path.Combine(dlg.SelectedPath, TextureInfo[texIndx].Name + ".png"));
							}
						}
					}
					foreach (NJS_OBJECT obj in model.GetObjects().Where(a => a.Attach != null))
						using (StreamWriter objstream = new StreamWriter(Path.Combine(dlg.SelectedPath, obj.Name + ".obj"), false))
						{
							objstream.WriteLine("mtllib " + TexturePackName + ".mtl");
							bool errorFlag = false;

							Direct3D.Extensions.WriteSingleModelAsObj(objstream, obj, TexturePackName, ref errorFlag);

							if (errorFlag) MessageBox.Show("Error(s) encountered during export. Inspect the output file for more details.");
						}
				}
		}

		private void copyModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clipboard.SetData(GetAttachType().AssemblyQualifiedName, selectedObject.Attach);
			pasteModelToolStripMenuItem.Enabled = true;
		}

		private void pasteModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Attach attach = (Attach)Clipboard.GetData(GetAttachType().AssemblyQualifiedName);
			if (selectedObject.Attach != null)
				attach.Name = selectedObject.Attach.Name;
			if (attach is BasicAttach batt)
			{
				batt.VertexName = "vertex_" + Extensions.GenerateIdentifier();
				batt.NormalName = "normal_" + Extensions.GenerateIdentifier();
				batt.MaterialName = "material_" + Extensions.GenerateIdentifier();
				batt.MeshName = "mesh_" + Extensions.GenerateIdentifier();
				foreach (NJS_MESHSET m in batt.Mesh)
				{
					m.PolyName = "poly_" + Extensions.GenerateIdentifier();
					m.PolyNormalName = "polynormal_" + Extensions.GenerateIdentifier();
					m.UVName = "uv_" + Extensions.GenerateIdentifier();
					m.VColorName = "vcolor_" + Extensions.GenerateIdentifier();
				}
			}
			else if (attach is ChunkAttach catt)
			{
				catt.VertexName = "vertex_" + Extensions.GenerateIdentifier();
				catt.PolyName = "poly_" + Extensions.GenerateIdentifier();
			}
			selectedObject.Attach = attach;
			attach.ProcessVertexData();
			NJS_OBJECT[] models = model.GetObjects();
			try { meshes[Array.IndexOf(models, selectedObject)] = attach.CreateD3DMesh(d3ddevice); }
			catch { }
			DrawLevel();
		}

		private void editMaterialsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (MaterialEditor dlg = new MaterialEditor(((BasicAttach)selectedObject.Attach).Material, TextureInfo))
			{
				dlg.FormUpdated += (s, ev) => DrawLevel();
				dlg.ShowDialog(this);
			}
		}

		private void importOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dlg = new OpenFileDialog()
			{
				DefaultExt = "obj",
				Filter = "OBJ Files|*.obj"
			})
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					Attach newattach = Direct3D.Extensions.obj2nj(dlg.FileName, TextureInfo?.Select(a => a.Name).ToArray());
					if (selectedObject.Attach != null)
						newattach.Name = selectedObject.Attach.Name;
					meshes[Array.IndexOf(model.GetObjects(), selectedObject)] = newattach.CreateD3DMesh(d3ddevice);
					selectedObject.Attach = newattach;
					DrawLevel();
				}
		}

		private void exportOBJToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog a = new SaveFileDialog
			{
				DefaultExt = "obj",
				Filter = "OBJ Files|*.obj",
				FileName = selectedObject.Name
			})
				if (a.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter objstream = new StreamWriter(a.FileName, false))
					using (StreamWriter mtlstream = new StreamWriter(Path.ChangeExtension(a.FileName, "mtl"), false))
					{
						#region Material Exporting
						string materialPrefix = selectedObject.Name;

						objstream.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(a.FileName) + ".mtl");

						// This is admittedly not an accurate representation of the materials used in the model - HOWEVER, it makes the materials more managable in MAX
						// So we're doing it this way. In the future we should come back and add an option to do it this way or the original way.
						for (int texIndx = 0; texIndx < TextureInfo.Length; texIndx++)
						{
							mtlstream.WriteLine(String.Format("newmtl {0}_material_{1}", materialPrefix, texIndx));
							mtlstream.WriteLine("Ka 1 1 1");
							mtlstream.WriteLine("Kd 1 1 1");
							mtlstream.WriteLine("Ks 0 0 0");
							mtlstream.WriteLine("illum 1");

							if (!string.IsNullOrEmpty(TextureInfo[texIndx].Name))
							{
								mtlstream.WriteLine("Map_Kd " + TextureInfo[texIndx].Name + ".png");

								// save texture
								string mypath = Path.GetDirectoryName(a.FileName);
								TextureInfo[texIndx].Image.Save(Path.Combine(mypath, TextureInfo[texIndx].Name + ".png"));
							}
						}
						#endregion

						bool errorFlag = false;

						Direct3D.Extensions.WriteSingleModelAsObj(objstream, selectedObject, materialPrefix, ref errorFlag);

						if (errorFlag) MessageBox.Show("Error(s) encountered during export. Inspect the output file for more details.");
					}
				}
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			UpdateWeightedModel();
			DrawLevel();
		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (suppressTreeEvent) return;
			selectedObject = (NJS_OBJECT)e.Node.Tag;
			SelectedItemChanged();
		}

		private void primitiveRenderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DrawLevel();
		}

		private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EditorOptionsEditor optionsEditor = new EditorOptionsEditor(cam);
			optionsEditor.FormUpdated += optionsEditor_FormUpdated;
			optionsEditor.Show();
		}

		void optionsEditor_FormUpdated()
		{
			DrawLevel();
		}

		private void findToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (FindDialog dlg = new FindDialog())
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					NJS_OBJECT obj = model.GetObjects().SingleOrDefault(o => o.Name == dlg.SearchText || (o.Attach != null && o.Attach.Name == dlg.SearchText));
					if (obj != null)
					{
						selectedObject = obj;
						SelectedItemChanged();
					}
					else
						MessageBox.Show(this, "Not found.", "SAMDL");
				}
		}

		private void showModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			DrawLevel();
		}

		private void showNodesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			DrawLevel();
		}

		private void showNodeConnectionsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			DrawLevel();
		}
	}
}