using Kusuri;
using UnityEditor;

public class LogTools : Editor
{
	[InitializeOnLoadMethod]
	public static void Init()
	{
		Utils.debug = EditorPrefs.GetBool("__DEBUG__");
		Utils.netDebug = EditorPrefs.GetBool("__NET_DEBUG__");
		Utils.eventDebug = EditorPrefs.GetBool("__EVENT_DEBUG__");
	}

	[MenuItem("Kusuri/Debug/关闭 日志")]
	public static void NoDebug()
	{
		Utils.debug = false;
		EditorPrefs.SetBool("__DEBUG__", false);
	}

	[MenuItem("Kusuri/Debug/开启 日志")]
	public static void DoDebug()
	{
		Utils.debug = true;
		EditorPrefs.SetBool("__DEBUG__", true);
	}

	[MenuItem("Kusuri/Debug/开启 事件日志")]
	public static void DoEventDebug()
	{
		Utils.eventDebug = true;
		EditorPrefs.SetBool("__EVENT_DEBUG__", true);
	}
	[MenuItem("Kusuri/Debug/关闭 事件日志")]
	public static void NoEventDebug()
	{
		Utils.eventDebug = false;
		EditorPrefs.SetBool("__EVENT_DEBUG__", false);
	}


	[MenuItem("Kusuri/Debug/关闭 网络日志")]
	public static void NoNetDebug()
	{
		Utils.netDebug = false;
		EditorPrefs.SetBool("__NET_DEBUG__", false);
	}

	[MenuItem("Kusuri/Debug/开启 网络日志")]
	public static void DoNetDebug()
	{
		Utils.netDebug = true;
		EditorPrefs.SetBool("__NET_DEBUG__", true);
	}
}