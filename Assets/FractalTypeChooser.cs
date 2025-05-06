using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FractalTypeChooser : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        FractalGenerator.fractalType = (FractalGenerator.FractalType)GetComponent<TMP_Dropdown>().value;
    }
    public void Change()
    {
        Update();
        FractalGenerator.Reset();
    }
}
