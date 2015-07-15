using System;
using UnityEngine;

// A singleton class to allow easy access to the KinectManager object and its components.
public class KinectSingleton : MonoBehaviour
{
    public static KinectSingleton Instance;

    public KinectSensor KinectSensor { get { return this.GetComponent<KinectSensor>(); } }
    public DepthWrapper DepthWrapper { get { return this.GetComponent<DepthWrapper>(); } }
    public SkeletonWrapper SkeletonWrapper { get { return this.GetComponent<SkeletonWrapper>(); } }

    void Start()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Two instances of signleton exist in this scene.", this);
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {

    }
}
