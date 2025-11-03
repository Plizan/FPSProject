using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class Utility
{
	public static Vector3 GetVector3(this ProtoPacket.Vector3 protoVec3) => new(protoVec3.X, protoVec3.Y, protoVec3.Z);

	public static ProtoPacket.Vector3 GetVector3(this Vector3 protoVec3) => new()
	{
		X = protoVec3.x,
		Y = protoVec3.y,
		Z = protoVec3.z
	};

	public static Quaternion GetQuaternion(this ProtoPacket.Quaternion protoQuat) => new(protoQuat.X, protoQuat.Y, protoQuat.Z, protoQuat.W);

	public static ProtoPacket.Quaternion GetQuaternion(this Quaternion quat) => new()
	{
		W = quat.w,
		X = quat.x,
		Y = quat.y,
		Z = quat.z
	};

	public static void SetActive(this Component component, bool isActive) => component.gameObject.SetActive(isActive);

	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		foreach (var item in enumerable)
		{
			action(item);
		}
	}

	public static void SetOnClick(this Button button, UnityAction action)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(action);
	}
}