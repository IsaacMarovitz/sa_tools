﻿using System.IO;
using System.Collections.Generic;
using SA_Tools;

namespace SonicRetro.SAModel.SAEditorCommon.UI
{
	public class ActionMappingList
	{
		public List<ActionKeyMapping> ActionKeyMappings { get; set; }

		public static ActionMappingList Load(string path, ActionKeyMapping[] defaultKeyMappings)
		{
			if (File.Exists(path))
			{
				return IniSerializer.Deserialize<ActionMappingList>(path);
			}
			else
			{
				ActionMappingList mappingList = new ActionMappingList();
				mappingList.ActionKeyMappings = new List<ActionKeyMapping>();

				foreach (ActionKeyMapping defaultMapping in defaultKeyMappings)
				{
					mappingList.ActionKeyMappings.Add(defaultMapping);
				}

				return mappingList;
			}
		}

		public void Save(string path)
		{
			if (!Directory.Exists(Path.GetDirectoryName(path)))
				Directory.CreateDirectory(Path.GetDirectoryName(path));
			IniSerializer.Serialize(this, path);
		}
	}
}
