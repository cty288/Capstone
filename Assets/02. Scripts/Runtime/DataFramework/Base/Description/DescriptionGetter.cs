namespace Runtime.DataFramework.Description {
	public delegate string DescriptionGetter<T>(T targetObject);
	
	public interface IDescriptionGetter<T> {
		
		public string GetDescription(T targetObject);
	}
	
}