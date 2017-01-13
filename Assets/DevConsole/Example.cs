using UnityEngine;
using DevConsole;

public class Example : MonoBehaviour {
    void Start()
    {
        Console.Log("Test");
        // Console.AddCommand(new CommandBase());
    }

    [Command]
    void Test() {
        Console.Log("Just do it");
    }

    [Command]
    static void TimeScale(float value) {
        Time.timeScale = value;
        Console.Log("Change successful", Color.green);
    }

    [Command]
    static void ShowTime() {
        Console.Log(Time.time.ToString());
    }

    [Command]
    static void SetGravity(Vector3 value) {
        Physics.gravity = value;
    }    
}