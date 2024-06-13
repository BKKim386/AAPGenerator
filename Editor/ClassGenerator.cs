using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Video;

namespace AAPathGenerator
{
	public class Node
	{
		public string Name;
	}

	//Root노드역할을 함
	public class NamespaceNode : Node
	{
		public NamespaceNode(AAPSettings aapSettings, string groupName)
		{
			ChildrenClass = new Dictionary<string, ClassNode>();
			
			string fullNamespace = aapSettings.DefaultNamespace.Length > 0 ? aapSettings.DefaultNamespace : "";
			if (aapSettings.UseGroupAsNamespace)
			{
				if (fullNamespace.Length > 0)
				{
					fullNamespace += $".{groupName}";
				}
				else
				{
					fullNamespace += groupName;
				}
			}

			Name = fullNamespace;
		}

		public Dictionary<string, ClassNode> ChildrenClass;

		public void Add(string addressableName)
		{
			Queue<string> split = new Queue<string>(addressableName.Split("/"));

			var first = split.Dequeue();
			if (split.Count == 0) throw new AAPFormatExceptions(first);

			if (ChildrenClass.TryGetValue(first, out var child))
			{
				child.Add(split);
			}
			else
			{
				ChildrenClass.Add(first, new ClassNode(first, split));
			}
		}
	}

	public class ClassNode : Node
	{
		public ClassNode(string name, Queue<string> queue)
		{
			Name = name;
			ChildrenClass = new Dictionary<string, ClassNode>();
			ChildrenVariable = new Dictionary<string, VariableNode>();
			
			Add(queue);
		}
		
		public Dictionary<string, ClassNode> ChildrenClass;
		public Dictionary<string, VariableNode> ChildrenVariable;

		public void Add(Queue<string> queue)
		{
			if (queue.Count == 1)
			{
				AddChildVariable(queue.Dequeue());
			}
			else
			{
				AddChildClass(queue);
			}
		}
		
		public void AddChildClass(Queue<string> queue)
		{
			var name = queue.Dequeue();
			
			if (ChildrenClass.TryGetValue(name, out var node))
			{
				node.Add(queue);
			}
			else
			{
				ChildrenClass.Add(name, new ClassNode(name, queue));
			}
		}

		public void AddChildVariable(string childName)
		{
			if (ChildrenVariable.ContainsKey(childName))
			{
				throw new AAPDuplicateNameException(childName);
			}
			else
			{
				ChildrenVariable.Add(childName, new VariableNode(childName));
			}
			//ChildrenVariable.Add(childName, new VariableNode(childName));
		}
	}

	public class VariableNode
	{
		public VariableNode(string fullName)
		{
			var split = fullName.Split(".");
			Extension = split[1];
			Name = split[0];
			if (_typeMap.TryGetValue(Extension, out AssetType) == false)
				throw new AAPNotSupportTypeException(Extension);
		}

		private static readonly Dictionary<string, Type> _typeMap = new()
		{
			{"prefab", typeof(GameObject)},
			{"unity", typeof(SceneAsset)},
			{"mat", typeof(Material)},
			
			{"png", typeof(Texture2D)},
			{"jpg", typeof(Texture2D)},
			{"jpeg", typeof(Texture2D)},
			{"psd", typeof(Texture2D)},
			{"tga", typeof(Texture2D)},
			
			{"txt", typeof(TextAsset)},
			{"json", typeof(TextAsset)},
			{"xml", typeof(TextAsset)},
			{"bytes", typeof(TextAsset)},
			{"csv", typeof(TextAsset)},
			{"html", typeof(TextAsset)},
			
			{"mp3", typeof(AudioClip)},
			{"wav", typeof(AudioClip)},
			{"ogg", typeof(AudioClip)},
			
			{"mp4", typeof(VideoClip)},
			{"avi", typeof(VideoClip)},
			{"mov", typeof(VideoClip)},
			{"webm", typeof(VideoClip)},
			
			{"ttf", typeof(Font)},
			{"otf", typeof(Font)},
			{"fon", typeof(Font)},
		};

		public string Name;
		public string Extension;
		public Type AssetType;
	}
	
	
	public class ClassGenerator
	{
		public ClassGenerator(AAPSettings settings)
		{
			this._settings = settings;
		}

		private AAPSettings _settings;

		public string Generate()
		{
			var groups = GetAddressableGroups();
			return PrintScript(groups);
		}
		
		private List<AddressableAssetGroup> GetAddressableGroups()
		{
			AddressableAssetSettings addressableSettings = AddressableAssetSettingsDefaultObject.GetSettings(false);

			if (addressableSettings == null)
			{
				Debug.LogError("Addressable Asset Settings not exist");
				return null;
			}

			var groups = addressableSettings.groups.Where(group => _settings.Groups.Contains(group.name)).ToList();
			if (!groups.Any())
			{
				Debug.LogError("Target groups not found");
				return null; 
			}

			return groups;
		}

		private string PrintScript(List<AddressableAssetGroup> groups)
		{
			CodeGenStringBuilder builder = new CodeGenStringBuilder();

			builder.WriteLine($"using AAPathGenerator;");
			builder.WriteLine();
			
			foreach (var group in groups)
			{
				var namespaceNode = new NamespaceNode(_settings, group.name);

				var entries = new List<AddressableAssetEntry>();
				group.GatherAllAssets(entries, false, true, false);

				foreach (var entry in entries)
				{
					try
					{
						namespaceNode.Add(entry.address);
					}
					catch (Exception e)
					{
						Debug.LogError(e.Message);
						return string.Empty;
					}
				}

				PrintGroup(namespaceNode, builder);
				builder.WriteLine();
			}

			return builder.ToString();
		}

		private void PrintGroup(NamespaceNode root, CodeGenStringBuilder builder)
		{
			CodeGenStringBuilder.Scope namespaceScope = root.Name.Length > 0 ? builder.StartNamespaceScope(root.Name) : null;

			void PrintClassRecursive(ClassNode classNode, StringBuilder pathStack)
			{
				var classScope = builder.StartClassScope(classNode.Name);
				if (pathStack.Length > 0) pathStack.Append("/");
				pathStack.Append(classNode.Name);
				foreach (var pair in classNode.ChildrenClass)
				{
					PrintClassRecursive(pair.Value, pathStack);
				}

				if (classNode.ChildrenVariable.Count > 0)
				{
					string stack = pathStack.ToString();
					foreach (var pair in classNode.ChildrenVariable)
					{
						var node = pair.Value;
						var pathName = $"_path{node.Name}";
						classScope.WriteLine($"private const string {pathName} = \"{stack}/{node.Name}.{node.Extension}\";");
						classScope.WriteLine($"public static {node.AssetType} {node.Name} => AssetLoader.Load<{node.AssetType}>({pathName});");
					}					
				}
				
				classScope.Dispose();
			}

			var classStackStringBuilder = new StringBuilder();
			foreach (var pair in root.ChildrenClass)
			{
				PrintClassRecursive(pair.Value, classStackStringBuilder);
				classStackStringBuilder.Clear();
			}
			
			namespaceScope?.Dispose();
		}
	}
}