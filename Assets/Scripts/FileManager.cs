using UnityEngine;
using System.IO;
using System;
/***
 * File Manager for the user study. Creates a folder for the user study. Inside that folder, creates a folder for each participant, i.e. Participant1, Participant2, etc. 
 * Each participant folder contains a folder for each of the different handwriting techniques, Collision-based, Raycast, Haptic, and Hand Tracking.
 * Inside each of the handwriting technique folders, create a folder for each of the different tasks.
 * The folder will include the following information:
 * - A json file that contains the following information:
 *      - The time taken to write (from the moment the user starts writing to the moment the user presses the scan screenshot button) 
 *      - What the model predicited the user wrote (the output of the model)
 *      - The confidence of the model in its prediction (the output of the model)
 * - A screenshot of the user's handwriting (the screenshot taken when the user presses the scan screenshot button)
 * - Cuts made to the photo by the Python script if cuts needed to be made.
 ***/
[System.Serializable]
public class HandwritingData
{
    public string result;
    public float confidence;
    public float writingTotalTime;
    public float modelTime;
    public float lineWidth;
}

public class FileManager : MonoBehaviour
{
    public static FileManager Instance { get; private set; }
    public WritingManager writingManager;
    private int curBoard;
    private string[] handwritingTechniques = new string[] { "CollisionBased", "Raycast", "Haptic"};
    private string basePath;
    private string currentParticipantFolderPath;
    private string currentTaskFolderPath;
    private byte[] imageBytes;
    private float writingStartTime;
    private float writingTotalTime;

    private void Awake() { 
        basePath = Path.Combine(Application.persistentDataPath, "UserStudyData");
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        int participantNumber = Directory.GetDirectories(basePath).Length + 1;
        string participantFolderPath = Path.Combine(basePath, "Participant" + participantNumber + "_[" + DateTime.Now.ToString("MM_dd") + "]");
        currentParticipantFolderPath = participantFolderPath;
        CreateParticipantFolder(participantFolderPath);
    }

    // Update is called once per frame
    void Update()
    {
        curBoard = writingManager.GetCurrentBoard();
    }

    private void CreateParticipantFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            foreach(string technique in handwritingTechniques)
            {
                string techniqueFolderPath = Path.Combine(path, technique);
                Directory.CreateDirectory(techniqueFolderPath);
            }
        }
    }

    public void CreateTaskFolder()
    {
        string technique = handwritingTechniques[curBoard]; //change back to curBoard when writingManager is added back in
        string taskFolderPath = Path.Combine(currentParticipantFolderPath, technique, "Task" + (Directory.GetDirectories(Path.Combine(currentParticipantFolderPath, technique)).Length + 1));
        Directory.CreateDirectory(taskFolderPath);
        currentTaskFolderPath = taskFolderPath;
    }

    public void StartTimer()
    {
        writingStartTime = Time.time;
    }

    public void StopTimer()
    {
        writingTotalTime = Time.time - writingStartTime;
    }

    public void CreateJsonFile(string result, float confidence) { 
        HandwritingData data = new HandwritingData();
        data.result = result;
        data.confidence = confidence;
        data.writingTotalTime = writingTotalTime;
        data.lineWidth = writingManager.GetCurrentBoardObject().getLineWidth();
        string json = JsonUtility.ToJson(data, true);
        string jsonFilePath = Path.Combine(currentTaskFolderPath, "data.json");
        File.WriteAllText(jsonFilePath, json);
        Debug.Log("Json file created at: " + jsonFilePath);
    }

    public void SaveImage(byte[] bytes) { 
        System.IO.File.WriteAllBytes(Path.Combine(currentTaskFolderPath, "handwriting.png"), bytes);
    }


}
