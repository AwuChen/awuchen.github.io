using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mopen : MonoBehaviour
{
    public UnityEvent enter;
    public UnityEvent exit;
    bool runOnce = false;
    void Start()
    {
        if(enter == null)
        enter = new UnityEvent();
        if(exit == null)
        exit = new UnityEvent();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NetworkPlayer")
        {
            enter.Invoke();
            other.gameObject.GetComponent<PlayerManager>().HidePlayer(true);
        }
        //if (!runOnce)
        //{
        //    StartCoroutine(Wait());
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "NetworkPlayer")
        {
            exit.Invoke();
            other.gameObject.GetComponent<PlayerManager>().HidePlayer(false);
        }
    }

    IEnumerator Wait()
    {
        //yield on a new YieldInstruction that waits for 3 seconds.
        yield return new WaitForSeconds(3);

        Application.OpenURL("https://shimo.im/boards/Vv7dJxrHziYmYUVC/");
        runOnce = true;
    }

}



