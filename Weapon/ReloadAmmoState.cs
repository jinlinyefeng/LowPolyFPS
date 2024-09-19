using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAmmoState : StateMachineBehaviour
{
    public float reloadTime = 0.8f;//reload 动画播放到多少百分比部分（80%）
    private bool hasReload;//判断当前是否在换弹


    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasReload = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasReload) return;
        if(stateInfo.normalizedTime >= reloadTime)
        {
            if (animator.GetComponent<Weapon_AutomaticGun>() != null)
            {
                animator.GetComponent<Weapon_AutomaticGun>().ShotGunReload();
            }
            hasReload = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hasReload = false ;
    }

}
