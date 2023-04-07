using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCharacter : MonoBehaviour
{
    //Door variable
    public DoorScript theDoor;
    //Our treasure variable
    public GameObject theTreasure;
    //Test variable
    public GameObject test;
    bool executingBehavior = false;
    MyTask myCurrentTask;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!executingBehavior)
            {
                executingBehavior = true;
                myCurrentTask = BuildTask_GetTreasue();

                MyEventBus.StartListening(myCurrentTask.TaskFinished, OnTaskFinished);
                myCurrentTask.run();
            }
        }
    }

    void OnTaskFinished()
    {
        MyEventBus.StopListening(myCurrentTask.TaskFinished, OnTaskFinished);
        //Debug.Log("Behavior complete! Success = " + myCurrentTask.succeeded);
        executingBehavior = false;
    }
    
    MyTask BuildTask_GetTreasue()
    {
        // create our behavior tree based on Millington pg. 344
        // building from the bottom up
        List<MyTask> taskList = new List<MyTask>();

        // if door isn't locked, open it
        MyTask isDoorNotLocked = new IsFalse(theDoor.isLocked);
        MyTask waitABeat = new Wait(0.5f);
        MyTask openDoor = new OpenDoor(theDoor);
        taskList.Add(isDoorNotLocked);
        taskList.Add(waitABeat);
        taskList.Add(openDoor);
        Sequence openUnlockedDoor = new Sequence(taskList);

        // barge a closed door
        taskList = new List<MyTask>();
        MyTask isDoorClosed = new IsTrue(theDoor.isClosed);
        MyTask hulkOut = new HulkOut(this.gameObject);
        MyTask bargeDoor = new BargeDoor(theDoor.transform.GetChild(0).GetComponent<Rigidbody>());
        taskList.Add(isDoorClosed);
        taskList.Add(waitABeat);
        taskList.Add(hulkOut);
        taskList.Add(waitABeat);
        taskList.Add(bargeDoor);
        Sequence bargeClosedDoor = new Sequence(taskList);

        // open a closed door, one way or another
        taskList = new List<MyTask>();
        taskList.Add(openUnlockedDoor);
        taskList.Add(bargeClosedDoor);
        Selector openTheDoor = new Selector(taskList);

        // get the treasure when the door is closed
        taskList = new List<MyTask>();
        MyTask moveToDoor = new MoveKinematicToObject(this.GetComponent<Kinematic>(), theDoor.gameObject);
        MyTask moveToTreasure = new MoveKinematicToObject(this.GetComponent<Kinematic>(), theTreasure.gameObject);
        taskList.Add(moveToDoor);
        taskList.Add(waitABeat);
        taskList.Add(openTheDoor); // one way or another
        taskList.Add(waitABeat);
        taskList.Add(moveToTreasure);
        Sequence getTreasureBehindClosedDoor = new Sequence(taskList);

        // get the treasure when the door is open 
        taskList = new List<MyTask>();
        MyTask isDoorOpen = new IsFalse(theDoor.isClosed);
        taskList.Add(isDoorOpen);
        taskList.Add(moveToTreasure);
        Sequence getTreasureBehindOpenDoor = new Sequence(taskList);

        // get the treasure, one way or another
        taskList = new List<MyTask>();
        taskList.Add(getTreasureBehindOpenDoor);
        taskList.Add(getTreasureBehindClosedDoor);
        Selector getTreasure = new Selector(taskList);

        return getTreasure;
    }
}
