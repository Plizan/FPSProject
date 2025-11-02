using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : ManagerBase
{
	public static EventManager Get => Managers.Event;

	private readonly Dictionary<EventType, HashSet<IEventHandler>> _eventHandlersByEventType = new();
	private readonly Dictionary<EventType, List<EventTypesArgsBase>> _dispatchEvents = new();
	private Coroutine _dispatchCoroutine;
	public void RemoveHandler(EventType[] type, IEventHandler handler) => type?.ForEach(i => RemoveHandler(i, handler));
	public void RemoveHandler(IEventHandler handler) => _eventHandlersByEventType.Values.ForEach(i => i.Remove(handler));
	public void RemoveHandler(EventType type, IEventHandler handler) => _eventHandlersByEventType[type].Remove(handler);

	public void AddHandler(EventType[] type, IEventHandler handler) => type?.ForEach(i => AddHandler(i, handler));

	public void AddHandler(EventType type, IEventHandler handler)
	{
		if (_eventHandlersByEventType.TryGetValue(type, out var handlers))
			handlers.Add(handler);
		else
			_eventHandlersByEventType[type] = new() { handler };
	}

	private bool _isPause;

	public void SetPause(bool isPause)
	{
		_isPause = isPause;

		if (_isPause == false)
		{
			if (_dispatchEvents.Count > 0)
				_dispatchCoroutine ??= StartCoroutine(DispatchCoroutine());
		}
	}

	public void DispatchEvent(EventType eventType, EventTypesArgsBase args)
	{
		if (_dispatchEvents.TryGetValue(eventType, out var list) == false)
			list = _dispatchEvents[eventType] = new();

		var index = list.FindIndex(i => i.Equals(args));
		if (index > -1)
			list[index] = args;
		else
			list.Add(args);

		if (_isPause)
			return;

		_dispatchCoroutine ??= StartCoroutine(DispatchCoroutine());
	}

	private IEnumerator DispatchCoroutine()
	{
		var events = _dispatchEvents.ToDictionary(i => i.Key, i => i.Value);
		_dispatchEvents.Clear();
		yield return null;
		foreach (var (key, value) in events)
		{
			if (_eventHandlersByEventType.TryGetValue(key, out var handlers))
			{
				foreach (var handler in handlers.ToList())
				{
					foreach (var @event in value)
					{
						handler.OnEvent(key, @event);
					}
				}
			}
		}

		if (_isPause == false && _dispatchEvents.Count > 0)
			_dispatchCoroutine = StartCoroutine(DispatchCoroutine());
		else
			_dispatchCoroutine = null;
	}
}

public enum EventType
{
	LocalPlayerAttack,
}

public interface IEventHandler
{
	void OnEvent(EventType eventType, EventTypesArgsBase args);
}

public static class EventTypesArgsBaseUtility
{
	public static EventTypeArgs Get<EventTypeArgs>(this EventTypesArgsBase args) where EventTypeArgs : EventTypesArgsBase
	{
		return (EventTypeArgs)args;
	}
}

public class EventTypesArgsBase
{
	public virtual bool Equals(EventTypesArgsBase other)
	{
		return GetType() == other.GetType();
	}
}

public class LocalPlayerAttackEventArgs : EventTypesArgsBase
{
	public readonly int attackIndex;
	
	public LocalPlayerAttackEventArgs(int attackIndex = 0)
	{
		this.attackIndex = attackIndex;
	}
}