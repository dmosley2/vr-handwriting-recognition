using UnityEngine;

public class plus : MonoBehaviour
{
    public Transform button;
    public Transform pressedPosition;
    private Vector3 initialPosition;

    //publi

    public TipScript paintBrush;

    void Start()
    {
        initialPosition = button.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("EnterUndo");
        if (!other.gameObject.CompareTag("VRHand")) return;
        //Debug.Log("EnterUndoOOOOOOOO");

        button.position = pressedPosition.position;

        //new WaitForSeconds(3);
        //Invoke("Delay", 2);


        //button.position = initialPosition;

    }

    //private void Delay()
    //{
    //    Debug.Log("Delaying!");
    //}

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("ExitUndo");
        if (!other.gameObject.CompareTag("VRHand")) return;
        //Debug.Log("ExitUndoOOOOOOOOOOOOOOO");

        button.position = initialPosition;
    }
}