using System.Collections.Generic;
using UnityEngine;

public class TipScript : MonoBehaviour
{
    public Transform tip;
    private AnimateHandController aController = null;
    private Color32 currentColor = Color.black;
    private int drawCount = 0;
    private bool isDrawing = false;
    public float increaseRate = 0.05f;
    public float brushSize = 0.001f;
    private PaintBoardScript paintboard;
    private GameObject stroke;


    void Start()
    {
        if (tip != null && tip.parent != null)
        {
            tip.parent.GetComponent<Renderer>().material.color = currentColor;
        }
    }

    private void Update()
    {
        //brushStroke.widthMultiplier = brushSize;


        // Grab and start painting
        if (aController == null || paintboard == null)
        {
            if (isDrawing) StopDrawing();
            return;
        }

        bool isInputPress = aController.GetGripValue() > 0.1f;

        Vector3 localPos = paintboard.transform.InverseTransformPoint(tip.position);
        localPos.y = .9f;
        Vector3 constrainedPoint = paintboard.transform.TransformPoint(localPos);

        if (isInputPress)
        {
            if (!isDrawing) StartDrawing(constrainedPoint, currentColor);
            paintboard.UpdateStroke(constrainedPoint);
        }
        else if (isDrawing)
        {
            StopDrawing();
        }

    }

    private void StartDrawing(Vector3 start, Color color)
    {
        isDrawing = true;
        paintboard.StartStroke(start);
    }

    private void StopDrawing()
    {
        isDrawing = false;
        if (paintboard != null)
        {
            paintboard.EndStroke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("VRHand"))
        {
            if (aController != null) return;
            AnimateHandController hand = other.gameObject.GetComponentInChildren<AnimateHandController>();
            if (hand != aController)
            {
                aController = hand;
            }
        }
        else if (other.gameObject.CompareTag("DrawBoard"))
        {
            PaintBoardScript board = other.GetComponent<PaintBoardScript>();
            if (board != null) paintboard = board;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("VRHand"))
        {
            bool isCurrent = other.gameObject == aController.gameObject;
            if (isCurrent)
            {
                if (isDrawing) StopDrawing();
                aController = null;
            }
        }
        else if (other.gameObject.CompareTag("DrawBoard"))
        {
            if (isDrawing) StopDrawing();
            paintboard = null;
        }
        else
        { return; }
    }
    public void SetColor(Color color)
    {
        currentColor = color;
        tip.parent.GetComponent<Renderer>().material.color = currentColor;
    }

    public Color32 GetColor()
    {
        return currentColor;
    }

}
