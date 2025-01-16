using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;

public static class AssetLoader
{
    public delegate void AssetLoadedCallback<T>(T asset);

    public static void LoadAssets<T>(string labelOrGroupName, Action<IList<T>> processAssetsMethod)
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(labelOrGroupName, null);

        handle.Completed += (operationHandle) =>
        {
            if (operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<T> loadedAssets = operationHandle.Result;
                processAssetsMethod?.Invoke(loadedAssets);
            }
            else
            {
                Debug.LogError("Failed to load assets: " + operationHandle.OperationException);
            }
        };
    }
}
