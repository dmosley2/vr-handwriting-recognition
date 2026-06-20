using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;

public class PaintBoardScript : MonoBehaviour, IBoard
{
    [SerializeField] private GameObject strokePrefab;
    //[SerializeField] private Transform drawingContainer;
    [SerializeField] private GameObject marker;
    [SerializeField] private TMP_Text sizeText;
    private float brushSize = 0.03f;
    private float increaseRate = 0.005f;
    private Stack<LineRenderer> strokes = new Stack<LineRenderer>();
    private LineRenderer curStroke;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        strokePrefab.GetComponent<LineRenderer>().startWidth = brushSize;
        strokePrefab.GetComponent<LineRenderer>().endWidth = brushSize;
    }

    // Update is called once per frame
    void Update()
    {
        sizeText.text = "Line width: " + brushSize;
    }

    public void StartStroke(Vector3 startPosition) {
        FileManager.Instance.StartTimer();
        GameObject stroke = Instantiate(strokePrefab, null);
        curStroke = stroke.GetComponent<LineRenderer>();
        curStroke.useWorldSpace = true;
        curStroke.positionCount = 1;
        curStroke.SetPosition(0, startPosition);
        strokes.Push(curStroke);
    }

    public void UpdateStroke(Vector3 worldPosition) {
        if (curStroke == null) return;
        //Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        if (curStroke.positionCount > 0)
        {
            if (Vector3.Distance(curStroke.GetPosition(curStroke.positionCount - 1), worldPosition) < 0.001f)
                return;
        }
        curStroke.positionCount++;
        curStroke.SetPosition(curStroke.positionCount - 1, worldPosition);
    }

    public void EndStroke()
    {
        curStroke = null;
    }

    public Stack<LineRenderer> GetStrokes() {
        return strokes;
    }

    public void Delete()
    {
        if (strokes.Count > 0)
        {
            LineRenderer lastStroke = strokes.Pop();
            Destroy(lastStroke.gameObject);
        }
    }

    public void DeleteAll()
    {
        while (strokes.Count > 0)
        {
            LineRenderer lastStroke = strokes.Pop();
            Destroy(lastStroke.gameObject);
        }
    }

    public GameObject GetStrokePrefab() {  return strokePrefab; }

    public void plus()
    {
        brushSize = Mathf.Min(brushSize + increaseRate, 0.05f);
        strokePrefab.GetComponent<LineRenderer>().startWidth = brushSize;
        strokePrefab.GetComponent<LineRenderer>().endWidth = brushSize;
    }

    public void minus()
    {
        brushSize = Mathf.Max(brushSize - increaseRate, 0.005f);
        strokePrefab.GetComponent<LineRenderer>().startWidth = brushSize;
        strokePrefab.GetComponent<LineRenderer>().endWidth = brushSize;
    }

    public float getLineWidth()
    {
        return brushSize;
    }

    //public void takeScreenshot()
    //{
    //    RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
    //    cam.targetTexture = rt;
    //    cam.Render();
    //    Texture2D image = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
    //    RenderTexture.active = rt;
    //    image.ReadPixels(new UnityEngine.Rect(0, 0, imageWidth, imageHeight), 0, 0);
    //    image.Apply(false);
    //    cam.targetTexture = null;
    //    RenderTexture.active = null;
    //    Destroy(rt);
    //    byte[] byteArray = image.EncodeToPNG();
    //    var dirPath = UnityEngine.Application.streamingAssetsPath + "/PreScan";
    //    if (!System.IO.Directory.Exists(dirPath))
    //    {
    //        System.IO.Directory.CreateDirectory(dirPath);
    //    }
    //    savePath = dirPath + "/Alphabet" + saveCounter + ".png";
    //    saveCounter++;
    //    System.IO.File.WriteAllBytes(savePath, byteArray);
    //    UnityEngine.Debug.Log("Screenshot Path: " + savePath);
    //    //rawImage = resultCanvas.GetComponent<RawImage>();
    //    Texture oldTex = rawImage.texture;
    //    rawImage.texture = image;
    //    if (oldTex != null)
    //    {
    //        Destroy(oldTex);
    //    }
    //}

    //public async void scanScreenshot()
    //{
    //    loadCounter = saveCounter - 1;
    //    AssetDatabase.Refresh();
    //    UnityEngine.Debug.Log("scanScreenshot() called");
    //    string basePath = Application.streamingAssetsPath;
    //    string imagePath = savePath;
    //    string scriptPath = Path.Combine(Application.streamingAssetsPath, "script.py");
    //    OCRResult ocrResult;
    //    string jsonResult;

    //    string args = string.Format("\"{0}\" \"{1}\" \"{2}\"", scriptPath, imagePath, basePath);
    //    ProcessStartInfo start = new ProcessStartInfo();
    //    start.FileName = Path.Combine(Application.streamingAssetsPath, "python_embed", "python.exe");
    //    start.Arguments = args;
    //    start.UseShellExecute = false;
    //    //start.RedirectStandardOutput = true;
    //    start.CreateNoWindow = true;

    //    Task<OCRResult> ocrPredict = Task.Run(async () =>
    //    {
    //        OCRResult result = null;
    //        using (Process process = Process.Start(start))
    //        {
    //            process.WaitForExit();
    //            string resultFile = Path.Combine(basePath, "output.json").Replace("\\", "/");
    //            if (File.Exists(resultFile))
    //            {
    //                string jsonResult = File.ReadAllText(resultFile);
    //                result = JsonUtility.FromJson<OCRResult>(jsonResult);
    //                File.Delete(resultFile);
    //            }
    //            else
    //            {
    //                UnityEngine.Debug.LogError("Task Failed: Result file not found.");
    //            }
    //        }

    //        if (result != null)
    //        {
    //            return result;
    //        }
    //        return null;
    //    });

    //    ocrResult = await ocrPredict;
    //    UnityEngine.Debug.Log(ocrResult.text);
    //    UnityEngine.Debug.Log(ocrResult.confidence);
    //    editCanvas(ocrResult);

    //    //Old scanScreenshot before TrOCR implementation
    //    //if (model == null)
    //    //{
    //    //    UnityEngine.Debug.LogError("model is NULL! Make sure it is assigned in the Inspector.");
    //    //    return;
    //    //}

    //    //if (rawImage == null)
    //    //{
    //    //    UnityEngine.Debug.LogError("RawImage component is missing! Make sure this GameObject has a RawImage.");
    //    //    return;
    //    //}


    //    //UnityEngine.Texture2D texture;
    //    ////if (savePath.IsUnityNull()) return;
    //    //string loadPath = savePath;
    //    //if (!System.IO.File.Exists(loadPath))
    //    //{
    //    //    UnityEngine.Debug.Log("File Path doesn't exist: " + loadPath);
    //    //    return;
    //    //}
    //    //byte[] fileData = System.IO.File.ReadAllBytes(loadPath);
    //    //texture = new UnityEngine.Texture2D(2, 2);
    //    //texture.LoadImage(fileData);
    //    //texture.Apply();
    //    //UnityEngine.Debug.Log($"Loaded texture size: {texture.width}x{texture.height}");
    //    //if (texture.width == 2 && texture.height == 2)
    //    //{
    //    //    UnityEngine.Debug.LogError("Image failed to load or is corrupt!");
    //    //}
    //    //// some constants for drawing
    //    //const int textPadding = 2;
    //    //const HersheyFonts textFontFace = HersheyFonts.HersheyPlain;
    //    //double textFontScale = System.Math.Max(texture.width, texture.height) / 512.0;
    //    //Scalar boxColor = Scalar.DeepPink;
    //    //Scalar textColor = Scalar.White;

    //    //// load alphabet
    //    //AlphabetOCR alphabet = new AlphabetOCR(model.bytes);

    //    //// scan image
    //    //var image = OpenCvSharp.Unity.TextureToMat(texture);
    //    //UnityEngine.Debug.Log($"Converted Mat size: {image.Width}x{image.Height}");
    //    //IList<AlphabetOCR.RecognizedLetter> letters = alphabet.ProcessImage(image);
    //    //foreach (var letter in letters)
    //    //{
    //    //    int line;
    //    //    var bounds = Cv2.BoundingRect(letter.Rect);

    //    //    // text box.
    //    //    var textData = string.Format("{0}: {1}%", letter.Data, System.Math.Round(letter.Confidence * 100));
    //    //    var textSize = Cv2.GetTextSize(textData, textFontFace, textFontScale, 1, out line);
    //    //    var textBox = new OpenCvSharp.Rect(
    //    //        bounds.X + (bounds.Width - textSize.Width) / 2 - textPadding,
    //    //        bounds.Bottom,
    //    //        textSize.Width + textPadding * 2,
    //    //        textSize.Height + textPadding * 2
    //    //    );

    //    //    // draw shape
    //    //    image.Rectangle(bounds, boxColor, 2);
    //    //    image.Rectangle(textBox, boxColor, -1);
    //    //    image.PutText(textData, textBox.TopLeft + new Point(textPadding, textPadding + textSize.Height), textFontFace, textFontScale, textColor, (int)(textFontScale + 0.5));
    //    //}

    //    //// result
    //    //texture = OpenCvSharp.Unity.MatToTexture(image);
    //    //byte[] byteArray = texture.EncodeToPNG();
    //    //System.IO.File.WriteAllBytes(UnityEngine.Application.dataPath + "/StreamingAssets/Results/result" + loadCounter + ".png", byteArray);



    //    //// output
    //    ////rawImage = resultCanvas.GetComponent<RawImage>();
    //    //rawImage.texture = texture;
    //    //rawImage.enabled = false;
    //    //rawImage.enabled = true;

    //    //var transform = resultCanvas.GetComponent<UnityEngine.RectTransform>();
    //    //transform.sizeDelta = new UnityEngine.Vector2(1024, 1024);
    //}

    //public void editCanvas(OCRResult result)
    //{
    //    if (result != null)
    //    {
    //        float conf = result.confidence;
    //        if (conf >= 0.8f)
    //        {
    //            resultText.color = Color.green;
    //        }
    //        else if (conf < 0.8f && conf >= 0.6)
    //        {
    //            resultText.color = Color.yellow;
    //        }
    //        else
    //        {
    //            resultText.color = Color.red;
    //        }
    //        resultText.text = result.text;
    //    }
    //    else
    //    {
    //        resultText.color = Color.white;
    //        resultText.text = "Error :(";
    //    }
    //}
}
