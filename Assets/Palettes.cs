using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Palettes : MonoBehaviour
{
    public palette[] colorPalettes;
    void Awake()
    {
        FractalGenerator.colors = colorPalettes[0].colors;
    }

    public void SetPalette(int num)
    {
        Debug.Log("palette swapped to"+num);
        FractalGenerator.colors = colorPalettes[num].colors;
        FractalGenerator.Regenerate();

    }
    // Start is called before the first frame update
}
[System.Serializable]
public struct palette
{
    public Color[] colors;

}
