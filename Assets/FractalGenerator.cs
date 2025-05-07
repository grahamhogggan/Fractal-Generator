using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;
using System;

public class FractalGenerator : MonoBehaviour
{
    public RawImage image;
    public int width;
    public int height;
    public int maxIteration;

    public Tuple offset;
    public double zoom;
    public double tolerance = 0.1f;
    public static double brightness = 1;


    public static Polynomial equation;
    public static Polynomial derivitave;
    public static double[] polynomialCoefficients;

    //speed variables;
    double widthScaled;
    double heightScaled;
    public ComplexNumber[] roots =
    {
        
        //10th degree polynomial roots
        /*
        new ComplexNumber(-0.974186f,0),
        new ComplexNumber(1.18532f,0),
        new ComplexNumber(-0.485874f,-0.879672f),
        new ComplexNumber(-0.485874f,0.879672f),
        new ComplexNumber(-0.321310f,-0.456570f),
        new ComplexNumber(-0.321310f,0.456570f),
        new ComplexNumber(0.313768f,-0.457131f),
        new ComplexNumber(0.313768f,0.457131f),
        new ComplexNumber(0.744990f,-0.850553f),
        new ComplexNumber(0.744990f,0.850553f),
        */
        /*
        new ComplexNumber(-2.18574f,0),
        new ComplexNumber(-0.97516f,0),
        new ComplexNumber(-0.118597f,0),
        new ComplexNumber(1.47373f,0),
        new ComplexNumber(-0.255145f,-0.945623f),
        new ComplexNumber(-0.255145f,0.945623f),
        new ComplexNumber(0.824690f,-0.502615f),
        new ComplexNumber(0.824690f,0.502615f),
        */
    };
    public Color[] colors = new Color[3];

    public double translateSpeed;
    public double zoomSpeed;

    public int colorScaleFactor = 1;
    private static int colorScale;

    private static Texture2D texture;
    public enum FractalType
    {
        Mandelbrot = 0,
        Newtonian = 1,
        BurningShip = 2,
        Sine_Cosine_Distortion = 3,
        PlanktonFractal = 4,
    };
    public static FractalType fractalType;
    private static FractalGenerator gen;
    void Awake()
    {
        polynomialCoefficients = new double[10];
        polynomialCoefficients[0] = 1;
        polynomialCoefficients[3] = 1;
        polynomialCoefficients[2]=1;
        equation = new Polynomial(polynomialCoefficients);
        derivitave = equation.Derivative();
        roots = equation.GetRoots();
    }
    void Start()
    {
        gen = this;
        colorScale = colorScaleFactor;

        GenerateFractal(false);

    }
    void Update()
    {


        colorScale = colorScaleFactor;
        double dirX = Input.GetAxisRaw("Horizontal") * translateSpeed * Time.deltaTime / zoom;
        if (Math.Abs(dirX) > 0)
        {
            offset += new Tuple(dirX, 0);
            GenerateFractal(false);
        }

        double dirY = Input.GetAxisRaw("Vertical") * translateSpeed * Time.deltaTime / zoom;
        if (Math.Abs(dirY) > 0)
        {
            offset += new Tuple(0, dirY);
            GenerateFractal(false);
        }

        double dirZ = Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed;

        if (Input.GetKey("left shift"))
        {
            brightness += dirZ;
            GenerateFractal(false);
        }
        else if (Math.Abs(dirZ) > 0)
        {
            zoom += dirZ * zoom;
            GenerateFractal(false);
        }


        if (Input.GetKeyDown("space"))
        {
            int initWidth = width;
            int initHeight = height;
            double initZoom = zoom;
            int initIter = maxIteration;
            while (maxIteration < 3000) maxIteration *= 2;
            while (width < 3000)
            {
                width *= 2;
                height *= 2;
                zoom *= 2;
            }
            GenerateFractal(true);
            width = initWidth;
            height = initHeight;
            zoom = initZoom;
            maxIteration = initIter;
            
        }
        if (!Input.GetKey("left shift") && !Input.GetKey("left ctrl"))
        {
            if (Input.GetKeyDown("e") && width < 3000)
            {
                width *= 2;
                height *= 2;
                zoom *= 2;
            }
            if (Input.GetKeyDown("q") && width > 10)
            {
                width /= 2;
                height /= 2;
                zoom /= 2;
            }
        }
        else if (Input.GetKey("left ctrl"))
        {
            if (Input.GetKeyDown("e") && colorScale < 500)
            {
                colorScaleFactor *= 2;
                colorScale *= 2;
            }
            if (Input.GetKeyDown("q") && colorScale > 1)
            {
                colorScaleFactor /= 2;
                colorScale /= 2;
            }
            GenerateFractal(false);
        }
        else
        {
            if (Input.GetKeyDown("e") && maxIteration < 3000)
            {
                maxIteration *= 2;
            }
            if (Input.GetKeyDown("q") && maxIteration > 1)
            {
                maxIteration /= 2;
            }
        }

    }
    void LateUpdate()
    {
                equation = new Polynomial(polynomialCoefficients);
        derivitave = equation.Derivative();
                roots = equation.GetRoots();

    }
    public static void Reset()
    {
        gen.zoom = 10;
        gen.width = 64;
        gen.height = 64;
        gen.maxIteration = 100;
        gen.colorScaleFactor = 8;
        brightness = 1;
        gen.GenerateFractal(false);
    }
    void GenerateFractal(bool save)
    {
        widthScaled = width / 2.0f;
        heightScaled = height / 2.0f;
        double beginningTime = Time.realtimeSinceStartup;


        texture = new Texture2D(width, height);

        JobHandle[] rowJobs = new JobHandle[width];

        NativeArray<Color>[] grid = new NativeArray<Color>[width];
        for (int i = 0; i < width; i++)
        {
            grid[i] = new NativeArray<Color>(height, Unity.Collections.Allocator.Persistent);
        }

        NativeArray<ComplexNumber> rootsNA = new NativeArray<ComplexNumber>(roots, Unity.Collections.Allocator.Persistent);
        NativeArray<Color> colorsNA = new NativeArray<Color>(colors, Unity.Collections.Allocator.Persistent);
        for (int x = 0; x < width; x++)
        {


            var job = new CalculateRow()
            {
                widthScaled = this.widthScaled,
                heightScaled = this.heightScaled,
                x = x,
                roots = rootsNA,
                colors = colorsNA,
                maxIteration = this.maxIteration,
                offset = this.offset,
                zoom = this.zoom,
                height = this.height,
                rowColors = grid[x],
                tolerance = this.tolerance,
            };
            rowJobs[x] = job.Schedule();
        }


        foreach (JobHandle j in rowJobs)
        {
            j.Complete();
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(i, j, grid[i][j]);
            }
        }
        image.texture = texture;
        texture.Apply();
        if (save)
        {
            Save(texture);
            Debug.Log("Generation finished in " + (Time.realtimeSinceStartup - beginningTime) + " seconds");
            beginningTime = Time.realtimeSinceStartup;
        }

        rootsNA.Dispose();
        colorsNA.Dispose();
        foreach (var r in grid)
        {

            r.Dispose();
        }

    }
    public void Save(Texture2D texture)
    {
        byte[] data = texture.EncodeToPNG();
        string ID = UnityEngine.Random.Range(0, 10000).ToString();
        if(!Directory.Exists(Application.dataPath+"/Saved_Images"))
        {
            Directory.CreateDirectory(Application.dataPath+"/Saved_Images");
        }
        string path = Application.dataPath + "/Saved_Images/Fractal_" + ID + ".png";
        File.WriteAllBytes(path, data);
    }



    //function and its derivative
    public static ComplexNumber Function(ComplexNumber c)
    {
        return equation.Evaluate(c);

        //10th degree polynomial
        //return 7*c.Pow(10) - 5*c.Pow(9) + 3*c.Pow(6) - 10*c.Pow(4)-2*c.Pow(2) + new ComplexNumber(-1,0);
        //8th degree polynomial
        //return 3 * c.Pow(8) + 2 * c.Pow(7) - 10 * c.Pow(6) + 3 * c.Pow(3) + 4 * c.Pow(2) - 8 * c + new ComplexNumber(-1, 0);
    }
    public static ComplexNumber FunctionDerivative(ComplexNumber c)
    {
        return (derivitave.Evaluate(c));
        //derivative of 10th degree polynomial
        //return 70 * c.Pow(9) - 45 * c.Pow(8) + 18 * c.Pow(5) - 40 * c.Pow(3) - 4 * c;
        //Derivative of 8th degree polynomial
        //return 24 * c.Pow(7) + 14 * c.Pow(6) - 60 * c.Pow(5) + 9 * c.Pow(2) + 8 * c+ new ComplexNumber(-8, 0);
    }


    struct CalculateRow : IJob
    {
        public double widthScaled;
        public double heightScaled;
        public int x;
        [ReadOnly]
        public NativeArray<ComplexNumber> roots;
        [ReadOnly]
        public NativeArray<Color> colors;
        public double tolerance;
        public double maxIteration;
        public Tuple offset;
        public double zoom;
        public double height;

        public NativeArray<Color> rowColors;

        public void Execute()
        {

            for (int y = 0; y < height; y++)
            {
                double zx = (x - widthScaled) / zoom + offset.x;
                double zy = (y - heightScaled) / zoom + offset.y;

                ComplexNumber z = new ComplexNumber(zx, zy);
                rowColors[y] = ComputeColor(z);
            }
        }
        public Color ComputeColor(ComplexNumber z)
        {
            switch (fractalType)
            {
                case FractalType.Newtonian:
                    return Newton(z);
                    break;
                case FractalType.Mandelbrot:
                    return Mandel(z);
                    break;
                case FractalType.BurningShip:
                    return Burning(z);
                    break;
                case FractalType.Sine_Cosine_Distortion:
                    return Trig(z);
                    break;
                case FractalType.PlanktonFractal:
                    return Plankton(z);
                    break;
            }
            return new Color(0, 0, 0, 1);
        }
        public Color Newton(ComplexNumber z)
        {
            bool succeeded = false;
            for (int iteration = 0; iteration < maxIteration; iteration++)
            {
                succeeded = false;
                z -= Function(z) / FunctionDerivative(z);
                for (int r = 0; r < roots.Length; r++)
                {
                    ComplexNumber difference = z - roots[r];
                    if (difference.MagnitudeSquared() < tolerance)
                    {
                        Color result = colors[Math.Clamp(r*colorScale,0,colors.Length-1)] * (float)brightness / Mathf.Sqrt(iteration);
                        result.a = 1;
                        return result;
                        succeeded = true;
                        break;
                    }
                }
                if (succeeded)
                {
                    break;
                }
            }
            return new Color(0, 0, 0, 1);
        }
        public Color Mandel(ComplexNumber z)
        {
            ComplexNumber c = new ComplexNumber(0, 0);
            int iterations = 0;
            for (int i = 0; i < maxIteration; i++)
            {
                if (c.MagnitudeSquared() > 4)
                {
                    int col = iterations % (colors.Length * colorScale) / colorScale;

                    Color res = colors[col] * (float)brightness / Mathf.Sqrt(iterations);
                    res.a = 1;
                    return res;
                }
                c = c.Pow(2) + z;
                iterations++;
            }
            return new Color(0, 0, 0, 1);
        }
        public Color Burning(ComplexNumber z)
        {
            ComplexNumber c = new ComplexNumber(0, 0);
            int iterations = 0;
            for (int i = 0; i < maxIteration; i++)
            {
                if (c.MagnitudeSquared() > 4)
                {
                    int col = iterations % (colors.Length * colorScale) / colorScale;

                    Color res = colors[col] * (float)brightness / Mathf.Sqrt(iterations);
                    res.a = 1;
                    return res;
                }
                ComplexNumber abs = new ComplexNumber(Math.Abs(c.real), Math.Abs(c.imaginary));
                c = abs.Pow(2) + z;
                iterations++;
            }
            return new Color(0, 0, 0, 1);
        }
        public Color Trig(ComplexNumber z)
        {
            ComplexNumber c = new ComplexNumber(0, 0);
            int iterations = 0;
            for (int i = 0; i < maxIteration; i++)
            {
                if (c.MagnitudeSquared() > 4)
                {
                    int col = iterations % (colors.Length * colorScale) / colorScale;

                    Color res = colors[col] * (float)brightness / Mathf.Sqrt(iterations);
                    res.a = 1;
                    return res;
                }
                c = new ComplexNumber(Math.Sin(c.real), Math.Cos(c.imaginary)) * c + z;
                iterations++;
            }
            return new Color(0, 0, 0, 1);
        }
        public Color Plankton(ComplexNumber z)
        {
            ComplexNumber c = new ComplexNumber(0, 0);
            int iterations = 0;
            for (int i = 0; i < maxIteration; i++)
            {
                if (c.MagnitudeSquared() > 4)
                {
                    int col = iterations % (colors.Length * colorScale) / colorScale;

                    Color res = colors[col] * (float)brightness / Mathf.Sqrt(iterations);
                    res.a = 1;
                    return res;
                }
                c = new ComplexNumber(c.imaginary * c.real, c.imaginary + c.real) * c + z;
                iterations++;
            }
            return new Color(0, 0, 0, 1);
        }



    }



}







public struct Tuple
{
    public double x;
    public double y;
    public Tuple(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
    public static Tuple operator +(Tuple a, Tuple b)
    {
        return new Tuple(a.x + b.x, a.y + b.y);
    }
}

//complex number struct
public struct ComplexNumber
{
    public double real;
    public double imaginary;

    public ComplexNumber(double real, double imaginary)
    {
        this.real = real;
        this.imaginary = imaginary;
    }

    public double MagnitudeSquared()
    {
        return real * real + imaginary * imaginary;
    }

    public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(
            a.real * b.real - a.imaginary * b.imaginary,
            a.real * b.imaginary + a.imaginary * b.real
        );
    }

    public static ComplexNumber operator *(double a, ComplexNumber b)
    {
        return new ComplexNumber(
            b.real * a, b.imaginary * a
        );
    }

    public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(a.real + b.real, a.imaginary + b.imaginary);
    }
    public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(a.real - b.real, a.imaginary - b.imaginary);
    }
    public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
    {
        double denominator = b.real * b.real + b.imaginary * b.imaginary;
        if (denominator == 0)
        {
            return (new ComplexNumber(0, 0));
        }
        return new ComplexNumber(
            (a.real * b.real + a.imaginary * b.imaginary) / denominator,
            (a.imaginary * b.real - a.real * b.imaginary) / denominator
        );
    }
    public ComplexNumber Pow(int power)
    {
        ComplexNumber result = new ComplexNumber(1, 0);
        for (int i = 0; i < power; i++)
        {
            result *= this;
        }
        return result;
    }







}
//*****************************************************
/*
 * polynomial struct
 * input an array of doubles starting with the constant term and increasing. input 0 for a skipped term
 */
//*****************************************************
public struct Polynomial
{
    public double[] coefficients;

    public Polynomial(double[] coefficients)
    {
        this.coefficients = coefficients;
    }
    public ComplexNumber Evaluate(ComplexNumber input)
    {

        ComplexNumber result = new ComplexNumber(0, 0);
        for (int i = 0; i < coefficients.Length; i++)
        {
            result += coefficients[i] * input.Pow(i);
        }
        return (ComplexNumber)result;
    }
    public Polynomial Derivative()
    {
        double[] derivArray = new double[coefficients.Length - 1];
        for (int i = 1; i < coefficients.Length; i++)
        {
            derivArray[i - 1] = coefficients[i] * i;
        }
        return new Polynomial(derivArray);
    }
    public ComplexNumber[] GetRoots()
    {

        ComplexNumber[] result = new ComplexNumber[coefficients.Length - 1];
        ComplexNumber guess = new ComplexNumber(1, 1);
        Polynomial poly = new Polynomial(coefficients);
        //Debug.Log("getting "+result.Length+" roots");
        for (int i = 0; i < result.Length; i++)
        {
            ComplexNumber root = poly.GetNewtonianRoot(guess);
            if (root.imaginary == 0)
            {
                //Debug.Log("root was real with value "+root.real);
                result[i] = root;
                //Debug.Log("dividing by polynomial " + root.real + "," + 0);
                Polynomial dividend = new Polynomial(
                    new double[]
                    {
                        result[i].real*-1,1
                    }
                    );
                poly = poly.Divide(dividend);
                /*
                string debug = "";
                foreach(double d in poly.coefficients)
                {
                    debug += d.ToString();
                    debug += ",";
                }
                Debug.Log("new polynomial is "+debug);
                */
            }
            else
            {
                //Debug.Log("complex root");
                result[i] = root;
                result[i + 1] = new ComplexNumber(root.real, root.imaginary * -1);

                //Debug.Log("dividing by polynomial");
                Polynomial dividend = new Polynomial(
                    new double[]
                    {
                        root.imaginary*root.imaginary+root.real*root.real,
                        -2*root.real,
                        1
                    }
                    );

                poly = poly.Divide(dividend);
                /*
                string debug = "";
                foreach (double d in poly.coefficients)
                {
                    debug += d.ToString();
                    debug += ",";
                }
                Debug.Log("new polynomial is " + debug);
                */
                i++;
            }
        }
        return result;
    }
    private ComplexNumber GetNewtonianRoot(ComplexNumber guess)
    {
        ComplexNumber g = guess;
        for (int i = 0; i < 100; i++)
        {
            g -= Evaluate(g) / Derivative().Evaluate(g);
        }
        return g;
    }
    public static Polynomial operator +(Polynomial x, Polynomial y)
    {
        int degree = Math.Max(x.coefficients.Length, y.coefficients.Length);
        double[] resultCoefficients = new double[degree];
        for (int i = 0; i < degree; i++)
        {
            resultCoefficients[i] = 0;
            if (x.coefficients.Length > i)
            {
                resultCoefficients[i] += x.coefficients[i];
            }
            if (y.coefficients.Length > i)
            {
                resultCoefficients[i] += y.coefficients[i];
            }
        }
        return new Polynomial(resultCoefficients);
    }
    public static Polynomial operator -(Polynomial x, Polynomial y)
    {
        int degree = Math.Max(x.coefficients.Length, y.coefficients.Length);
        double[] resultCoefficients = new double[degree];
        for (int i = 0; i < degree; i++)
        {
            resultCoefficients[i] = 0;
            if (x.coefficients.Length > i)
            {
                resultCoefficients[i] += x.coefficients[i];
            }
            if (y.coefficients.Length > i)
            {
                resultCoefficients[i] -= y.coefficients[i];
            }
        }
        /*
        Debug.Log("result of subtraction coefficients");
        foreach (double d in resultCoefficients)
        {
            Debug.Log(d);
        }
        */
        return new Polynomial(resultCoefficients);
    }
    public static Polynomial operator *(Polynomial x, double y)
    {
        double[] resCo = new double[x.coefficients.Length];
        for (int i = 0; i < x.coefficients.Length; i++)
        {
            resCo[i] = x.coefficients[i] * y;
        }
        /*
        Debug.Log("result of multiplication coefficients");
        foreach (double d in resCo)
        {
            Debug.Log(d);
        }
        */
        return new Polynomial(resCo);
    }
    public Polynomial MultiplyByPowerOfX(int pow)
    {
        double[] ResCo = new double[coefficients.Length + pow];
        for (int i = 0; i < coefficients.Length; i++)
        {
            ResCo[ResCo.Length - 1 - i] = coefficients[coefficients.Length - 1 - i];
        }
        /*
        Debug.Log("result of scaling by x^" + pow);
        foreach (double d in ResCo)
        {
            Debug.Log(d);
        }
        */
        return new Polynomial(ResCo);
    }
    public Polynomial Divide(Polynomial p)
    {
        double[] resCo = new double[coefficients.Length - p.coefficients.Length + 1];
        //Debug.Log("resulting polynomial is of length " + resCo.Length);
        Polynomial dividend = new Polynomial(coefficients);

        double leadingCoefficient = p.coefficients[p.coefficients.Length - 1];
        for (int i = 0; i < resCo.Length; i++)
        {
            //Debug.Log("iteration " + i);
            int place = dividend.coefficients.Length - 1 - i;
            // Debug.Log("divided coefficient is " + dividend.coefficients[place]);
            int rPlace = resCo.Length - 1 - i;
            double result = dividend.coefficients[place] / leadingCoefficient;
            resCo[rPlace] = result;
            // Debug.Log("calculated value of " +result);
            dividend -= (p * result).MultiplyByPowerOfX(rPlace);
            /*
            Debug.Log("new dividend coefficients");
            foreach(double d in dividend.coefficients)
            {
                Debug.Log(d);
            }
            */

        }
        /*
        Debug.Log("Result: ");
        foreach (double d in resCo)
        {
            Debug.Log(d);
        }
        */
        return new Polynomial(resCo);
    }

}

