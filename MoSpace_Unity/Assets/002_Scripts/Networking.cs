using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Networking : MonoBehaviour
{
    [System.Serializable]
    public class Position
    {
        public float x,y,z;
    }

    string serverUrl = "localhost:8080";

    // Use this for initialization
    void Start()
    {
        StartCoroutine(Get());
        print("started");
    }

    IEnumerator Get()
    {
        WWW www;

        string url = serverUrl + "/getPos";
        www = new WWW(url);

        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Get succeeded!");
            Debug.Log(www.text);
            Position pos = JsonUtility.FromJson<Position>(www.text);
            Debug.Log(pos.x);
        }
        Debug.Log("networking debug log");
    }

    public void setPos(float x, float y, float z)
    {
        StartCoroutine(Set(x,y,z));
    }

    IEnumerator Set(float x, float y, float z)
    {
        
        WWW www;
        Dictionary<string, string> postHeader = new Dictionary<string, string>();
        postHeader.Add("Content-Type", "application/json");

        Position pos = new Position();
        pos.x = x;
        pos.y = y;
        pos.z = z;

        string posStr = JsonUtility.ToJson(pos);

        var formData = System.Text.Encoding.UTF8.GetBytes(posStr);
        string url = serverUrl + "/setPos";

        www = new WWW(url, formData, postHeader);

        yield return www;

        if (www.error != "")
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Set succeeded!");
            Debug.Log(www.text);
        }
    }
}
