using UnityEngine;
using System.Collections;
//This script executes commands to change character animations
[RequireComponent (typeof (Animator))]
public class Actions : MonoBehaviour {
 

 
	private Animator animator,animatorWeap;
 
	void Awake () {
		animator = GetComponent<Animator> ();

    }
 

	public void Idle1()
	{
		animator.SetBool ("Idle1", true);
	}
	public void Idle2()
	{
		animator.SetBool ("Idle2", true);
	}
	public void Hit1()
	{
		animator.SetBool ("Hit1", true);
	}
	public void Hit2()
	{
		animator.SetBool ("Hit2", true);
	}
	public void Walk()
	{
		animator.SetBool ("Walk", true);
	}
	public void Trot()
	{
		animator.SetBool ("Trot", true);
	}
	public void Dead1()
	{
		animator.SetBool ("Dead1", true);
	}
	public void Dead2()
	{
		animator.SetBool ("Dead2", true);
	}
	public void Canter()
	{
		animator.SetBool ("Canter", true);
	}
	public void MoveForward()
	{
		animator.SetBool ("MoveForward", true);
	}
	public void StrafeLeft()
	{
		animator.SetBool ("StrafeLeft", true);
	}
	public void StrafeRight()
	{
		animator.SetBool ("StrafeRight", true);
	}	
	public void TurnLeft()
	{
		animator.SetBool ("TurnLeft", true);
	}
	public void TurnRight()
	{
		animator.SetBool ("TurnRight", true);
	}
	public void Jump()
	{
		animator.SetBool ("Jump", true);
	}
	public void Lame()
	{
		animator.SetBool ("Lame", true);
	}
	public void OpenPanel()
	{
		animator.SetBool ("OpenPanel", true);
	}
 
	public void PowerOn()
	{
		animator.SetBool ("PowerOn", true);
	}
	public void PowerOff()
	{
		animator.SetBool ("PowerOff", true);
	}
	public void Attack()
	{
	 
		//animator.SetBool ("Attack", true);
		animator.SetLayerWeight(1,1);
 

	}
	public void StopAttack()
	{
 
 
		animator.SetLayerWeight(1,0);

	} 

}
