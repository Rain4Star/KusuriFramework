using KModel;
using KUISys;
using Kusuri;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(UIElem), true)]
public class UiPrefabTools : Editor
{
	private static Assembly uiScriptAss = null;

	static UiPrefabTools()
	{
		uiScriptAss = typeof(ModelBase).Assembly;		// 由于 model 和 view 在一个程序集立，所有这样写
	}

	public override void OnInspectorGUI()
	{
		
		GameObject go = (target as UIElem).gameObject;
		if (go == null || go.name != "Prefab")
		{
			if (PrefabUtility.IsAnyPrefabInstanceRoot(go))
			{
				if (GUILayout.Button("选择场景"))
				{
					// 获取类型
					Type type = uiScriptAss.GetType($"Kusuri.GameUI.{go.name.Substring(go.name.LastIndexOf("#") + 1)}");
					if (type != null)
					{
						// 选择场景并高亮显示
						UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path.Combine("Assets", UIMgr.GetUIScenePath(type)));
						if (asset != null)
						{
							Selection.activeObject = asset;
							EditorGUIUtility.PingObject(asset);
						}
					}
				}
			}
			base.OnInspectorGUI();
			return;
		}

		GUILayout.BeginHorizontal();
		if (target.GetType() == typeof(UIElem))
		{
			if (GUILayout.Button("1.生成 CS 脚本"))
			{
				GenCSScript(go);
			}
			else if (GUILayout.Button("2.替换 CS 脚本"))
			{
				ReplaceCSScript(go);
			}
		}
		else 
		{
			if (GUILayout.Button("生成 UI 预制体"))
				SavePrefab(go);
			if (GUILayout.Button("选择预设"))
			{
				string path = EditorSceneManager.GetActiveScene().path;
				path = path.Substring(path.LastIndexOf("Scene") + 6).Replace(".unity", ".prefab");
				path = Path.Combine("Assets", UIMgr.uiRelativePath, path);
				UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
				if (asset != null)
				{
					Selection.activeObject = asset;
					EditorGUIUtility.PingObject(asset);
				}
			}
		}
		GUILayout.EndHorizontal();
		base.OnInspectorGUI();
	}


	// 生成 UI 脚本
	public void GenCSScript(GameObject go)
	{
		// 拼接类名
		string path = EditorSceneManager.GetActiveScene().path;
		path = path.Substring(path.LastIndexOf("Scene") + 6).Replace(".unity", "");

		UIElem comp = go.GetComponent<UIElem>();
		string csPath = Path.Combine(UIMgr.scPath, $"{path}.cs");
		if (comp != null)
		{
			if (!File.Exists(csPath))
			{
				string scDir = Path.GetDirectoryName(csPath);
				if(!Directory.Exists(scDir)) Directory.CreateDirectory(scDir);

				string clsName = Path.GetFileNameWithoutExtension(csPath);
				try
				{
					File.WriteAllText(csPath, BuildUIScript(path, clsName));
				}
				catch (Exception ex)
				{
					throw ex;
				}
				AssetDatabase.ImportAsset(csPath);
				AssetDatabase.Refresh();
			}
			else
			{
				Utils.Error($"CS 脚本已存在: {csPath}", "UI 预制体工具");
			}
		}
	}

	// 替换 UIElem 组件
	public void ReplaceCSScript(GameObject go)
	{
		// 拼接类名
		string path = EditorSceneManager.GetActiveScene().path;
		string clsName = $"Kusuri.GameUI.{Path.GetFileNameWithoutExtension(path)}";

		// 获取类型
		Type type = uiScriptAss.GetType(clsName);
		if (type != null)
		{
			// 添加组件
			Undo.DestroyObjectImmediate(go.GetComponent<UIElem>());
			go.AddComponent(type);
		}
		else
		{
			Utils.Error($"未找到对应的 ui 脚本： {clsName}", "UI 预制体工具");
		}
	}

	// 生成 UI 预制体
	public void SavePrefab(GameObject go)
	{
		// 拼接类名
		string path = EditorSceneManager.GetActiveScene().path;
		path = path.Substring(path.LastIndexOf("Scene") + 6).Replace(".unity", "");
		string prefabPath = Path.Combine(UIMgr.uiPath, $"{path}.prefab");
		string dir = Path.GetDirectoryName(prefabPath);
		if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);

		// 保存预制体
		go = Instantiate(go);
		ProcessNotExportToParent(go.transform);
		if (PrefabUtility.SaveAsPrefabAsset(go, prefabPath) == null)
		{
			Utils.Error("<color=#ac0000><size=18>导出失败</size></color>", "UI 预制体工具");
		}
		else
		{
			Utils.Print($"<color=#00ac00><size=18>导出成功</size>\n{prefabPath}</color>", "UI 预制体工具");
		}
		DestroyImmediate(go);
	}

	// 处理 UI 预设引用的其他预设（依赖的预设）
	public void ProcessNotExportToParent(Transform tf)
	{
		for (int i = tf.childCount - 1; i >= 0; i--)
		{
			Transform child = tf.GetChild(i);
			if (child.name.StartsWith("__NP__#"))
			{
				DestroyImmediate(child.gameObject);
				continue;
			}
			ProcessNotExportToParent(child);
		}
	}


	// 构造 UI 脚本
	public string BuildUIScript(string resPath, string clsName)
	{
		return
@$"using KUISys;

namespace Kusuri.GameUI
{{
	[UIElem(""{resPath}"")]
	public class {clsName} : {typeof(UIElem).Name}
	{{
		private void OnEnable()
		{{
			// 在此处添加事件监听
		}}

		private void OnDisable()
		{{
			// 在此处移除事件监听
		}}
	}}
}}
";
	}
}
