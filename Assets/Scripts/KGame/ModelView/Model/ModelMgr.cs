using Kusuri;
using System;
using System.Collections.Generic;

namespace KModel
{
	public class ModelMgr : Singleton<ModelMgr>, IMonoSingleton
	{
		private Dictionary<Type, ModelBase> _modelDic = new();

		void IMonoSingleton.Update() { }
		void IMonoSingleton.Destroy()
		{
			Clear();
		}

		public T GetModel<T>() where T : ModelBase, new()
		{
			Type type = typeof(T);
			if (_modelDic.TryGetValue(type, out var model))
			{
				return model as T;
			}
			T res = new T();
			res.Init();
			if (res is IMonoSingleton) MonoSingletonMgr.Add(res as IMonoSingleton);
			_modelDic[type] = res;
			return res;
		}

		/// <summary>
		/// 清理 model 的数据
		/// </summary>
		public void Clear()
		{
			foreach (var model in _modelDic.Values)
			{
				model.Clear();
			}
		}
	}
}
