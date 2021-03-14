using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
	/// <summary>
	/// Inserts an object at the top of the Stack.
	/// </summary>
	/// <param name="item">The object to push onto the Stack. The value can be null for reference types.</param>
	/// <typeparam name="T">Type: T</typeparam>
	public static void Push<T>(this List<T> _this, T item)
	{
		if (_this == null)
        {
            return;
        }	

        _this.Add(item);
	}

	/// <summary>
	/// Removes and returns the object at the top of the Stack.
	/// </summary>
	/// <returns>The object removed from the top of the Stack.</returns>
	/// <typeparam name="T">Type: T</typeparam>
	public static T Pop<T>(this List<T> _this)
	{
        if (_this == null || _this.Count <= 0)
		{
            throw new System.InvalidOperationException("The Stack is empty.");
		}
        T item = _this[_this.Count - 1];
        _this.RemoveAt(_this.Count - 1);
        return item;
	}

	/// <summary>
	/// Returns the object at the top of the Stack without removing it.
	/// </summary>
	/// <returns>The Object at the top of the Stack.</returns>
	/// <typeparam name="T">Type: T</typeparam>
	public static T StackPeek<T>(this List<T> _this)
    {
		if (_this == null || _this.Count <= 0)
		{
            throw new System.InvalidOperationException("The Stack is empty.");
		}
		T item = _this[_this.Count - 1];
		return item;
    }

	/// <summary>
	/// Returns the object at the beginning of the Queue without removing it.
	/// </summary>
	/// <returns>The Object at the beginning of the Queue.</returns>
	/// <typeparam name="T">Type: T</typeparam>
	public static T QueuePeek<T>(this List<T> _this)
	{
		if (_this == null || _this.Count <= 0)
		{
			throw new System.InvalidOperationException("The Queue is empty.");
		}
		T item = _this[0];
		return item;
	}

	/// <summary>
	/// Removes and returns the object at the beginning of the Queue.
	/// </summary>
	/// <returns>The object that is removed from the beginning of the Queue.</returns>
	/// <param name="_this">This.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Dequeue<T>(this List<T> _this)
	{
		if (_this == null || _this.Count <= 0)
		{
			throw new System.InvalidOperationException("The Queue is empty.");
		}
		T item = _this[0];
        _this.RemoveAt(0);
		return item;
	}

	/// <summary>
	/// Adds an object to the end of the Queue.
	/// </summary>
	/// <returns>The enqueue.</returns>
	/// <param name="_this">This.</param>
	/// <param name="item">The object to add to the Queue. The value can be null.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void Enqueue<T>(this List<T> _this, T item)
	{
		if (_this == null)
		{
			return;
		}

		_this.Add(item);
	}
}
