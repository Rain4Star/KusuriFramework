using System;
using Newtonsoft.Json;

namespace KModel
{
	public class BagItem
	{
		// id, cnt
		[JsonRequired]
		private int[] data;

		public override bool Equals(object? obj)
		{
			if (obj is BagItem other)
			{
				return other.Id == this.Id;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return MakeHashCode(Id);
		}

		public static int MakeHashCode(int id)
		{
			HashCode res = new();
			res.Add(id);
			return res.ToHashCode();
		}

		public BagItem(int id, int count)
		{
			data = new int[2];
			data[0] = id;
			data[1] = count;
		}

		[JsonIgnore]
		public int Id
		{
			get { return data[0]; }
			protected set { data[0] = value; }
		}

		[JsonIgnore]
		public int Count
		{
			get { return data[1]; }
			protected set { data[1] = value; }
		}

		public virtual void Use(int useType, int cnt)
		{

		}

		public virtual void Update(int opType, int cnt)
		{
			switch (opType)
			{
				case 0:     // 设置数量
					Count = cnt; break;
			}
		}
	}
}
