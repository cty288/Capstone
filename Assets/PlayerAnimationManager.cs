using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;

public class PlayerAnimationManager : EntityAttachedViewController<PlayerEntity>
{
    [SerializeField] private Animator playerAnim;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        this.RegisterEvent<PlayerAnimationEvent>(OnPlayerAnimationEvent);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnPlayerAnimationEvent(PlayerAnimationEvent e)
    {
        Debug.Log(e.parameterName + e.flag);
        if (e.flag == 2)
        {
            Debug.Log("shootanim");
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

    protected override void OnEntityFinishInit(PlayerEntity entity)
    {
        
    }
}
