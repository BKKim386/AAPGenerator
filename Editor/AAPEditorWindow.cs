using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AAPathGenerator
{
	public class AAPEditorWindow : EditorWindow
	{
		private const string SettingPath = "AAPathGenerator/AAPSettings.asset";
		
		[MenuItem("Window/AAP Generator")]
		public static void ShowMyEditor()
		{
			// This method is called when the user selects the menu item in the Editor
			EditorWindow wnd = GetWindow<AAPEditorWindow>();
			wnd.titleContent = new GUIContent("AAP Generator");
			wnd.Show();	
		}

		private void OnGUI()
		{
			if (GUILayout.Button("Generate"))
			{
				var setting = AssetDatabase.LoadAssetAtPath<AAPSettings>($"Assets/{SettingPath}");
				if(setting == null) 
				{
					Debug.LogError("AAPSettingFile not exist");
					return;
				}
				
				var generator = new ClassGenerator(setting);
				var fullScript = generator.Generate();
				Debug.Log(fullScript);
				File.WriteAllText($"{Application.dataPath}/{setting.SavePath}/{setting.FileName}.cs", fullScript);
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}
		}
	}
}