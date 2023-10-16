using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;

public class PlayerAnimationManager : EntityAttachedViewController<PlayerEntity>
{
    [SerializeField] private Animator playerAnim;
    // Start is called before the first frame update
    void Start()
    {
        this.RegisterEvent<PlayerAnimationEvent>(OnPlayerAnimationEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPlayerAnimationEvent(PlayerAnimationEvent e)
    {
        if (e.flag == 2)
        {
            playerAnim.SetTrigger(e.parameterName);
        }
        else
        {
            bool b;
            if (e.flag == 1)
                b = true;
            else
                b = false;
            playerAnim.SetBool(e.parameterName,b);
        }
    }
}
