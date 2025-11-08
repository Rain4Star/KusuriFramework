using System;
using UnityEngine;

namespace KUISys
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class UIElemAttribute : Attribute
	{
		public string resPath { get; }
		public byte openType { get; }
		public byte layer { get; }
		public Type[] uiDeps { get; }

		/// <summary>
		/// ui 配置注解
		/// </summary>
		/// <param name="resPath">ui 预设路径</param>
		/// <param name="openType">ui 打开方式：[0:手动管理]  [1:栈式打开，隐藏栈顶]  [2:栈式打开，关闭栈顶]  [3:堆式打开，与栈顶合批处理]</param>
		/// <param name="layer"></param>
		/// <param name="uiDeps">依赖列表，在加载 ui 前，会先加载依赖</param>
		public UIElemAttribute(string resPath, byte openType = 0, byte layer = 0, Type[] uiDeps = null)
		{
			this.resPath = resPath;
			this.openType = openType;
			this.uiDeps = uiDeps;
			this.layer = layer;
		}
	}

	public class UIElem : MonoBehaviour
	{
		public virtual void Clear() { }

		public virtual void OnShow() { }

		public virtual void OnHide() { }

		public virtual void Show() { }
	}
}
