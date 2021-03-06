using System.Reflection.Emit;
using System;
//#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OyTools
{

	public class LayerAndTagEditTools
	{
		public const string nameSpace = "DragonPlus.Assets";

		public static void SetEditLayerAndTagSettings()
		{
			GameLayer.SetEditorTag();
			GameTag.SetEditorTag();
			GameSortingLayerEditor.SetEditorTag();
		}

		public static void GenLayerAndTagCode()
		{
			GameLayer.GenerateGameLayer();
			GameTag.GenerateGameTag();
			GameSortingLayerEditor.GameSortingLayer();
		}
	}

// namespace Games {
	public partial class GameLayer
	{
		[MenuItem("Edit/Game/SetEditorLayer")]
		public static void SetEditorTag()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			var customBeginIndex = GameUtils.layer_customBeginIndex;
			while (it.NextVisible(true))
			{
				if (it.name == "layers")
				{
					int end = Mathf.Min(customBeginIndex + GameUtils.customLayers.Length, it.arraySize);
					for (int i = customBeginIndex; i < end; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						dataPoint.stringValue = GameUtils.customLayers[i - customBeginIndex];
					}

					tagManager.ApplyModifiedProperties();
					if (customBeginIndex + GameUtils.customLayers.Length > 32)
					{
						Debug.LogFormat("<color=red>Layer不能超过31</color>");
					}

					break;
				}
			}
		}

		[MenuItem("Edit/Game/Generate GameLayer.cs")]
		public static void GenerateGameLayer()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "layers")
				{
					StringWriter sw = new StringWriter();
					sw.WriteLine($"namespace {LayerAndTagEditTools.nameSpace}");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic partial class GameLayer");
					sw.WriteLine("\t{");

					sw.WriteLine("\t\t#region Unity Default Lock");
					for (int i = 0; i < 8; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						sw.WriteLine($"\t\t//Layer{i}");
						string fieldname = string.IsNullOrEmpty(dataPoint.stringValue)
							? "Layer" + i
							: dataPoint.stringValue.Replace(" ", "_");
						sw.WriteLine(string.Format("\t\tpublic const int {0}\t=\t1 << {1};", fieldname, i));

					}

					sw.WriteLine("\t\t#endregion");
					sw.WriteLine("\n");

					List<string> fieldnames = new List<string>();
					for (int i = 8; i < it.arraySize; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						sw.WriteLine($"\t\t//Layer{i}");
						string fieldname = string.IsNullOrEmpty(dataPoint.stringValue)
							? "Layer" + i
							: dataPoint.stringValue.Replace(" ", "_");
						sw.WriteLine(string.Format("\t\tpublic const int {0}\t=\t1 << {1};", fieldname, i));
						fieldnames.Add(string.Format("\"{0}\"", fieldname));
					}

					string fieldnameStr = "";
					string gap = "";
					for (int i = 0; i < fieldnames.Count; i++)
					{
						fieldnameStr += gap;
						fieldnameStr += fieldnames[i];
						gap = ",";
					}

					sw.WriteLine("\n");
					sw.WriteLine("\t\tpublic static int           customBeginIndex = 8;");
					sw.WriteLine("\t\tpublic static string[]      customLayers = {" + fieldnameStr + "};");

					sw.WriteLine("\t}");
					sw.WriteLine("}");
					var path = Path.Combine(Application.dataPath, GameUtils.GEN_CODE_PATH, "GameLayer.cs");
					File.WriteAllText(path, sw.ToString(), System.Text.Encoding.UTF8);
					AssetDatabase.Refresh();
					break;
				}
			}
		}
	}

	public partial class GameTag
	{

		[MenuItem("Edit/Game/SetEditorTag")]
		public static void SetEditorTag()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "tags")
				{

					it.ClearArray();
					int end = GameUtils.tag_customBeginIndex + GameUtils.customTags.Length;
					if (it.arraySize < end)
					{
						it.arraySize = end;
					}

					for (int i = GameUtils.tag_customBeginIndex; i < end; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						dataPoint.stringValue = GameUtils.customTags[i - GameUtils.tag_customBeginIndex];
					}

					tagManager.ApplyModifiedProperties();

					break;
				}
			}
		}

		[MenuItem("Edit/Game/Generate GameTag.cs")]
		public static void GenerateGameTag()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "tags")
				{
					StringWriter sw = new StringWriter();
					sw.WriteLine($"namespace {LayerAndTagEditTools.nameSpace}");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic partial class GameTag");
					sw.WriteLine("\t{");

					sw.WriteLine("\t\t#region Unity Default Lock");
					sw.WriteLine("\t\tpublic const string Untagged        = \"Untagged\";");
					sw.WriteLine("\t\tpublic const string Respawn         = \"Respawn\";");
					sw.WriteLine("\t\tpublic const string Finish          = \"Finish\";");
					sw.WriteLine("\t\tpublic const string EditorOnly      = \"EditorOnly\";");
					sw.WriteLine("\t\tpublic const string MainCamera      = \"MainCamera\";");
					sw.WriteLine("\t\tpublic const string Player          = \"Player\";");
					sw.WriteLine("\t\tpublic const string GameController  = \"GameController\";");

					sw.WriteLine("\t\t#endregion");
					sw.WriteLine("\n");

					string fieldnameStr = "";
					string gap = "";

					for (int i = 0; i < it.arraySize; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						string fieldname = string.IsNullOrEmpty(dataPoint.stringValue)
							? "Tag" + i
							: dataPoint.stringValue.Replace(" ", "_");

						sw.WriteLine(string.Format("\t\tpublic const string {0}  = \"{1}\";", fieldname,
							dataPoint.stringValue));

						fieldnameStr += gap;
						fieldnameStr += string.Format("\"{0}\"", dataPoint.stringValue);
						gap = ",";
					}

					sw.WriteLine("\n");
					sw.WriteLine("\t\tpublic static int           customBeginIndex = 0;");
					sw.WriteLine("\t\tpublic static string[]      customTags = {" + fieldnameStr + "};");

					sw.WriteLine("\t}");
					sw.WriteLine("}");
					var path = Path.Combine(Application.dataPath, GameUtils.GEN_CODE_PATH, "GameTag.cs");
					File.WriteAllText(path, sw.ToString(), System.Text.Encoding.UTF8);
					AssetDatabase.Refresh();

					break;
				}
			}
		}
	}

	public class GameSortingLayerEditor
	{
		public static bool IsUseBitIndex = true;

		[MenuItem("Edit/Game/SetEditorSortingLayer")]
		public static void SetEditorTag()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "m_SortingLayers")
				{
					int[] ids;
					string[] names;
					GameUtils.GetValuesAndFieldNames<GameSortingLayer>(out ids, out names);
					it.ClearArray();

					int length = ids.Length;
					if (it.arraySize < length)
					{
						it.arraySize = length;
					}

					for (int i = 0; i < length; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						SerializedProperty namePoint = dataPoint.FindPropertyRelative("name");
						SerializedProperty uniqueIDPoint = dataPoint.FindPropertyRelative("uniqueID");
						namePoint.stringValue = names[i];
						uniqueIDPoint.intValue = IsUseBitIndex ? 1 << (ids[i] - 1) : ids[i];
					}

					tagManager.ApplyModifiedProperties();
					break;
				}
			}
		}

		[MenuItem("Edit/Game/Generate GameSortingLayer.cs")]
		public static void GameSortingLayer()
		{
			SerializedObject tagManager =
				new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
			SerializedProperty it = tagManager.GetIterator();
			while (it.NextVisible(true))
			{
				if (it.name == "m_SortingLayers")
				{
					StringWriter sw = new StringWriter();
					sw.WriteLine($"namespace {LayerAndTagEditTools.nameSpace}");
					sw.WriteLine("{");
					sw.WriteLine("\tpublic enum GameSortingLayer");
					sw.WriteLine("\t{");

					int bitIndex = 0;
					for (int i = 0; i < it.arraySize; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						SerializedProperty namePoint = dataPoint.FindPropertyRelative("name");
						SerializedProperty uniqueIDPoint = dataPoint.FindPropertyRelative("uniqueID");
						Debug.Log(namePoint.stringValue + " = " + uniqueIDPoint.intValue);
						if (uniqueIDPoint.intValue == 0)
						{
							bitIndex = i;
							break;
						}
					}

					Debug.Log(bitIndex);

					for (int i = 0; i < it.arraySize; i++)
					{
						SerializedProperty dataPoint = it.GetArrayElementAtIndex(i);
						SerializedProperty namePoint = dataPoint.FindPropertyRelative("name");
						SerializedProperty uniqueIDPoint = dataPoint.FindPropertyRelative("uniqueID");
						sw.WriteLine(string.Format("\t\t{0}\t=\t{1},\t\t//SortIndex{1}",
							namePoint.stringValue.Replace(" ", "_"),
							IsUseBitIndex ? i - bitIndex : uniqueIDPoint.intValue));
					}

					sw.WriteLine("\t}");
					sw.WriteLine("}");
					var path = Path.Combine(Application.dataPath, GameUtils.GEN_CODE_PATH, "GameSortingLayer.cs");
					File.WriteAllText(path, sw.ToString(), System.Text.Encoding.UTF8);
					AssetDatabase.Refresh();
					break;
				}
			}
		}
	}
}
//#endif