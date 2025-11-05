using UnityEngine;
using KGameClient;
using KUISys;
using Kusuri.GameUI;
using KModel;
using System;
using KConfig;
using Kusuri;
using UnityEngine.UI;

public class GameMain : MonoBehaviour
{
	public Image img;

	public void OnGUI()
	{
		if (GUILayout.Button("111"))
		{
			UIMgr.Ins.OpenWindow<Stack1_1>();
		}
		else if (GUILayout.Button("111_111"))
		{
			UIMgr.Ins.OpenWindow<Stack1_11>();
		}
		else if (GUILayout.Button("222"))
		{
			UIMgr.Ins.OpenWindow<Stack2_1>();
		}
		else if (GUILayout.Button("Close_111"))
		{
			UIMgr.Ins.CloseWindow<Stack1_1>();
		}
		else if (GUILayout.Button("Close_111_111"))
		{
			UIMgr.Ins.CloseWindow<Stack1_11>();
		}
		else if (GUILayout.Button("Close_222"))
		{
			UIMgr.Ins.CloseWindow<Stack2_1>();
		}
	}

	public void Awake()
	{
		DontDestroyOnLoad(this);
		GameClient.Ins.InitProcessor(
			new (byte tp, CmdProcessor proc)[]
			{
				(1, new UserProcessor()),
				(255, new AssetProcessor()),
			}
		);
		GameClient.Ins.StartClient((Action)StartFunc);
	}

	private void StartFunc()
	{
		ModelMgr.Ins.GetModel<UserModel>().CS_TestAlive();
		
	}

	public void Start()
	{
		UIMgr.Ins.OpenWindow<LogInUI>();
	}
}
