using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using TMPro;
using UnityEngine.UI;

public class NewtonsMethodParameterInput : MonoBehaviour
{
    private TMP_InputField field;
    public int index;
    // Start is called before the first frame update
    void Start()
    {
        field = GetComponent<TMP_InputField>();
        field.text = FractalGenerator.polynomialCoefficients[index].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        FractalGenerator.polynomialCoefficients[index]=int.Parse(field.text);
    }
}
