using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KModel
{
	public class AssetData
	{
		public int type;
		public int len;
		public int fragCount;
		public int fragSize;

		public byte finTp, finSub;  // 接收完后，需要执行的协议

		[JsonIgnore]
		public byte[] buffer;

		[JsonIgnore]
		private HashSet<int> _orderSet;

		public void Init()
		{
			fragCount = (len + fragSize - 1) / fragSize;
			_orderSet = new(fragCount);
			buffer = new byte[len];
		}

		public void Append(int order, byte[] frag)
		{
			if (_orderSet.Contains(order)) return;
			_orderSet.Add(order);
			int beg = fragSize * order;
			for (int i = beg, j = 0; i < Math.Min(beg + fragSize, len); i++, j++)
			{
				this.buffer[i] = frag[j];
			}
		}

		public bool IsFinshed()
		{
			return _orderSet.Count == fragCount;
		}

		public void Clear()
		{
			_orderSet.Clear();
		}
	}
}
