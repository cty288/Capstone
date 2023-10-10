using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ILevelModel : IModel {
		BindableProperty<int> CurrentLevelCount { get; }
		
	}
	public class LevelModel: AbstractSavableModel, ILevelModel {
		[field: ES3Serializable] public BindableProperty<int> CurrentLevelCount { get; } = new BindableProperty<int>(1);
		
		
		
		
	}
}