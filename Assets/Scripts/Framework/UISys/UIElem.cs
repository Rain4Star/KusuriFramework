using System;
using UnityEngine;

namespace KUISys
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class UIElemAttribute : Attribute
	{
		public string resPath { get; }
		public byte openType { get; }
		public bool autoRemove { get; }
		public Type[] uiDeps { get; }

		public UIElemAttribute(string resPath, byte openType = 0, bool autoRemove = true, Type[] uiDeps = null)
		{
			this.resPath = resPath;
			this.autoRemove = this.autoRemove;
			this.openType = openType;
			this.uiDeps = uiDeps;
		}
	}

	public class UIElem : MonoBehaviour
	{
		public virtual int Layer() { return 0; }

		public virtual void Show() { }

		public virtual void AddListener() { }

		public virtual void RemoveListener() { }

	}
}
