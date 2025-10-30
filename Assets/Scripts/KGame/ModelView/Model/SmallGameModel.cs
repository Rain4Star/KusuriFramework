using KModel;
using System.Collections.Generic;
using UnityEngine;

namespace KModel
{
	public class SmallGameModel : ModelBase
	{
		// TO_CONFIG 等配置系统做完后，这里读取配置文件
		public List<(string name, string desc, Color iconClr)> smallGameCfg = new()
		{
			("000  SanXiao", "this is SanXiao", Color.blue),
			("000  AAA", "this is AAA", Color.blue),
			("000  BBB", "this is BBB", Color.blue),
			("000  CCC", "this is CCC", Color.blue),
			("000  DDD", "this is DDD", Color.blue),
			("000  EEE", "this is EEE", Color.blue),
			("000  SanXiao", "this is SanXiao", Color.blue),
			("000  SanXiao", "this is SanXiao", Color.blue),
			("111  SanXiao", "this is SanXiao", Color.blue),
			("111  AAA", "this is AAA", Color.blue),
			("111  BBB", "this is BBB", Color.blue),
			("111  CCC", "this is CCC", Color.blue),
			("111  DDD", "this is DDD", Color.blue),
			("111  EEE", "this is EEE", Color.blue),
			("111  SanXiao", "this is SanXiao", Color.blue),
			("111  SanXiao", "this is SanXiao", Color.blue),
			("111  SanXiao", "this is SanXiao", Color.blue),
			("222  AAA", "this is AAA", Color.blue),
			("222  BBB", "this is BBB", Color.blue),
			("222  CCC", "this is CCC", Color.blue),
			("222  DDD", "this is DDD", Color.blue),
			("222  EEE", "this is EEE", Color.blue),
			("222  SanXiao", "this is SanXiao", Color.blue),
			("222  SanXiao", "this is SanXiao", Color.blue),
		};
	}
}
