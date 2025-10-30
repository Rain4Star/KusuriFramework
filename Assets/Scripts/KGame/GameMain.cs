using UnityEngine;
using KGameClient;
using Kusuri;
using KUISys;
using Kusuri.GameUI;
using KModel;
using System;

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
		GameClient.Ins.StartClient(
			(Action)ModelMgr.Ins.GetModel<UserModel>().CS_TestAlive
			);
	}

	public void Start()
	{
		UIMgr.Ins.OpenWindow<LogInUI>();
	}

	private void Update()
	{
		MonoSingletonMgr.Update();
	}

	private void OnDisable()
	{
		
	}

	public void OnDestroy()
	{
		MonoSingletonMgr.OnDestroy();
	}
}
