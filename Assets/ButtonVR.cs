using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Demo;
using Unity.VisualScripting;

public class ButtonVR : MonoBehaviour
{
    public Transform button;
    public Transform pressedPosition;
    private Vector3 initialPosition;
    public UnityEvent onRelease;

    void DeleteFolderContents(string path) {
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
        foreach (System.IO.FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = button.position;
        DeleteFolderContents(Application.dataPath + "/StreamingAssets/PreScan");
        DeleteFolderContents(Application.dataPath + "/StreamingAssets/Results");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("EnterUndo");
        if (!other.gameObject.CompareTag("VRHand")) return;
        button.position = pressedPosition.position;

    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("ExitUndo");
        if (!other.gameObject.CompareTag("VRHand")) return;
        onRelease.Invoke();
        button.position = initialPosition;
    }
}
