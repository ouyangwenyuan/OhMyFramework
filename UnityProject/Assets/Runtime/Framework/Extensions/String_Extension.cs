using System;
using UnityEngine;

public static class StringExtension
{
	public static bool IsEmptyString(this String _this)
	{
		return _this == null || _this == "" || _this == "none";
	}
}

