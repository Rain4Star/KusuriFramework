using UnityEngine;
using KGameClient;
using KUISys;
using Kusuri.GameUI;
using KModel;
using System;
using KConfig;
using Kusuri;
public class GameMain : MonoBehaviour
{
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

		UITools.Init();
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
