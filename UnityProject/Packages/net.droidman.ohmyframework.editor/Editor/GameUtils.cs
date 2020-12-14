using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;
/// <summary>
/// sortlayers
/// </summary>
public enum GameSortingLayer {
	War_Terrain = -2,
	War_Shadow = -1,
	Default = 0,
	War_Unit = 1,
	Background,
	Background1,
	UI,
	UI1,
	Foreground,
	Foreground1,
	Guide,
	Guide1,
	Avatar,
	Avatar1,
	Effect,
	Effect1,
	Text,
	Debug
}
public class GameUtils
{
	//存储layers，tags，sortinglayers 常量类，方便代码中引用
	public const string GEN_CODE_PATH = "Src/Games";
	//layer自定义起始index
	public static int layer_customBeginIndex = 8;
	//自定义layer 名字
	public static string[] customLayers = { "War_Terrain", "War_Unit", "War_Obstacle", "Background", "Background1", "UI1", "UI2", "Foreground", "Foreground1", "Guide", "Guide1", "Avatar", "Avatar1", "Effect", "Effect1", "Text", "Debug", "Environments", "Buildings", "Enemies", "Teammates", "Characters", "Protagonists", "Players" };
	//tag自定义起始index
	public static int tag_customBeginIndex = 0;
	//自定义tag
	public static string[] customTags = {"Except","Export","Environments","Buildings","Enemies","Teammates","Cameras","Characters","Lights","Protagonists","Players", "UICamera", "War_Terrain", "War_Unit" };
	/// <summary>
	/// 遍历枚举
	/// </summary>
	/// <param name="ids">枚举值</param>
	/// <param name="names">枚举名字</param>
	/// <typeparam name="T">枚举类</typeparam>
	public static void GetValuesAndFieldNames<T>(out int[] ids, out string[] names)
	{
		Type type = typeof(T);
		T[] valueArr =(T[]) Enum.GetValues(type);
		List<T> valueList = new List<T>(valueArr);
		valueList.Sort();

		names = new string[valueList.Count];
		ids = new int[valueList.Count];

		for(int i = 0; i < valueList.Count; i ++)
		{

			ids[i] = Convert.ToInt32(valueList[i]);
			names[i] = Enum.GetName(type, valueList[i]);
		}

	}
}
