using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityStandardAssets.ImageEffects;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class VHSEffect : ImageEffectBase {

//	[DisplayModifier(true)]
	public Texture2D baseNoise;
//	[DisplayModifier(true)]
	public Texture2D parasites;
    Color[] colors;

    [Range(0, 3f)]
    public float effectIntensity = 1;
    public bool onlyHalfScreen;
    public bool skipEffect;

	[Range(0, 1f)]
	public float minWhiteNoise=0.1f,maxWhiteNoise=0.35f;
	[Range(0,1f)]
    public float red=1, green = 0.7f, blue = 0.3f;
    [Range(0, 1f)]
    public float redAlpha = 1, greenAlpha = 0.7f, blueAlpha = 0.3f,minAlpha=0.5f;

    public Vector2 blurDistance=new Vector2(0.01f, 0.01f);
    [Range(0,256)]
    public int blurXsteps = 128;
    [Range(0, 256)]
    public int blurYsteps = 128;
    [Range(0, 0.01f)]
    public float distortionX = 0.00075f;
    [Range(0, 1f)]
    public float exponentialNess = 0.7f;
    [Range(0, 100000)]
    public int minXSpacing=80, maxXSpacing=200;
	[Range(0, 100000)]
	public int minYSpacing = 3, maxYSpacing = 5;

	[Range(1,32)]
    public int parasiteLength=5;

    string autoParasiteName = "GeneratedParasiteNoise";
	string autoBaseNoiseName = "GeneratedBaseNoise";

    private void Awake()
    {
       // ResetTextures();       
    }

    override protected void  Start()
    {

        enabled = false;
     //   RefreshAll(false);
        enabled = true;
	}

	public void RefreshAll(bool resetTextures=true)
	{
       if (resetTextures)
            ResetTextures();

        int xPowOf2 = Mathf.ClosestPowerOfTwo(Screen.width), yPowOf2 = Mathf.ClosestPowerOfTwo(Screen.height);
        if (colors == null || colors.Length != xPowOf2 * yPowOf2)
            colors = new Color[xPowOf2 * yPowOf2];

             
		//if (!baseNoise)
		CreateNoise(xPowOf2, yPowOf2);

		//if (!parasites || parasites.name.Contains(autoParasiteName))
		CreateTexture(xPowOf2, yPowOf2);

	}

	private void Update()
    {

    }

    private void OnPreRender()
    {/*
        if (colors == null)
        {   
            RefreshAll(false);
        }*/
        material.SetFloatArray("_WhiteNoiseSettings", new[] { minWhiteNoise, maxWhiteNoise, 0.5f });

    }


    [Header("Dev variables")]
    [Range(1, 4)]
    public int AAlevel = 1;
    [Range(1, 32)]
    public int downSamplingX=1, downSamplingY=1;

	public bool onlyBlur;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        // RefreshNoise();
        if (skipEffect)
        {
            Graphics.Blit(source, destination);
            return;
        }


        material.SetTexture("_NoiseTex", parasites);
        material.SetVector("_NoiseTex_TexelSize",
            new Vector4(1f/parasites.width, 1f / parasites.height, parasites.width, parasites.height));
        material.SetVector("_BlurVars",
            new Vector4(blurDistance.x, blurDistance.y, blurXsteps, blurYsteps));

        material.SetFloat("_WhiteNoiseMin",  minWhiteNoise);
        material.SetFloat("_DistortX", distortionX);
       
        material.SetFloat("_WhiteNoiseMax", maxWhiteNoise);
        material.SetFloat("_OverallEffect", effectIntensity);
        material.SetFloat("_HalfScreen", onlyHalfScreen ? 1 : 0);
		/*
       
        material.EnableKeyword("_MainTex");
        material.EnableKeyword("_NoiseTex");
        material.SetTextureScale("_NoiseTex", new Vector2(0.9f, 21.6f));
        material.SetTextureScale("_MainTex", new Vector2(0.9f, 21.6f));
        */

		//source.useMipMap = true;
		//source.mipMapBias = -3;
		int closePowX = Mathf.ClosestPowerOfTwo(source.width);
		int closePowY = Mathf.ClosestPowerOfTwo(source.height);

		//pre-blur pass
		RenderTexture blurH = RenderTexture.GetTemporary((closePowX / downSamplingX), (closePowY / downSamplingY), 0, source.format, RenderTextureReadWrite.Default, 1 << (AAlevel - 1));
		RenderTexture blur = RenderTexture.GetTemporary((closePowX / downSamplingX), (closePowY / downSamplingY),0, source.format, RenderTextureReadWrite.Default, 1 << (AAlevel - 1));

		blurH.filterMode = FilterMode.Trilinear;
		Graphics.Blit(source, blurH, material,0);
		Graphics.Blit(blurH, blur, material,1);
		
		Graphics.Blit(blur, blurH, material, 0);
		Graphics.Blit(blurH, blur, material, 1);
		material.SetTexture("_BlurTex", blur);

		if (onlyBlur)
		{
			Graphics.Blit(blur, destination);


			return;
		} else
		{
			RenderTexture temp = RenderTexture.GetTemporary(closePowX, closePowY, 24, source.format, RenderTextureReadWrite.Default, 1 << (AAlevel - 1));
			temp.filterMode = FilterMode.Trilinear;
			//Graphics.Blit(blur, source);
			RenderTexture.ReleaseTemporary(temp);
			Graphics.Blit(source, destination, material, 2);
		}


		//final pass

        //Graphics.CopyTexture(source,0,0, temp,0,0);

        
        //Graphics.BlitMultiTap(source, destination, material, new Vector2[] { new Vector2(0.1f, 0.1f),new Vector2(-1, -1)});
        
		RenderTexture.ReleaseTemporary(blurH);
		RenderTexture.ReleaseTemporary(blur);
	}
    
    void FillParasites()
    {
        var pixelCount = parasites.width * parasites.height;
        System.Func<float,float>  expInterp = (_a) => Mathf.Lerp(_a, _a* _a, exponentialNess);

        float r, g, b, ra,ga,ba;

        for (int y = Random.Range(minYSpacing, maxYSpacing); 
			y < parasites.height;
			y+= Random.Range(minYSpacing, maxYSpacing))
        {
			for (int x = parasiteLength + Random.Range(0, maxXSpacing); 
				x< parasites.width; 
				x +=  parasiteLength + Random.Range(minXSpacing, maxXSpacing))
			{
				for (int s = 0; s < parasiteLength; s++)
				{
					r = expInterp(Random.value * red * (ra = Mathf.Max(minAlpha, redAlpha)));
					g = expInterp(Random.value * green * (ga = Mathf.Max(minAlpha, greenAlpha)));
					b = expInterp(Random.value * blue * (ba = Mathf.Max(minAlpha, blueAlpha)));

					var nColor = new Color(r, g, b, Mathf.Max(minAlpha, ra, ba, ga));
					colors[y* parasites.width + x - s] = nColor;// Color.Lerp(old[i-s], nColor, 0.75f);
				}
			}
		}
       
        parasites.SetPixels(colors);
        parasites.Apply();
    }


    void CreateTexture(int width, int height)
    {
        
        parasites = new Texture2D(width, height,TextureFormat.RGBAFloat,false,true);
		
        parasites.name = autoParasiteName;
        FillParasites();
        
    }

	void CreateNoise(int width, int height)
	{

		var pixelCount = width * height;
		baseNoise = new Texture2D(width, height, TextureFormat.RGBAFloat, true, true);
		baseNoise.name = autoBaseNoiseName;
   
		for (int i = 0; i < pixelCount; i++)
		{
			// if (i % Screen.height > Screen.height) continue;
			var val = Random.Range(0.000f,1f);
			colors[i] = new Color(val,val,val,1);

		}

		baseNoise.SetPixels(colors);
		baseNoise.Apply();
	}



    private void ResetTextures()
    {
        //colors = null;
        baseNoise = null;
        parasites = null;
    }

    protected override void OnDisable()
	{

        //RefreshAll(false);
    }

    private void Reset()
    {

        RefreshAll();
    }


    private void OnValidate()
    {

    //    RefreshAll(false);
    }

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(VHSEffect))]
public class VHSEditor : UnityEditor.Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = target as VHSEffect;

        if (GUILayout.Button("Rebuild Textures"))
        { 
            t.RefreshAll();
        }
     
    }
}
#endif