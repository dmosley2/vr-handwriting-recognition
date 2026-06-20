using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    //public Transform button;
    public TipScript paintBrush;

  

    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        //if (!other.gameObject.CompareTag("VRHand")) return;
        Debug.Log("SetColor!");
        
        paintBrush.SetColor(GetComponent<Renderer>().material.color);
    }



  
}
