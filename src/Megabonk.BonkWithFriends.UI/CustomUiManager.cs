using System;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes;

using UnityEngine;

namespace Megabonk.BonkWithFriends.UI;

internal static class CustomUiManager
{
	private const string UiAssetBundleFileName = "bonkwithfriends.bwf";

	private const string UiAssetBundleManifestFileName = "bonkwithfriends.bwf.manifest";

	private static Il2CppSystem.Action<AsyncOperation> _onAssetBundleCreateRequestCompletedDelegate;

	private static Il2CppSystem.Action<AsyncOperation> _onAssetBundleRequestCompletedDelegate;

	static CustomUiManager()
	{
		BonkWithFriendsMod.SceneWasLoaded = (BonkWithFriendsMod.OnSceneWasLoadedDelegate)Delegate.Combine(BonkWithFriendsMod.SceneWasLoaded, new BonkWithFriendsMod.OnSceneWasLoadedDelegate(OnSceneWasLoaded));
		BonkWithFriendsMod.SceneWasInitialized = (BonkWithFriendsMod.OnSceneWasInitializedDelegate)Delegate.Combine(BonkWithFriendsMod.SceneWasInitialized, new BonkWithFriendsMod.OnSceneWasInitializedDelegate(OnSceneWasInitialized));
		BonkWithFriendsMod.SceneWasUnloaded = (BonkWithFriendsMod.OnSceneWasUnloadedDelegate)Delegate.Combine(BonkWithFriendsMod.SceneWasUnloaded, new BonkWithFriendsMod.OnSceneWasUnloadedDelegate(OnSceneWasUnloaded));
		_onAssetBundleCreateRequestCompletedDelegate = (Il2CppSystem.Action<AsyncOperation>)(System.Action<AsyncOperation>)OnAssetBundleCreateRequestCompleted;
		_onAssetBundleRequestCompletedDelegate = (Il2CppSystem.Action<AsyncOperation>)(System.Action<AsyncOperation>)OnAssetBundleRequestCompleted;
	}

	internal static void LoadUiAssetBundle()
	{
		string text = Path.Combine(Directory.GetCurrentDirectory(), "bonkwithfriends.bwf");
		if (!File.Exists(text))
		{
			throw new FileNotFoundException(text);
		}
		((AsyncOperation)AssetBundle.LoadFromFileAsync(text)).m_completeCallback = _onAssetBundleCreateRequestCompletedDelegate;
	}

	private static void OnAssetBundleCreateRequestCompleted(AsyncOperation operation)
	{
		((AsyncOperation)((Il2CppObjectBase)operation).Cast<AssetBundleCreateRequest>().assetBundle.LoadAssetAsync<GameObject>("Assets/Prefabs/CustomUiRoot.prefab")).m_completeCallback = _onAssetBundleRequestCompletedDelegate;
	}

	private static void OnAssetBundleRequestCompleted(AsyncOperation operation)
	{
		Object.Instantiate<GameObject>(((Il2CppObjectBase)((Il2CppObjectBase)operation).Cast<AssetBundleRequest>().asset).Cast<GameObject>());
	}

	private static void OnSceneWasLoaded(int buildIndex, string sceneName)
	{
		switch (buildIndex)
		{
		case 1:
			OnMainMenuSceneLoadedOrInitialized(initialized: false);
			break;
		case 2:
			OnGeneratedLevelSceneLoadedOrInitialized(initialized: false);
			break;
		case 3:
			OnLoadingScreenSceneLoadedOrInitialized(initialized: false);
			break;
		case -1:
		case 0:
			break;
		}
	}

	private static void OnSceneWasInitialized(int buildIndex, string sceneName)
	{
		switch (buildIndex)
		{
		case 1:
			OnMainMenuSceneLoadedOrInitialized(initialized: true);
			break;
		case 2:
			OnGeneratedLevelSceneLoadedOrInitialized(initialized: true);
			break;
		case 3:
			OnLoadingScreenSceneLoadedOrInitialized(initialized: true);
			break;
		case -1:
		case 0:
			break;
		}
	}

	private static void OnMainMenuSceneLoadedOrInitialized(bool initialized)
	{
	}

	private static void OnGeneratedLevelSceneLoadedOrInitialized(bool initialized)
	{
	}

	private static void OnLoadingScreenSceneLoadedOrInitialized(bool initialized)
	{
	}

	private static void OnSceneWasUnloaded(int buildIndex, string sceneName)
	{
		switch (buildIndex)
		{
		case 1:
			OnMainMenuSceneUnloaded();
			break;
		case 2:
			OnGeneratedLevelSceneUnloaded();
			break;
		case 3:
			OnLoadingScreenSceneUnloaded();
			break;
		case -1:
		case 0:
			break;
		}
	}

	private static void OnMainMenuSceneUnloaded()
	{
		ResetCustomMainMenuUi();
	}

	private static void OnGeneratedLevelSceneUnloaded()
	{
	}

	private static void OnLoadingScreenSceneUnloaded()
	{
	}

	private static void ResetCustomMainMenuUi()
	{
	}
}
