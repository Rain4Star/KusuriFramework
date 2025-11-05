using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KConfig
{
	/// <summary>
	/// 配置文件对应的类注解
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class CfgItemAttribute : Attribute
	{
		public string resPath { get; }
		public string mainKey { get; }
		public bool notClear { get; }

		public CfgItemAttribute(string resPath, string mainKey, bool notClear)
		{
			this.resPath = resPath;
			this.mainKey = mainKey;
			this.notClear = notClear;
		}
	}
}
