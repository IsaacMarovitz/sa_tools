﻿using SharpDX;
using SharpDX.Direct3D9;
using SAModel;
using SAModel.Direct3D;
using SAModel.SAEditorCommon.DataTypes;
using SAModel.SAEditorCommon.SETEditing;
using System.Collections.Generic;
using BoundingSphere = SAModel.BoundingSphere;
using Mesh = SAModel.Direct3D.Mesh;
using SAModel.SAEditorCommon;

namespace SADXObjectDefinitions.EmeraldCoast
{
	public abstract class Seagull : ObjectDefinition
	{
		protected NJS_OBJECT model;
		protected Mesh[] meshes;

		public override HitResult CheckHit(SETItem item, Vector3 Near, Vector3 Far, Viewport Viewport, Matrix Projection, Matrix View, MatrixStack transform)
		{

			transform.Push();
			transform.NJTranslate(item.Position);
			int RotY = item.Rotation.Y + 34;
			if ((RotY + 24) != 0x4000)
			{
				transform.NJRotateY((RotY + 24) - 0x4000);
			}
			transform.NJScale(1.5f, 1.5f, 1.5f);
			HitResult result = model.CheckHit(Near, Far, Viewport, Projection, View, transform, meshes);
			transform.Pop();
			return result;
		}

		public override List<RenderInfo> Render(SETItem item, Device dev, EditorCamera camera, MatrixStack transform)
		{
			List<RenderInfo> result = new List<RenderInfo>();
			transform.Push();
			transform.NJTranslate(item.Position);
			int RotY = item.Rotation.Y + 34;
			if ((RotY + 24) != 0x4000)
			{
				transform.NJRotateY((RotY + 24) - 0x4000);
			}
			transform.NJScale(1.5f, 1.5f, 1.5f);
			result.AddRange(model.DrawModelTree(dev.GetRenderState<FillMode>(RenderState.FillMode), transform, ObjectHelper.GetTextures("OBJ_BEACH"), meshes, EditorOptions.IgnoreMaterialColors, EditorOptions.OverrideLighting));
			if (item.Selected)
				result.AddRange(model.DrawModelTreeInvert(transform, meshes));
			transform.Pop();
			return result;
		}

		public override List<ModelTransform> GetModels(SETItem item, MatrixStack transform)
		{
			List<ModelTransform> result = new List<ModelTransform>();
			transform.Push();
			transform.NJTranslate(item.Position);
			int RotY = item.Rotation.Y + 34;
			if ((RotY + 24) != 0x4000)
			{
				transform.NJRotateY((RotY + 24) - 0x4000);
			}
			transform.NJScale(1.5f, 1.5f, 1.5f);
			result.Add(new ModelTransform(model, transform.Top));
			transform.Pop();
			return result;
		}

		public override BoundingSphere GetBounds(SETItem item)
		{
			MatrixStack transform = new MatrixStack();
			transform.NJTranslate(item.Position);
			int RotY = item.Rotation.Y + 34;
			if ((RotY + 24) != 0x4000)
			{
				transform.NJRotateY((RotY + 24) - 0x4000);
			}
			transform.NJScale(1.5f, 1.5f, 1.5f);
			return ObjectHelper.GetModelBounds(model, transform);
		}

		public override Matrix GetHandleMatrix(SETItem item)
		{
			Matrix matrix = Matrix.Identity;

			MatrixFunctions.Translate(ref matrix, item.Position);

			int RotY = item.Rotation.Y + 34;
			if ((RotY + 24) != 0x4000)
			{
				MatrixFunctions.RotateY(ref matrix, (RotY + 24) - 0x4000);
			}

			return matrix;
		}
	}

	public class Kamome : Seagull
	{
		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("stg01_beach/common/models/seaobj_kamome_a.nja.sa1mdl");
			meshes = ObjectHelper.GetMeshes(model);
		}

		public override string Name { get { return "Seagull (Animated)"; } }
	}

	public class Kamomel : Seagull
	{
		public override void Init(ObjectData data, string name)
		{
			model = ObjectHelper.LoadModel("stg01_beach/common/models/seaobj_kamome_b.nja.sa1mdl");
			meshes = ObjectHelper.GetMeshes(model);
		}

		public override string Name { get { return "Seagull"; } }
	}
}