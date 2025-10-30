
using Kusuri;
using System.Threading;
using UnityEditor;

public class NotAutoRebuildTool : Editor
{
	/// <summary>
	/// 重构之后加锁
	/// </summary>
	[InitializeOnLoadMethod]
	private static void Init()
	{
		EditorApplication.LockReloadAssemblies();
		Utils.Print("加锁");
	}


	/// <summary>
	/// 出现多重锁后，不手动解锁的话，只能重启unity，这里加一个手动解锁的方法
	/// </summary>
	[MenuItem("Kusuri/reload/unlock 程序集 _F7")]
	private static void Unlock()
	{
		EditorApplication.UnlockReloadAssemblies();
		Utils.Print("解锁");
	}

	/// <summary>
	/// 重新构建程序集
	/// </summary>
	[MenuItem("Kusuri/reload/rebuild 程序集 _F8")]
	private static void Rebuild()
	{
		EditorApplication.UnlockReloadAssemblies();
		Utils.Print("重建");
		EditorUtility.RequestScriptReload();
	}
}

