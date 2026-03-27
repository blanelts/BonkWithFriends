using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MelonLoader;

namespace Megabonk.BonkWithFriends.Networking.Messages;

internal sealed class NetworkMessageDispatcher
{
	private const int DefaultNetworkMessageCacheSize = 32;

	private const int DefaultHandlerSize = 16;

	private Dictionary<Type, NetworkMessageAttribute> _networkMessageCache;

	private Dictionary<MessageType, MessageSendFlags> _sendFlagsCache;

	private Dictionary<MessageType, SteamNetworkMessageDelegate> _handlers;

	private bool _isServer;

	internal bool IsSetup { get; private set; }

	internal NetworkMessageDispatcher(bool isServer)
	{
		_isServer = isServer;
		_networkMessageCache = new Dictionary<Type, NetworkMessageAttribute>(32);
		_sendFlagsCache = new Dictionary<MessageType, MessageSendFlags>(32);
		_handlers = new Dictionary<MessageType, SteamNetworkMessageDelegate>(16);
		Setup();
	}

	private void Setup()
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		Type[] array = types;
		foreach (Type type in array)
		{
			if (!(type == null) && type.IsSubclassOf(typeof(MessageBase)))
			{
				NetworkMessageAttribute customAttribute = type.GetCustomAttribute<NetworkMessageAttribute>();
				if (customAttribute != null)
				{
					_networkMessageCache[type] = customAttribute;
					_sendFlagsCache[customAttribute.Type] = customAttribute.SendFlags;
				}
			}
		}
		array = types;
		foreach (Type type2 in array)
		{
			if (type2 == null)
			{
				continue;
			}
			Melon<BonkWithFriendsMod>.Logger.Msg("[Setup] Processing type " + type2.FullName);
			MethodInfo[] methods = type2.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo[] methods2 = type2.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MemberInfo memberInfo = null;
			FieldInfo field = type2.GetField("Instance", BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			PropertyInfo property = type2.GetProperty("Instance", BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				memberInfo = field;
			}
			else if (property != null)
			{
				memberInfo = property;
			}
			Melon<BonkWithFriendsMod>.Logger.Msg("[Setup] Processing static methods");
			MethodInfo[] array2 = methods;
			foreach (MethodInfo methodInfo in array2)
			{
				if (methodInfo == null)
				{
					continue;
				}
				NetworkMessageHandlerAttribute customAttribute2 = methodInfo.GetCustomAttribute<NetworkMessageHandlerAttribute>();
				if (customAttribute2 == null || customAttribute2.Type == MessageType.None)
				{
					continue;
				}
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{"Setup"}] Processing: {methodInfo.IsStatic} {type2.FullName}::{methodInfo.Name}");
				ParameterInfo returnParameter = methodInfo.ReturnParameter;
				if (returnParameter == null || returnParameter.ParameterType != typeof(void))
				{
					continue;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters == null || parameters.Length != 1)
				{
					continue;
				}
				ParameterInfo parameterInfo = parameters[0];
				if (parameterInfo != null && !(parameterInfo.ParameterType != typeof(SteamNetworkMessage)))
				{
					MessageType type3 = customAttribute2.Type;
					ParameterExpression parameterExpression = Expression.Parameter(typeof(SteamNetworkMessage), "SteamNetworkMessage".ToLower());
					Expression<Action<SteamNetworkMessage>> expression = Expression.Lambda<Action<SteamNetworkMessage>>(Expression.Call(methodInfo, parameterExpression), new ParameterExpression[1] { parameterExpression });
					SteamNetworkMessageDelegate value = expression.Compile().Invoke;
					Melon<BonkWithFriendsMod>.Logger.Msg("[Setup] " + expression.ToString());
					if (type3 != MessageType.None)
					{
						_handlers[type3] = value;
					}
					Melon<BonkWithFriendsMod>.Logger.Msg($"[{"Setup"}] Processed: {type2.FullName}::{methodInfo.Name}");
				}
			}
			Melon<BonkWithFriendsMod>.Logger.Msg("[Setup] Processing instance methods");
			array2 = methods2;
			foreach (MethodInfo methodInfo2 in array2)
			{
				if (methodInfo2 == null)
				{
					continue;
				}
				NetworkMessageHandlerAttribute customAttribute3 = methodInfo2.GetCustomAttribute<NetworkMessageHandlerAttribute>();
				if (customAttribute3 == null || customAttribute3.Type == MessageType.None)
				{
					continue;
				}
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{"Setup"}] Processing: {methodInfo2.IsStatic} {type2.FullName}::{methodInfo2.Name}");
				if (memberInfo == null)
				{
					throw new NullReferenceException("fieldOrPropertyInfo for: " + type2.FullName);
				}
				ParameterInfo returnParameter2 = methodInfo2.ReturnParameter;
				if (returnParameter2 == null || returnParameter2.ParameterType != typeof(void))
				{
					continue;
				}
				ParameterInfo[] parameters2 = methodInfo2.GetParameters();
				if (parameters2 == null || parameters2.Length != 1)
				{
					continue;
				}
				ParameterInfo parameterInfo2 = parameters2[0];
				if (parameterInfo2 == null || parameterInfo2.ParameterType != typeof(SteamNetworkMessage))
				{
					continue;
				}
				MessageType type4 = customAttribute3.Type;
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(SteamNetworkMessage), "SteamNetworkMessage".ToLower());
				Type baseType = memberInfo.GetType().BaseType;
				Type? baseType2 = baseType.BaseType;
				MemberExpression memberExpression = null;
				if (baseType2 == typeof(FieldInfo))
				{
					FieldInfo field2 = memberInfo as FieldInfo;
					memberExpression = Expression.Field(null, field2);
				}
				else if (baseType == typeof(PropertyInfo))
				{
					PropertyInfo property2 = memberInfo as PropertyInfo;
					memberExpression = Expression.Property(null, property2);
				}
				if (memberExpression == null)
				{
					continue;
				}
				Type type5 = memberExpression.Type;
				MethodCallExpression ifTrue = Expression.Call(memberExpression, methodInfo2, parameterExpression2);
				Expression<Action<SteamNetworkMessage>> expression2 = Expression.Lambda<Action<SteamNetworkMessage>>(Expression.IfThen(Expression.NotEqual(memberExpression, Expression.Constant(null, type5)), ifTrue), new ParameterExpression[1] { parameterExpression2 });
				Action<SteamNetworkMessage> action = expression2.Compile();
				Melon<BonkWithFriendsMod>.Logger.Msg("[Setup] " + expression2.ToString());
				SteamNetworkMessageDelegate b = action.Invoke;
				if (type4 != MessageType.None)
				{
					if (_handlers.TryGetValue(type4, out var value2))
					{
						_handlers[type4] = (SteamNetworkMessageDelegate)Delegate.Combine(value2, b);
						Melon<BonkWithFriendsMod>.Logger.Msg($"Assigned another handler for message type: {type4}");
					}
					else
					{
						_handlers[type4] = b;
						Melon<BonkWithFriendsMod>.Logger.Msg($"Assigned handler for message type: {type4}");
					}
				}
				Melon<BonkWithFriendsMod>.Logger.Msg($"[{"Setup"}] Processed: {type2.FullName}::{methodInfo2.Name}");
			}
		}
		IsSetup = true;
	}

	internal SteamNetworkMessageDelegate GetHandler(MessageType messageType)
	{
		Dictionary<MessageType, SteamNetworkMessageDelegate> handlers = _handlers;
		if (handlers != null && handlers.Count > 0 && messageType != MessageType.None && _handlers.TryGetValue(messageType, out var value))
		{
			return value;
		}
		return null;
	}

	internal SteamNetworkMessageDelegate GetHandler(Type type)
	{
		if (!_networkMessageCache.TryGetValue(type, out var value))
		{
			return null;
		}
		MessageType type2 = value.Type;
		if (type2 == MessageType.None)
		{
			return null;
		}
		Dictionary<MessageType, SteamNetworkMessageDelegate> handlers = _handlers;
		if (handlers != null && handlers.Count > 0 && type2 != MessageType.None && _handlers.TryGetValue(type2, out var value2))
		{
			return value2;
		}
		return null;
	}

	internal (MessageType messageType, MessageSendFlags messageSendFlags) GetMessageTypeAndSendFlags(Type type)
	{
		if (!_networkMessageCache.TryGetValue(type, out var value))
		{
			return default((MessageType, MessageSendFlags));
		}
		if (value.Type == MessageType.None)
		{
			return default((MessageType, MessageSendFlags));
		}
		return (messageType: value.Type, messageSendFlags: value.SendFlags);
	}

	internal MessageSendFlags GetMessageSendFlags(MessageType messageType)
	{
		if (messageType == MessageType.None)
		{
			throw new ArgumentOutOfRangeException("messageType");
		}
		if (!_sendFlagsCache.TryGetValue(messageType, out var value))
		{
			throw new KeyNotFoundException("messageType");
		}
		return value;
	}

	internal void Dispatch(SteamNetworkMessage steamNetworkMessage)
	{
		if (!IsSetup || steamNetworkMessage == null)
		{
			return;
		}
		MessageType type = steamNetworkMessage.Type;
		Melon<BonkWithFriendsMod>.Logger.Msg($"{"Dispatch"} - {type}");
		if ((_isServer && (int)type > 1 && (int)type < 16384) || (!_isServer && (int)type > 16384 && (int)type < 32768))
		{
			return;
		}
		SteamNetworkMessageDelegate handler = GetHandler(type);
		if (handler == null)
		{
			return;
		}
		try
		{
			handler(steamNetworkMessage);
		}
		catch (Exception ex)
		{
			Melon<BonkWithFriendsMod>.Logger.Error((object)ex);
		}
	}

	internal void Reset()
	{
		_networkMessageCache?.Clear();
		_sendFlagsCache?.Clear();
		_handlers?.Clear();
	}
}
