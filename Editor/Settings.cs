using System;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AAPathGenerator
{
	[CreateAssetMenu]
	public class AAPSettings : ScriptableObject
	{
		//Target AddressableAssets Group Name
		public string[] Groups;
		
		//Save path where generate script
		public string SavePath;
		
		//name of .cs File
		public string FileName = "AAPGenerated";

		//Check this true for Group's name as Namespace
		public bool UseGroupAsNamespace;

		//Most front Namespace
		public string DefaultNamespace;
	}

	[CustomEditor(typeof(AAPSettings))]
	public class AAPSettingEditor : Editor
	{
		private AAPSettings aapSettings;
		private bool[] groupToggles;
		private string[] allGroups;
		
		private void OnEnable()
		{
			if(aapSettings == null)
				aapSettings = target as AAPSettings;
			
			var groups = AddressableAssetSettingsDefaultObject.GetSettings(false).groups;
			allGroups = groups.Select(group => group.Name).ToArray();

			groupToggles = new bool[allGroups.Length];
			for (int i = 0; i < groupToggles.Length; ++i)
			{
				groupToggles[i] = aapSettings.Groups != null && aapSettings.Groups.Contains(allGroups[i]);
			}
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Addressable Groups", EditorStyles.boldLabel);
			for (int i = 0; i < allGroups.Length; ++i)
			{
				groupToggles[i] = EditorGUILayout.Toggle(allGroups[i], groupToggles[i]);
			}
			
			if (GUILayout.Button("Save Addressable Groups"))
			{
				aapSettings.Groups = allGroups.Where((group, index) => groupToggles[index]).ToArray();
				
				EditorUtility.SetDirty(aapSettings);
			}
			
			base.OnInspectorGUI();
		}
	}
}