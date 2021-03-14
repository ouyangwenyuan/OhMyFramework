
using UnityEngine;

using System;
using System.Collections.Generic;

public static class GameObjectVisitor
{
	public delegate bool condition(GameObject obj);
	
	public static GameObject FindChild(this GameObject _this, string name)
	{
		return FindChildInCondition(_this, delegate(GameObject obj) {
			return (obj.name == name);
		});
	}

	public static GameObject FindChildInCondition(this GameObject _this, condition cond)
	{
		if (_this == null)
			return null;

		Transform transform = _this.GetComponent<Transform>();
		foreach (Transform trans in transform)
		{
			if (cond(trans.gameObject))
			{
				return trans.gameObject;
			}
			else
			{
				GameObject obj = trans.gameObject.FindChildInCondition(cond);
				if (obj)
					return obj;
			}
		}
		
		return null;
	}

	public static void FindChildren(this GameObject _this, string name, List<GameObject> objs)
	{
		FindChildrenInCondition(_this, objs, delegate(GameObject obj) {
			return (obj.name == name);
		});
	}

	public static void FindChildrenInCondition(this GameObject _this, List<GameObject> objs, condition cond)
	{
		if (_this == null)
			return;

		Transform transform = _this.GetComponent<Transform>();
		foreach (Transform trans in transform)
		{
			if (cond(trans.gameObject))
			{
				objs.Add(trans.gameObject);
			}
			
			trans.gameObject.FindChildrenInCondition(objs, cond);
		}
	}

	public static T FindComponentInChildren<T>(this GameObject _this)
		where T: MonoBehaviour
	{
		if (_this == null)
			return null;

		T ret = null;
		FindChildInCondition(_this, (obj) => {
			ret = obj.GetComponent<T>();
			return ret != null;
		});

		return ret;
	}

	public static void VisitChildren(this GameObject _this, Action<GameObject> fn)
	{
        foreach (Transform trans in _this.transform)
		{
			fn(trans.gameObject);
			trans.gameObject.VisitChildren(fn);
		}
	}

	public static void RemoveAllChildren(this GameObject _this)
	{
        foreach (Transform trans in _this.transform)
		{
            UnityEngine.Object.Destroy(trans.gameObject);
		}
	}
}

public static class MonoBehaviourVisitor
{
	public static GameObject FindChild(this MonoBehaviour _this, string name)
	{
		return _this.gameObject.FindChild(name);
	}
	
	public static void FindChildren(this MonoBehaviour _this, string name, List<GameObject> objs)
	{
		_this.gameObject.FindChildren(name, objs);
	}
}

public static class GameObject_MonoBehaviour
{
	public static T SafeGetComponent<T>(this GameObject _this)
		where T: Component
	{
		if (_this == null)
			return null;

		return _this.GetComponent<T>();
	}

	public static T GetOrCreateComponent<T>(this GameObject _this)
		where T: Component
	{
		if (_this == null)
			return null;

		var ret = _this.GetComponent<T>();
		return (ret == null) ? _this.AddComponent<T>() : ret;
	}
}

public static class Component_Extension
{
	public static T SafeGetComponent<T>(this Component _this)
		where T: MonoBehaviour
	{
		return (_this == null) ? null : _this.gameObject.SafeGetComponent<T>();
	}
	
	public static T GetOrCreateComponent<T>(this Component _this)
		where T: MonoBehaviour
	{
		return (_this == null) ? null : _this.gameObject.GetOrCreateComponent<T>();
	}
}

public static class GameObject_Instantiate
{
	public static GameObject Instantiate(this GameObject _this)
	{
		if (_this == null)
			return null;

		return GameObject.Instantiate(_this) as GameObject;
	}

	public static GameObject Instantiate(this GameObject _this, Vector3 position, Quaternion rotation)
	{
		if (_this == null)
			return null;

		GameObject ret = _this.Instantiate();
		ret.transform.localPosition = position;
		ret.transform.localRotation = rotation;

		return ret;
	}
}

public static class GameObject_Hierarchy
{
	public static void ChangeParent(this GameObject _this, GameObject parent, bool resetMat = true)
	{
		_this.transform.ChangeParent(parent.transform, resetMat);
	}

	public static void ChangeParent(this GameObject _this, GameObject parent, Vector3 localPosition)
	{
		_this.transform.ChangeParent(parent.transform, false);
		_this.transform.localPosition = localPosition;
	}
}

public static class MonoBehaviour_MonoBehaviour
{
	public static T AddComponent<T>(this MonoBehaviour _this)
		where T: MonoBehaviour
	{
		if (_this == null)
			return null;
		
		return _this.gameObject.AddComponent<T>();
	}

	public static T GetComponent<T>(this MonoBehaviour _this)
		where T: MonoBehaviour
	{
		if (_this == null)
			return null;
		
		return _this.gameObject.GetComponent<T>();
	}
}
