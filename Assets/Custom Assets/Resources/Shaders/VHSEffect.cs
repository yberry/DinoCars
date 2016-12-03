﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class VHSEffect : ImageEffectBase {

//	[DisplayModifier(true)]
	public Texture2D baseNoise;
//	[DisplayModifier(true)]
	public Texture2D parasites;
    Color[] colors;

	[Range(0, 1f)]
	public float minWhiteNoise=0.25f,maxWhiteNoise=0.75f;
	[Range(0,1f)]
    public float red=1, green = 1, blue = 1;
    [Range(0, 1f)]
    public float redAlpha = 1, greenAlpha = 1, blueAlpha = 1,minAlpha=0.5f;

    [Range(0, 1f)]
    public float exponentialNess = 0;
    [Range(0, 100000)]
    public int minXSpacing=3, maxXSpacing=6;
	[Range(0, 100000)]
	public int minYSpacing = 3, maxYSpacing = 6;

	[Range(1,32)]
    public int parasiteLength=4;

    string autoParasiteName = "GeneratedParasiteNoise";
	string autoBaseNoiseName = "GeneratedBaseNoise";

	void Start()
    {
		RefreshAll();
	}

	void RefreshAll()
	{
		if (colors == null)
			colors = new Color[Screen.width * Screen.height];

			//if (!baseNoise)
			CreateNoise(Screen.width, Screen.height);

			//if (!parasites || parasites.name.Contains(autoParasiteName))
			CreateTexture(Screen.width, Screen.height);

	}

	private void Update()
    {
      //  RefreshNoise();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
       // RefreshNoise();
        material.SetTexture("_NoiseTex", parasites);
		Graphics.Blit(source, destination, material);
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


    private void OnValidate()
    {
		RefreshAll();
    }

    void CreateTexture(int width, int height)
    {
        
        parasites = new Texture2D(width, height,TextureFormat.RGBAFloat,false,true);
		
        parasites.name = autoParasiteName;
        FillParasites();
        //noise.Apply(false, false);


    }

	void CreateNoise(int width, int height)
	{
		var pixelCount = width * height;
		baseNoise = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
		baseNoise.name = autoBaseNoiseName;
		for (int i = 0; i < pixelCount; i++)
		{
			// if (i % Screen.height > Screen.height) continue;
			var val = Random.Range(minWhiteNoise,maxWhiteNoise);
			colors[i] = new Color(val,val,val,val);

		}
		baseNoise.SetPixels(colors);
		baseNoise.Apply();
	}

	private void OnDisable()
	{
		RefreshAll();
	}
}

#if UNITY_EDITOR

#endif