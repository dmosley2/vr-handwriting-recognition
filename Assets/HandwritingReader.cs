using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TMPro;

//[System.Serializable]
//public class OCRResult
//{
//    public string text;
//    public float confidence;
//}

public class HandwritingReader : MonoBehaviour
{
    public string basePath;
    public RawImage resultImage;
    public TMP_Text resultText;

    public OCRResult runModel(string imagePath, string scriptPath, string resultPath)
    {
        OCRResult ocrResult = null;
        string args = string.Format("\"{0}\" \"{1}\" \"{2}\"", scriptPath, imagePath, resultPath);
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = Path.Combine(basePath, "python_embed", "python.exe");
        start.Arguments = args;
        start.UseShellExecute = false;
        //start.RedirectStandardOutput = true;
        //start.RedirectStandardError = true;
        start.CreateNoWindow = false;
        start.UseShellExecute = true;
        start.WorkingDirectory = basePath;

        if (!System.IO.File.Exists(start.FileName))
        {
            UnityEngine.Debug.Log("The Python EXE is missing at: " + start.FileName);
        }

        using (Process process = Process.Start(start))
        {
            process.WaitForExit();
            string resultFile = Path.Combine(basePath, "output.json").Replace("\\", "/");
            UnityEngine.Debug.Log("UNITY LOOKING AT: " + Path.GetFullPath(resultFile));
            if (File.Exists(resultFile))
            {
                string jsonResult = File.ReadAllText(resultFile);
                ocrResult = JsonUtility.FromJson<OCRResult>(jsonResult);
                //File.Delete(resultFile);
            }
            else
            {
                string dir = Path.GetDirectoryName(resultFile);
                if (Directory.Exists(dir))
                {
                    string[] files = Directory.GetFiles(dir);
                    UnityEngine.Debug.Log("Files actually in that folder: " + string.Join(", ", files));
                }
                UnityEngine.Debug.LogError("Task Failed: Result file not found.");
            }
            UnityEngine.Debug.Log("Python script executed successfully.");
            //UnityEngine.Debug.Log(result);
            //UnityEngine.Debug.Log("TrOCR says: " + ocrResult.text);
            //UnityEngine.Debug.Log("Confidence: " + ocrResult.confidence.ToString("P1"));
        }

        if (ocrResult == null)
        {
            UnityEngine.Debug.Log("OCR Result is empty");
            return null;
        }
        return ocrResult;
    }

    async void Start()
    {
        basePath = Application.streamingAssetsPath;
        string scriptPath = Path.Combine(basePath, "script.py").Replace("\\", "/");
        string imagePath = Path.Combine(basePath, "HelloWorld.png").Replace("\\", "/");
        UnityEngine.Debug.Log(imagePath);
        byte[] file = File.ReadAllBytes(imagePath);
        Texture2D image = new Texture2D(2, 2);
        ImageConversion.LoadImage(image, file);
        resultImage.texture = image;
        Task<OCRResult> ocrPredict = Task.Run(() =>
        {
            return runModel(imagePath, scriptPath, basePath);
        });
        OCRResult ocrResult = await ocrPredict;
        if (ocrResult != null)
        {
            float conf = ocrResult.confidence;
            if (conf >= 0.8f)
            {
                resultText.color = Color.green;
            }
            else if (conf < 0.8f && conf >= 0.6)
            {
                resultText.color = Color.yellow;
            }
            else
            {
                resultText.color = Color.red;
            }
            resultText.text = ocrResult.text;
        }
        else
        {
            resultText.color = Color.white;
            resultText.text = "Error :(";
        }
    }

}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
