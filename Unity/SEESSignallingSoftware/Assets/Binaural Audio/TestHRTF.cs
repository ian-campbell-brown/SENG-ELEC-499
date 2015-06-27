using UnityEngine;
using System.Collections;

public class TestHRTF : MonoBehaviour
{
    public Transform _Object = null;

    private int _Ticks = 0;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        _Ticks++;

        if (_Ticks == 400)
        {
            Debug.Log("report");

            _Object.position += Vector3.right;
        }
	}
}
