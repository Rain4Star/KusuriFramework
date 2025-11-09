using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KConfig
{
	public class CfgObj
	{
		public enum ECFG_TYPE
		{
			None,
			IntDic,
			StringDic,
			List,
		};

		public float lastCheck;
		public uint visit;
		public bool CanRemove;

		public object data;
		public int count;
		
		public ECFG_TYPE type = ECFG_TYPE.None;

		public T GetItem<T>(int id)
		{
			switch (type)
			{
				case ECFG_TYPE.IntDic:
				{
					if ((data as Dictionary<int, T>).TryGetValue(id, out var val))
					{
						visit++;
						return val;
					}
					break;
				}
				case ECFG_TYPE.List:
				{
					if (data is List<T> list)
					{
						visit++;
						if (id >= 0 && id < list.Count) return list[id];
					}
					break;
				}
			}
			return default;
		}
		public T GetItem<T>(string id)
		{
			if (type == ECFG_TYPE.StringDic)
			{
				if ((data as Dictionary<string, T>).TryGetValue(id, out var val))
				{
					visit++;
					return val;
				}
			}
			return default;
		}

		public Dictionary<int, T> GetIntDic<T>()
		{
			if (type != ECFG_TYPE.IntDic) throw new Exception("配置文件不是目标类型");
			return data as Dictionary<int, T>;
		}

		public Dictionary<string, string> GetStringDic<T>()
		{
			if (type != ECFG_TYPE.StringDic) throw new Exception("配置文件不是目标类型");
			return data as Dictionary<string, string>;
		}

		public List<T> GetList<T>()
		{
			if (type != ECFG_TYPE.List) throw new Exception("配置文件不是目标类型");
			return data as List<T>;
		}
	}
}
