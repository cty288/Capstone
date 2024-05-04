using MikroFramework.Architecture;
using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Levels.Commands
{
    public struct OnLoadingScreenHide {
        // public Transform targetTransform;
    }
    
    public class LoadingScreenHideCommand : AbstractCommand<LoadingScreenHideCommand> 
    {
        protected override void OnExecute() {
            this.SendEvent<OnLoadingScreenHide>(new OnLoadingScreenHide() {
            });
        }
		
        public static LoadingScreenHideCommand Allocate() {
            LoadingScreenHideCommand command = SafeObjectPool<LoadingScreenHideCommand>.Singleton.Allocate();
            return command;
        }
    }
}