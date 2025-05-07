using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using TMPro;
using UnityEngine.UI;

public class NewtonsMethodParameterInput : MonoBehaviour
{
    private TMP_InputField field;
    public int index;
    private string lastVal;
    // Start is called before the first frame update
    void Start()
    {
        field = GetComponent<TMP_InputField>();
        field.text = FractalGenerator.polynomialCoefficients[index].ToString();
    }
    void Update()

    {
        if(field.text!=lastVal)
        {
        Change();
        lastVal=field.text;
        }
    }
    // Update is called once per frame

    public void Change()
    {
                string val = field.text;
        if(val.Length==0||val=="-") val = "0";
        FractalGenerator.polynomialCoefficients[index]=int.Parse(val);
        FractalGenerator.Regenerate();
    }
}
