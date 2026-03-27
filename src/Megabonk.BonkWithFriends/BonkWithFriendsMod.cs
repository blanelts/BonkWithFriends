using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Actors.Player;
using Il2CppSystem.Collections.Generic;
using Megabonk.BonkWithFriends.Debug;
using Megabonk.BonkWithFriends.Debug.UI;
using Megabonk.BonkWithFriends.Managers;
using Megabonk.BonkWithFriends.Steam;
using Megabonk.BonkWithFriends.UI;
using MelonLoader;
using UnityEngine;
using SteamManager = Megabonk.BonkWithFriends.Steam.SteamManager;

namespace Megabonk.BonkWithFriends;

internal sealed class BonkWithFriendsMod : MelonMod
{
	internal delegate void OnSceneWasLoadedDelegate(int buildIndex, string sceneName);

	internal delegate void OnSceneWasInitializedDelegate(int buildIndex, string sceneName);

	internal delegate void OnSceneWasUnloadedDelegate(int buildIndex, string sceneName);

	[HarmonyPatch(typeof(DataManager), "Load")]
	private static class DataManagerLoadPatch1
	{
		private static void Postfix(DataManager __instance)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			Melon<BonkWithFriendsMod>.Logger.Msg(new string('=', 20));
			Melon<BonkWithFriendsMod>.Logger.Msg("Skins for each character");
			Melon<BonkWithFriendsMod>.Logger.Msg("");
			Dictionary<ECharacter, List<SkinData>> skinData = __instance.skinData;
			if (skinData == null)
			{
				return;
			}
			Il2CppSystem.Collections.Generic.Dictionary<ECharacter, List<SkinData>>.Enumerator enumerator = skinData.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<ECharacter, List<SkinData>> current = enumerator.Current;
				if (current == null)
				{
					continue;
				}
				ECharacter key = current.Key;
				List<SkinData> value = current.Value;
				Melon<BonkWithFriendsMod>.Logger.Msg($"Character {key} has {value.Count} skins:");
				Il2CppSystem.Collections.Generic.List<SkinData>.Enumerator enumerator2 = value.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					SkinData current2 = enumerator2.Current;
					if (!((Object)(object)current2 == (Object)null))
					{
						Melon<BonkWithFriendsMod>.Logger.Msg($"Skin type: {current2.skinType}");
					}
				}
				Melon<BonkWithFriendsMod>.Logger.Msg("");
			}
			Melon<BonkWithFriendsMod>.Logger.Msg(new string('=', 20));
		}
	}

	[HarmonyPatch(typeof(MyPlayer), "Start")]
	private static class MyPlayerStartPatch1
	{
		private static void Postfix(MyPlayer __instance)
		{
			_ = (Object)(object)((Component)__instance).gameObject == (Object)null;
		}
	}

	[HarmonyPatch(typeof(MainMenu), "Start")]
	private static class MainMenuStartPatch1
	{
		private static void Postfix(MainMenu __instance)
		{
			try
			{
				CustomUiManager.LoadUiAssetBundle();
			}
			catch (Exception ex)
			{
				Melon<BonkWithFriendsMod>.Logger.Error((object)ex);
			}
		}
	}

	private GameObject _managersGameObject;

	private Megabonk.BonkWithFriends.Steam.SteamManager _steamManager;

	internal static OnSceneWasLoadedDelegate SceneWasLoaded;

	internal static OnSceneWasInitializedDelegate SceneWasInitialized;

	internal static OnSceneWasUnloadedDelegate SceneWasUnloaded;

	internal static BonkWithFriendsMod Instance { get; private set; }

	internal SynchronizationContext MainThreadSyncContext { get; private set; }

	public static bool IsSteamApiDllMissing { get; private set; }

	public override void OnEarlyInitializeMelon()
	{
		if (Instance != null)
		{
			throw new InvalidOperationException("Instance");
		}
		Instance = this;
	}

	public override void OnInitializeMelon()
	{
		MainThreadSyncContext = SynchronizationContext.Current;
		SetCustomDllImportResolver();
		SetupManagers();
		RunAllStaticConstructors();
	}

	private void SetCustomDllImportResolver()
	{
		System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => !a.FullName.Contains("il2cpp", StringComparison.OrdinalIgnoreCase) && a.FullName.Contains("Steamworks.NET", StringComparison.OrdinalIgnoreCase));
		if (assembly != null)
		{
			System.Runtime.InteropServices.NativeLibrary.SetDllImportResolver(assembly, DllImportResolver);
		}
	}

	private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (string.IsNullOrWhiteSpace(libraryName))
		{
			return IntPtr.Zero;
		}
		string value = "steam_api";
		if (libraryName.Contains(value))
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			if (string.IsNullOrWhiteSpace(currentDirectory) || !Directory.Exists(currentDirectory))
			{
				return IntPtr.Zero;
			}
			string[] source = new string[3] { ".dll", ".so", ".dylib" };
			foreach (string item in Directory.EnumerateFiles(currentDirectory, "*.*", SearchOption.TopDirectoryOnly))
			{
				string fileName = Path.GetFileName(item);
				if (!string.IsNullOrWhiteSpace(fileName) && fileName.Contains(value) && source.Any(fileName.EndsWith))
				{
					return System.Runtime.InteropServices.NativeLibrary.Load(item);
				}
			}
			string path = Path.Combine(currentDirectory, "plugins");
			if (Directory.Exists(path))
			{
				foreach (string item2 in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly))
				{
					string fileName2 = Path.GetFileName(item2);
					if (!string.IsNullOrWhiteSpace(fileName2) && fileName2.Contains(value) && source.Any(fileName2.EndsWith))
					{
						return System.Runtime.InteropServices.NativeLibrary.Load(item2);
					}
				}
			}
		}
		IsSteamApiDllMissing = true;
		return IntPtr.Zero;
	}

	private void SetupManagers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		_managersGameObject = new GameObject("BonkWithFriendsMod_Managers");
		Object.DontDestroyOnLoad((Object)(object)_managersGameObject);
		_steamManager = _managersGameObject.AddComponent<SteamManager>();
		_managersGameObject.AddComponent<TestModeGui>();
	}

	public override void OnLateInitializeMelon()
	{
	}

	public override void OnUpdate()
	{
		TestModeManager.Update();
		BotManager.Update();
		SpectatorManager.Update();
		ResurrectionManager.Update();
	}

	private static void RunAllStaticConstructors()
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			Melon<BonkWithFriendsMod>.Logger.Msg("Ran .cctor for " + type.FullName);
		}
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		SceneWasLoaded?.Invoke(buildIndex, sceneName);
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		SceneWasInitialized?.Invoke(buildIndex, sceneName);
		if (sceneName == "MainMenu")
		{
			MultiplayerUI.CreateMultiplayerMenus();
		}
		if (sceneName == "GeneratedMap")
		{
			PlayerSceneManager.OnSceneLoaded(sceneName);
		}
	}

	public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
	{
		SceneWasUnloaded?.Invoke(buildIndex, sceneName);
		if (sceneName == "GeneratedMap")
		{
			SpectatorManager.ExitSpectatorMode();
			ResurrectionManager.ClearState();
			PlayerSceneManager.OnSceneUnloaded(sceneName);
		}
	}
}
