using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.AudioKit;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class PlayAudioClip : EnemyAction
    {
        public string audioClipName;
        public bool is3DSound;
        public float volume = 1;
        
        public override void OnStart()
        {
            base.OnStart();
            
            if(audioClipName == null)
                return;
            
            if(is3DSound)
                AudioSystem.Singleton.Play3DSound(audioClipName, transform.position, volume);
            else
                AudioSystem.Singleton.Play2DSound(audioClipName, volume);
        }
    }
}