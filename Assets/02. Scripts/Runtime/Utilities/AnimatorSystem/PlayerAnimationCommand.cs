using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using UnityEngine;

namespace Runtime.Utilities.AnimatorSystem
{
    public enum AnimationEventType
    {
        Trigger,
        Bool,
        Float
    }
    public struct PlayerAnimationEvent
    {
        public string parameterName;
        public AnimationEventType type;
        public float flag;

        // flag 0 = false, 1= true, 2 = trigger
    }
    public class PlayerAnimationCommand : AbstractCommand<PlayerAnimationCommand> 
    {
        private string parameterName;
        private AnimationEventType type;
        private float flag;
        protected override void OnExecute()
        {
            this.SendEvent<PlayerAnimationEvent>(new PlayerAnimationEvent(){parameterName = parameterName,type=type,flag=flag});
        }

        public PlayerAnimationCommand()
        {
            
        }

        public static PlayerAnimationCommand Allocate(string parameterName,AnimationEventType type,float flag)
        {
            PlayerAnimationCommand command = SafeObjectPool<PlayerAnimationCommand>.Singleton.Allocate();
            command.parameterName = parameterName;
            command.type = type;
            command.flag = flag;
            return command;
        }
    }
}

