using UnityEngine;

public class ScreenOffDisplay : MonoBehaviour
{
	void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }	
}
