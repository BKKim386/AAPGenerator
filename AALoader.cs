using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AAPathGenerator
{
	public static class AssetLoader
	{
		private static Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();

		public static T Load<T>(string name) where T : Object
		{
			if (_loadedAssets.TryGetValue(name, out var exist))
			{
				if (exist is MonoBehaviour mono)
					return mono.GetComponent<T>();
				
				return exist as T;
			}
			else
			{
				var ret = Addressables.LoadAssetAsync<T>(name).WaitForCompletion();
				return ret;	
			}
		}
	}
}