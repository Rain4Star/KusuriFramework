using KConfig;
using Newtonsoft.Json;

namespace KModel
{
	public enum EBagSortType
	{
		
	}

	public enum EBagSelectType
	{
		Id,
		Name,
		Quality,
		Cat,
	}

	public class BagItem
	{
		public enum EUseType
		{
			Set = 0,
			Add = 1,
			Drop = 2,
			Sell = 3,
			Use = 4
		}

		// id, cnt
		[JsonRequired]
		private int[] data;

		[JsonIgnore]
		public ItemCfg cfg;

		public BagItem()
		{
			data = new int[2];
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

		public virtual void Set(int cnt)
		{
			Count = cnt;
		}

		public virtual void Add(int cnt)
		{
			Count += cnt;
		}

		public virtual void Sell(int cnt)
		{
			Count -= cnt;
		}

		public virtual void Drop(int cnt)
		{
			Count -= cnt;
		}

		public virtual void Use(int cnt)
		{
			Count -= cnt;
		}

		public virtual void Update(EUseType opType, int cnt)
		{
			switch (opType)
			{
				case EUseType.Set:
					Set(cnt); break;
				case EUseType.Add:
					Add(cnt); break;
				case EUseType.Drop:
					Drop(cnt); break;
				case EUseType.Sell:
					Sell(cnt); break;
				case EUseType.Use:
					Use(cnt); break;
			}
		}
	}
}
