using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewtonsMethodCoefficientsTab : MonoBehaviour
{
    public GameObject tab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(FractalGenerator.fractalType==FractalGenerator.FractalType.Newtonian)
        {
            tab.SetActive(true);
        }
        else
        {
            tab.SetActive(false);
        }
    }
}
