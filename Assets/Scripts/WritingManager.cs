using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using SaadKhawaja.InstantScreenshot;

public class WritingManager : MonoBehaviour
{
    //public GameObject pen;
    public GameObject penBoard;
    public GameObject rayBoard;
    public GameObject hapticBoard;
    public InputActionReference primaryButtonAction;
    public InputActionReference secondaryButtonAction;
    public GameObject raycastEnv;
    public GameObject bedroomEnv;
    public GameObject hapticEnv;
    public GameObject leftHand;
    public GameObject rightHand;
    public HapticPlugin hapticPlugin;
    private GameObject currentBoardObject;
    private IBoard currentBoardScript;
    private int currentBoard; // 0 for penBoard, 1 for rayBoard, 2 for hapticBoard
    private int numEnvironments;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBoard = 0;
        if (hapticPlugin == null)
        {
            numEnvironments = 2;
        }
        else
        {
            numEnvironments = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (primaryButtonAction.action.WasPressedThisFrame())
        {
            changeInterface();
        }

        if(secondaryButtonAction.action.WasPressedThisFrame())
        {
            InstantScreenshot.TakeScreenshot("C:\\Users\\domin\\AppData\\LocalLow\\DefaultCompany\\Drawing");
        }

        if (currentBoard % numEnvironments == 0)
        {
            bedroomEnv.SetActive(true);
            raycastEnv.SetActive(false);
            hapticEnv.SetActive(false);
            leftHand.SetActive(true);
            rightHand.SetActive(true);
            currentBoardObject = penBoard;
        }
        else if (currentBoard % numEnvironments == 1)
        {
            bedroomEnv.SetActive(false);
            raycastEnv.SetActive(true);
            hapticEnv.SetActive(false);
            leftHand.SetActive(true);
            rightHand.SetActive(true);
            currentBoardObject = rayBoard;
        }
        else if (currentBoard % numEnvironments == 2)
        {
            bedroomEnv.SetActive(false);
            raycastEnv.SetActive(false);
            hapticEnv.SetActive(true);
            leftHand.SetActive(false);
            rightHand.SetActive(false);
            currentBoardObject = hapticBoard;
        }

    }

    public int GetCurrentBoard()
    {
        return currentBoard % numEnvironments;
    }

    public void changeInterface()
    {
        currentBoard++;
    }

    public IBoard GetCurrentBoardObject()
    {
        return currentBoardObject.GetComponent<IBoard>();
    }

    //public GameObject GetPenBoard()
    //{
    //    return penBoard;
    //}

    //public GameObject GetRayBoard()
    //{
    //    return rayBoard;
    //}

}
