using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MyTask
{
    public abstract void run();
    public bool succeeded;

    protected int eventId;
    const string EVENT_NAME_PREFIX = "FinishedTask";
    public string TaskFinished
    {
        get
        {
            return EVENT_NAME_PREFIX + eventId;
        }
    }
    public MyTask()
    {
        eventId = MyEventBus.GetEventID();
    }
}

public class IsTrue : MyTask
{
    bool varToTest;

    public IsTrue(bool someBool)
    {
        varToTest = someBool;
        
    }

    public override void run()
    {
        succeeded = varToTest;
        MyEventBus.TriggerEvent(TaskFinished);
    }
}


public class IsFalse : MyTask
{
    bool varToTest;

    public IsFalse(bool someBool)
    {
        varToTest = someBool;
    }

    public override void run()
    {
        succeeded = !varToTest;
        MyEventBus.TriggerEvent(TaskFinished);
    }
}

public class OpenDoor : MyTask
{
    DoorScript mDoor;

    public OpenDoor(DoorScript someDoor)
    {
        mDoor = someDoor;
    }

    public override void run()
    {
        succeeded = mDoor.Open();
        MyEventBus.TriggerEvent(TaskFinished);
    }
}

public class BargeDoor : MyTask
{
    Rigidbody mDoor;

    public BargeDoor(Rigidbody someDoor)
    {
        mDoor = someDoor;
    }

    public override void run()
    {
        mDoor.AddForce(-10f, 0, 0, ForceMode.VelocityChange);
        succeeded = true;
        MyEventBus.TriggerEvent(TaskFinished);
    }
}

public class HulkOut : MyTask
{
    GameObject mEntity;

    public HulkOut(GameObject someEntity)
    {
        mEntity = someEntity;
    }

    public override void run()
    {
        //Debug.Log("hulking out");
        mEntity.transform.localScale *= 2;
        mEntity.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        succeeded = true;
        MyEventBus.TriggerEvent(TaskFinished);
    }
}

/*
//Broken code - "Will break if used before barge, hulk out, or open door
//But it works before moveto or an isfalse
public class Pause : MyTask
{
    Timer myTimer;

    public Pause(float time)
    {
        myTimer = new Timer(time * 1000f); // miliseconds!
        myTimer.AutoReset = false;
        myTimer.Elapsed += OnTimeElapsed;
    }

    public override void run()
    {
        //myTimer.Enabled = true;
        myTimer.Start();
    }

    void OnTimeElapsed(object source, ElapsedEventArgs e)
    {
        //myTimer.Enabled = false;
        myTimer.Stop();
        Debug.Log("Pause time elapsed.");
        succeeded = true;
        MyEventBus.TriggerEvent("FinishedTask" + eventId);
    }
}
*/

public class Wait : MyTask
{
    float mTimeToWait;

    public Wait(float time)
    {
        mTimeToWait = time;
    }

    public override void run()
    {
        succeeded = true;
        MyEventBus.ScheduleTrigger(TaskFinished, mTimeToWait);
    }
}

public class MoveKinematicToObject : MyTask
{
    Arriver mMover;
    GameObject mTarget;

    public MoveKinematicToObject(Kinematic mover, GameObject target)
    {
        mMover = mover as Arriver;
        mTarget = target;
    }

    public override void run()
    {
        //Debug.Log("Moving to target position: " + mTarget);
        mMover.OnArrived += MoverArrived;
        mMover.myTarget = mTarget;
    }

    public void MoverArrived()
    {
        //Debug.Log("arrived at " + mTarget);
        mMover.OnArrived -= MoverArrived;
        succeeded = true;
        MyEventBus.TriggerEvent(TaskFinished);
    }
}

public class Sequence : MyTask
{
    List<MyTask> children;
    MyTask currentTask;
    int currentTaskIndex = 0;

    public Sequence(List<MyTask> taskList)
    {
        children = taskList;
    }

    // Sequence wants all tasks to succeed
    // try all tasks in order
    // stop and return false on the first task that fails
    // return true if all tasks succeed
    public override void run()
    {
        //Debug.Log("sequence running child task #" + currentTaskIndex);
        currentTask = children[currentTaskIndex];
        MyEventBus.StartListening(currentTask.TaskFinished, OnChildTaskFinished);
        currentTask.run();
    }

    void OnChildTaskFinished()
    {
        //Debug.Log("Behavior complete! Success = " + currentTask.succeeded);
        if (currentTask.succeeded)
        {
            MyEventBus.StopListening(currentTask.TaskFinished, OnChildTaskFinished);
            currentTaskIndex++;
            if (currentTaskIndex < children.Count)
            {
                this.run();
            }
            else
            {
                // we've reached the end of our children and all have succeeded!
                succeeded = true;
                MyEventBus.TriggerEvent(TaskFinished);
            }

        }
        else
        {
            // sequence needs all children to succeed
            // a child task failed, so we're done
            succeeded = false;
            MyEventBus.TriggerEvent(TaskFinished);
        }
    }
}

public class Selector : MyTask
{
    List<MyTask> children;
    MyTask currentTask;
    int currentTaskIndex = 0;

    public Selector(List<MyTask> taskList)
    {
        children = taskList;
    }

    // Selector wants only the first task that succeeds
    // try all tasks in order
    // stop and return true on the first task that succeeds
    // return false if all tasks fail
    public override void run()
    {
        //Debug.Log("selector running child task #" + currentTaskIndex);
        currentTask = children[currentTaskIndex];
        MyEventBus.StartListening(currentTask.TaskFinished, OnChildTaskFinished);
        currentTask.run();
    }

    void OnChildTaskFinished()
    {
        //Debug.Log("Behavior complete! Success = " + currentTask.succeeded);
        if (currentTask.succeeded)
        {
            succeeded = true;
            MyEventBus.TriggerEvent(TaskFinished);
        }
        else
        {
            MyEventBus.StopListening(currentTask.TaskFinished, OnChildTaskFinished);
            currentTaskIndex++;
            if (currentTaskIndex < children.Count)
            {
                this.run();
            }
            else
            {
                // we've reached the end of our children and they have not succeeded :(
                succeeded = false;
                MyEventBus.TriggerEvent(TaskFinished);
            }
        }
    }
}

