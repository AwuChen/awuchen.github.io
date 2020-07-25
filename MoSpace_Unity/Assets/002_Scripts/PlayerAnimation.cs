using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public PlayerManager player;

    void Update()
    {
        if(animator != null && player != null)
            animator.SetBool("walking", player.walking);


        
    }
}
