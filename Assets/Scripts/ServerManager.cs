using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    private Process serverProcess;
    private string serverUrl = "http://127.0.0.1:5000/predict";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        InitializeServer();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitializeServer()
    {
        string basePath = Application.streamingAssetsPath;
        string scriptPath = Path.Combine(Application.streamingAssetsPath, "script.py");
        serverProcess = new Process();
        serverProcess.StartInfo.FileName = Path.Combine(basePath, "python_embed", "python.exe");
        serverProcess.StartInfo.Arguments = $"\"{scriptPath}\"";
        serverProcess.StartInfo.WorkingDirectory = basePath;
        //serverProcess.StartInfo.RedirectStandardOutput = false;
        serverProcess.StartInfo.UseShellExecute = true;
        serverProcess.StartInfo.CreateNoWindow = true;
        //serverProcess.StartInfo.RedirectStandardError = false;
        serverProcess.Start();
    }

    private void OnApplicationQuit()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            serverProcess.Kill();
            serverProcess.Dispose();
        }
    }

    public async Task<string> PostImage(byte[] image)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", image, "screenshot.png", "image/png");
        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            var op = request.SendWebRequest();
            await op;
            if (request.result == UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(request.downloadHandler.text);
                return request.downloadHandler.text;
            }
            else
            {
                UnityEngine.Debug.LogError("Error posting image: " + request.error);
                UnityEngine.Debug.LogError("Response code: " + request.downloadHandler.text);
                return null;
            }
        }
    }
}
