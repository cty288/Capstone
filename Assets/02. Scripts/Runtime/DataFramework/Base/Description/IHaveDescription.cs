namespace Runtime.DataFramework.Description {
	public interface IHaveDescription<T> {
		public string GetDescription(T targetObject);
	}
}