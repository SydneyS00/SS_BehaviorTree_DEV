using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    //Variables to see if our door is closed or open
    public bool isClosed = false;
    public bool isLocked = false;

    Vector3 closedRotation = new Vector3(0, 0, 0);
    Vector3 openRotation = new Vector3(0, -135, 0);

    
    void Start()
    {
        if (isClosed)
        {
            transform.eulerAngles = closedRotation;
        }
        else
        {
            transform.eulerAngles = openRotation;
        }
    }

    //If the door is closed AND is not locked then open the door
    public bool Open()
    {
        if (isClosed && !isLocked)
        {
            //Debug.Log("door is now open");
            isClosed = false;
            transform.eulerAngles = openRotation;
            return true;
        }

        //Debug.Log("door was either locked or already open");
        return false;
    }

    //If the door is not closed then close the door 
    public bool Close()
    {
        if (!isClosed)
        {
            //Debug.Log("door is now closed");
            transform.eulerAngles = closedRotation;
            isClosed = true;
        }
        return true;
    }
}
