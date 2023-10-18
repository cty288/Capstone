using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using UnityEngine;

namespace Runtime.Utilities.AnimatorSystem
{
    public struct PlayerAnimationEvent
    {
        public string parameterName;
        public int flag;
        
        // flag 0 = false, 1= true, 2 = trigger
    }
    public class PlayerAnimationCommand : AbstractCommand<PlayerAnimationCommand> 
    {
        private string parameterName;
        private int flag;
        protected override void OnExecute()
        {
            this.SendEvent<PlayerAnimationEvent>(new PlayerAnimationEvent(){parameterName = parameterName,flag=flag});
        }

        public PlayerAnimationCommand()
        {
            
        }

        public static PlayerAnimationCommand Allocate(string parameterName,int flag)
        {
            PlayerAnimationCommand command = SafeObjectPool<PlayerAnimationCommand>.Singleton.Allocate();
            command.parameterName = parameterName;
            command.flag = flag;
            return command;
        }
    }
}

