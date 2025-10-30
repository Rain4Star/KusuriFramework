using UnityEditor;
using UnityEngine;

// 预制体依赖处理工具

[InitializeOnLoad]
public class UiPrefabNPTool
{
	static UiPrefabNPTool()
	{
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
	}

	private static void OnHierarchyWindowItemOnGUI(int instanceId, Rect selecttionRect)
	{
		GameObject go = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
		if ((go == null)) return;

		// 不是预制体
		if (PrefabUtility.IsAnyPrefabInstanceRoot(go) == false) return;

		// 调整 toggle 的位置和大小
		selecttionRect.x += selecttionRect.width - 20;
		selecttionRect.width = 20;


		// 获取 toggle 的状态
		bool isSelect = go.name.StartsWith("__NP__#");
		// 绘制 toggle
		EditorGUI.BeginChangeCheck();
		bool newVal = EditorGUI.Toggle(selecttionRect, isSelect);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(go, "Toggle __NP__# suffix");
			if (newVal == true && isSelect == false) go.name = "__NP__#" + go.name;
			else if (newVal == false && isSelect == true) go.name = go.name.Substring(7);
			EditorUtility.SetDirty(go);
		}
	}
}

/*
[CustomEditor(typeof(Transform), editorForChildClasses : true)]
public class UiPrefabNPTool : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		GameObject go = (target as Transform).gameObject;
		if (go == null || PrefabUtility.IsAnyPrefabInstanceRoot(go) == false) return;
		bool isNp = go.name.StartsWith("__NP__#");
		if (isNp == true)
		{
			if (GUILayout.Button("导出")) go.name = go.name.Substring(7); 
		}
		else
		{
			if (GUILayout.Button("不导出")) go.name = "__NP__#" + go.name;
		}
	}
}*/