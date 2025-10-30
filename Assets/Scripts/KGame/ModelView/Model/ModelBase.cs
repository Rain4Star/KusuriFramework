namespace KModel
{
	/// <summary>
	/// model 的基类，model 视为特殊的单例
	/// 通过 ModelMgr.GetModel<T> 方法获取或创建
	/// 在 ModelMgr 中会处理 IMonoSingleton 接口的逻辑
	/// </summary>
	public class ModelBase
	{
		public virtual void Init() { }

		public virtual void Clear() { }
	}
}
