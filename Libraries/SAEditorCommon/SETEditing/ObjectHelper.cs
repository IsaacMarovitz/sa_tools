﻿using SharpDX;
using SharpDX.Direct3D9;
using SAModel.Direct3D;
using SAModel.SAEditorCommon.DataTypes;
using SAModel.SAEditorCommon.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;
using Mesh = SAModel.Direct3D.Mesh;

namespace SAModel.SAEditorCommon.SETEditing
{
	public static class ObjectHelper
	{
		private static readonly FVF_PositionTextured[] SquareVerts = {
			new FVF_PositionTextured(new Vector3(-8, 8, 0), new Vector2(1, 0)),
			new FVF_PositionTextured(new Vector3(-8, -8, 0), new Vector2(1, 1)),
			new FVF_PositionTextured(new Vector3(8, 8, 0), new Vector2(0, 0)),
			new FVF_PositionTextured(new Vector3(8, -8, 0), new Vector2(0, 1))
		};
		private static readonly short[] SquareInds = { 0, 1, 2, 1, 3, 2 };
		private static NJS_OBJECT QuestionBoxModel;
		private static Mesh SquareMesh;
		private static Mesh QuestionBoxMesh;
		private static BoundingSphere SquareBounds;

		public enum RotType
		{
			X,
			Y,
			Z,
			XY,
			XZ,
			YX,
			YZ,
			ZX,
			ZY,
			XYZ,
			XZY,
			YXZ,
			YZX,
			ZXY,
			ZYX,
			NoRot
		}

		public enum SclType
		{
			X,
			Y,
			Z,
			XY,
			XZ,
			YZ,
			XYZ,
			AllX,
			AllY,
			AllZ,
			NoScl
		}

		public static void Init(Device device)
		{
			QuestionBoxModel = new ModelFile(Resources.questionmark).Model;
			QuestionBoxMesh = ObjectHelper.GetMeshes(QuestionBoxModel).First();
			SquareMesh = new Mesh<FVF_PositionTextured>(SquareVerts, new short[][] { SquareInds });
			SquareBounds = SharpDX.BoundingSphere.FromPoints(SquareVerts.Select(a => a.Position).ToArray()).ToSAModel();

			QuestionMark = Resources.questionmark_t.ToTexture(device);
		}

		internal static Texture QuestionMark;

		public static NJS_OBJECT LoadModel(string file)
		{
			return new ModelFile(file).Model;
		}

		public static Mesh[] GetMeshes(NJS_OBJECT model)
		{
			model.ProcessVertexData();
			NJS_OBJECT[] models = model.GetObjects();
			Mesh[] Meshes = new Mesh[models.Length];
			for (int i = 0; i < models.Length; i++)
				if (models[i].Attach != null)
					Meshes[i] = models[i].Attach.CreateD3DMesh();
			return Meshes;
		}

		public static Texture[] GetTextures(string name)
		{
			if (LevelData.Textures != null && LevelData.Textures.ContainsKey(name) && !EditorOptions.DisableTextures)
				return LevelData.Textures[name];
			return null;
		}

		public static HitResult CheckSpriteHit(Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{
			return SquareMesh.CheckHit(Near, Far, Viewport, Projection, View, transform);
		}

		public static RenderInfo[] RenderSprite(Device dev, MatrixStack transform, Texture texture, Vector3 center, bool selected)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			NJS_MATERIAL mat = new NJS_MATERIAL
			{
				DiffuseColor = Color.White
			};
			if (texture == null && !EditorOptions.DisableTextures)
				texture = QuestionMark;
			result.Add(new RenderInfo(QuestionBoxMesh, 0, transform.Top, mat, texture, dev.GetRenderState<FillMode>(RenderState.FillMode), new BoundingSphere(center.X, center.Y, center.Z, 8)));
			if (selected)
			{
				mat = new NJS_MATERIAL
				{
					DiffuseColor = Color.Yellow,
					UseAlpha = false
				};
				result.Add(new RenderInfo(QuestionBoxMesh, 0, transform.Top, mat, null, FillMode.Wireframe, new BoundingSphere(center.X, center.Y, center.Z, 8)));
			}
			return result.ToArray();
		}

		public static BoundingSphere GetSpriteBounds(MatrixStack transform)
		{
			return GetSpriteBounds(transform, 1);
		}

		public static BoundingSphere GetSpriteBounds(MatrixStack transform, float scale)
		{
			return GetModelBounds(QuestionBoxModel, transform, scale, new BoundingSphere());
		}

		public static float BAMSToRad(int BAMS)
		{
			return Direct3D.Extensions.BAMSToRad(BAMS);
		}

		public static int RadToBAMS(float rad)
		{
			return (int)(rad * (65536 / (2 * Math.PI)));
		}

		public static float BAMSToDeg(int BAMS)
		{
			return (float)(BAMS / (65536 / 360.0));
		}

		public static int DegToBAMS(float deg)
		{
			return (int)(deg * (65536 / 360.0));
		}

		public static float NJSin(int BAMS)
		{
			return Direct3D.Extensions.NJSin(BAMS);
		}

		public static float NJCos(int BAMS)
		{
			return Direct3D.Extensions.NJCos(BAMS);
		}

		public static BoundingSphere GetModelBounds(NJS_OBJECT model, MatrixStack transform)
		{
			return GetModelBounds(model, transform, 1);
		}

		public static BoundingSphere GetModelBounds(NJS_OBJECT model, MatrixStack transform, float scale)
		{
			return GetModelBounds(model, transform, scale, new BoundingSphere());
		}

		public static BoundingSphere GetModelBounds(NJS_OBJECT model, MatrixStack transform, float scale, BoundingSphere bounds)
		{
			transform.Push();
			model.ProcessTransforms(transform);
			scale *= Math.Max(Math.Max(model.Scale.X, model.Scale.Y), model.Scale.Z);
			if (model.Attach != null)
				bounds = Direct3D.Extensions.Merge(bounds, new BoundingSphere(Vector3.TransformCoordinate(model.Attach.Bounds.Center.ToVector3(), transform.Top).ToVertex(), model.Attach.Bounds.Radius * scale));
			foreach (NJS_OBJECT child in model.Children)
				bounds = GetModelBounds(child, transform, scale, bounds);
			transform.Pop();
			return bounds;
		}

		public static void RotateObject(MatrixStack transform, SETItem item, int addrx, int addry, int addrz, string type = "XYZ")
		{
			switch (type)
			{
				case "X":
					transform.NJRotateX(item.Rotation.X + addrx);
					break;
				case "Y":
					transform.NJRotateY(item.Rotation.Y + addry);
					break;
				case "Z":
					transform.NJRotateZ(item.Rotation.Z + addrz);
					break;
				case "XY":
					transform.NJRotateX(item.Rotation.X + addrx);
					transform.NJRotateY(item.Rotation.Y + addry);
					break;
				case "XZ":
					transform.NJRotateX(item.Rotation.X + addrx);
					transform.NJRotateZ(item.Rotation.Z + addrz);
					break;
				case "YX":
					transform.NJRotateY(item.Rotation.Y + addry);
					transform.NJRotateX(item.Rotation.X + addrx);
					break;
				case "YZ":
					transform.NJRotateY(item.Rotation.Y + addry);
					transform.NJRotateZ(item.Rotation.Z + addrz);
					break;
				case "ZX":
					transform.NJRotateZ(item.Rotation.Z + addrz);
					transform.NJRotateX(item.Rotation.X + addrx);
					break;
				case "ZY":
					transform.NJRotateZ(item.Rotation.Z + addrz);
					transform.NJRotateY(item.Rotation.Y + addry);
					break;
				case "XZY":
					transform.NJRotateX(item.Rotation.X + addrx);
					transform.NJRotateZ(item.Rotation.Z + addrz);
					transform.NJRotateY(item.Rotation.Y + addry);
					break;
				case "YXZ":
					transform.NJRotateY(item.Rotation.Y + addry);
					transform.NJRotateX(item.Rotation.X + addrx);
					transform.NJRotateZ(item.Rotation.Z + addrz);
					break;
				case "YZX":
					transform.NJRotateY(item.Rotation.Y + addry);
					transform.NJRotateZ(item.Rotation.Z + addrz);
					transform.NJRotateX(item.Rotation.X + addrx);
					break;
				case "ZXY":
					transform.NJRotateZ(item.Rotation.Z + addrz);
					transform.NJRotateX(item.Rotation.X + addrx);
					transform.NJRotateY(item.Rotation.Y + addry);
					break;
				case "ZYX":
					transform.NJRotateZYX(item.Rotation.X + addrx, item.Rotation.Y + addry, item.Rotation.Z + addrz);
					break;
				case "None":
					break;
				case "XYZ":
				default:
					transform.NJRotateXYZ(item.Rotation.X + addrx, item.Rotation.Y + addry, item.Rotation.Z + addrz);
					break;
			}
		}

		public static Vector3 GetScale(SETItem item, float addfx, float addfy, float addfz, string type = "None")
		{
			float x = 1;
			float y = 1;
			float z = 1;

			switch (type)
			{
				case "X":
					x = item.Scale.X;
					break;
				case "Y":
					y = item.Scale.Y;
					break;
				case "Z":
					z = item.Scale.Z;
					break;
				case "XY":
					x = item.Scale.X;
					y = item.Scale.Y;
					break;
				case "XZ":
					x = item.Scale.X;
					z = item.Scale.Z;
					break;
				case "YZ":
					y = item.Scale.Y;
					z = item.Scale.Z;
					break;
				case "XYZ":
					x = item.Scale.X;
					y = item.Scale.Y;
					z = item.Scale.Z;
					break;
				case "AllX":
					x = item.Scale.X;
					y = item.Scale.X;
					z = item.Scale.X;
					break;
				case "AllY":
					x = item.Scale.Y;
					y = item.Scale.Y;
					z = item.Scale.Y;
					break;
				case "AllZ":
					x = item.Scale.Z;
					y = item.Scale.Z;
					z = item.Scale.Z;
					break;
				case "None":
				default:
					break;
			}

			if (addfx != 0)
				x += addfx;
			if (addfy != 0)
				y += addfy;
			if (addfz != 0)
				z += addfz;

			return new Vector3(x, y, z);
		}
	}
}