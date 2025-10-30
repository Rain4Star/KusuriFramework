using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class RunMainTool
{
	static RunMainTool()
	{
		EditorApplication.playModeStateChanged += RunMain;
	}

	private static void RunMain(PlayModeStateChange stateChange)
	{
		if (stateChange == PlayModeStateChange.EnteredPlayMode)
		{
			GameObject targetGo = GameObject.Find("run_main");
			if (targetGo != null && targetGo.activeSelf)
			{
				SceneManager.LoadScene("GameMain", LoadSceneMode.Single);
			}
		}
	}
}
