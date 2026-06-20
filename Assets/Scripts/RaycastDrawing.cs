using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System;
using TMPro;

public class RaycastDrawing : MonoBehaviour, IBoard
{
    public Transform rightController;
    public GameObject whiteboard;
    public LineRenderer lineRenderer;
    public InputActionReference triggerActionValue;
    public int size = 7;
    public TMP_Text sizeText;
    private Texture2D texture;
    private Vector2 lastUV;
    private bool isDrawing = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Material whiteboardRenderer = whiteboard.GetComponent<Renderer>().material;
        if (whiteboardRenderer.mainTexture != null)
        {
            texture = whiteboardRenderer.mainTexture as Texture2D;
        }
        else
        {
            // Create a new texture if the whiteboard doesn't have one and make it white
            texture = new Texture2D(512, 512);
            whiteboardRenderer.mainTexture = texture;
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, Color.clear);
        }
            }
            texture.Apply();
            //Set the whiteboard's material to use the new texture
            whiteboardRenderer.mainTexture = texture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        sizeText.text = "Line width: " + size;
        float triggerValue = triggerActionValue.action.ReadValue<float>();
        Vector3 endPosition = rightController.position + (rightController.forward * 10f);
        Ray ray = new Ray(rightController.position, rightController.forward);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            endPosition = hit.point;
            if (hit.collider != null && hit.collider.gameObject == whiteboard)
            {
                if (triggerValue > 0.1f)
                {
                    if (isDrawing)
                    {
                        DrawLine(lastUV, hit.textureCoord);
                        lastUV = hit.textureCoord;
                    }
                    else
                    {
                        FileManager.Instance.StartTimer();
                        isDrawing = true;
                        lastUV = hit.textureCoord;
                    }
                }
                else
                {
                    isDrawing = false;
                }
            }
            else
            {
                isDrawing = false;
            }
        }
        lineRenderer.SetPosition(0, rightController.position);
        lineRenderer.SetPosition(1, endPosition);
    }

    void DrawTexture(int x, int y)
    {
        for (int i = -(size/2); i <= (size/2); i++) { 
            for(int j = -(size/2); j <= (size/2); j++) {
                int drawX = Mathf.Clamp(x + i, 0, texture.width - 1);
                int drawY = Mathf.Clamp(y + j, 0, texture.height - 1);
                Color color = Color.black;
                texture.SetPixel(drawX, drawY, color);
            }
        }
    }

    void DrawLine(Vector2 startUV, Vector2 endUV)
    {
        float distance = Vector2.Distance(startUV, endUV);
        int steps = Mathf.CeilToInt(distance * 100); // Adjust the multiplier for smoother lines
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 lerpedUV = Vector2.Lerp(startUV, endUV, t);

            int x = (int)(lerpedUV.x * texture.width);
            int y = (int)(lerpedUV.y * texture.height);

            DrawTexture(x, y);
        }
        texture.Apply();
    }

    public void EraseBoard()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
        texture.Apply();
    }

    public void increaseSize()
    {
        size = Mathf.Min(size + 1, 10);
    }

    public void decreaseSize()
    {
        size = Mathf.Max(size - 1, 1);
    }

    public float getLineWidth()
    {
        return size;
    }
}
