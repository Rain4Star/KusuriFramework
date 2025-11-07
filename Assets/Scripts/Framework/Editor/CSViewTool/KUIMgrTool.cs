using KUISys;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[CustomEditor(typeof(UIMgr))]
public class KUIMgrTool : Editor
{
	FieldInfo _uiStkField, _uiStkTopField;
	public KUIMgrTool()
	{
		Type type = typeof(UIMgr);
		_uiStkField = type.GetField("_uiStk", BindingFlags.Instance | BindingFlags.NonPublic);
		_uiStkTopField = type.GetField("_uiStkTop", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		UIMgr mgr = (UIMgr)target;
		if (mgr == null) return;

		var stk = _uiStkField.GetValue(mgr) as List<HashSet<Type>>;
		var top = (int)_uiStkTopField.GetValue(mgr);
		if(stk == null) return;

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField($"UI Stack");
		EditorGUI.indentLevel++;
		for (int i = 0; i < top; i++)
		{
			EditorGUILayout.LabelField($"Level [{i}]", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			foreach (var type in stk[i])
			{
				EditorGUILayout.LabelField(type.Name);
			}
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;
	}
}