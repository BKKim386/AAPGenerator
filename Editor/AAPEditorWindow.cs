using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AAPathGenerator
{
	public class AAPEditorWindow : EditorWindow
	{
		private const string SettingPath = "AAPathGenerator/AAPSettings.asset";
		private static AAPSettings _settings;
		private static AAPSettings Settings {
			get
			{
				if(_settings == null)
					GetSetting();
				
				return _settings;
			}
		}

		[MenuItem("Window/AAP Generator")]
		public static void ShowMyEditor()
		{
			// This method is called when the user selects the menu item in the Editor
			EditorWindow wnd = GetWindow<AAPEditorWindow>();
			wnd.titleContent = new GUIContent("AAP Generator");
			wnd.Show();	
		}

		private static void GetSetting()
		{
			_settings = AssetDatabase.LoadAssetAtPath<AAPSettings>($"Assets/{SettingPath}");
		}

		private void OnGUI()
		{
			if (GUILayout.Button("Generate"))
			{
				var generator = new ClassGenerator(Settings);
				var fullScript = generator.Generate();
				Debug.Log(fullScript);
				File.WriteAllText($"{Application.dataPath}/{Settings.SavePath}/{Settings.FileName}.cs", fullScript);
				AssetDatabase.Refresh(ImportAssetOptions.Default);
			}

			if (GUILayout.Button("Settings"))
			{
				EditorGUIUtility.PingObject(Settings);
				Selection.activeObject = Settings;
			}
		}
	}
}