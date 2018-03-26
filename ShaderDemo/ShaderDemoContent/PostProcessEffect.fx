float SketchThreshold = 1.0;
float SketchBrightness = 0.333;
float2 SketchJitter;
float2 ScreenResolution;

sampler Sampler;
float2 SamplerSize;

#define SampleCount 15
float2 SampleOffsets[SampleCount];
float SampleWeights[SampleCount];

texture SketchTexture;
sampler SketchSampler : register(s1) = sampler_state
{
	Texture = (SketchTexture);
	AddressU = Wrap;
	AddressV = Wrap;
};

texture NoiseTexture;
sampler NoiseSampler : register(s2) = sampler_state
{
	Texture = (NoiseTexture);
	AddressU = Wrap;
	AddressV = Wrap;
};

struct PixelShader_Input
{
	float2 TexCoord : TEXCOORD0;
};

float4 SketchEffect(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(Sampler, p.TexCoord.xy);

	float4 noise = tex2D(NoiseSampler, p.TexCoord.xy);
	noise.xy = (noise.xy - 0.5f) * 1.8f;

	float blur = 0.0015;
	float blur2 = sin(radians(p.TexCoord.x * 270)) * 0.001f;

	//簡易ブラー
	col += tex2D(Sampler, p.TexCoord.xy - (noise.xy) * noise.z * 0.015 - blur);
	col += tex2D(Sampler, p.TexCoord.xy - (noise.xy) * noise.z * 0.015 + blur2);
	col *= 0.3333f;

	//シーンの色を調整して、非常に暗い値を削除し、コントラストを上げる
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//スケッチパターンのオーバーレイテクスチャを検索
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//結果を正の色空間のグレースケール値に変換する
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	col *= sketchResult;
	//col = sketchResult;

	//画像の中心座標を取得する
	float2 origin = SamplerSize * 0.5;
	float column = round((p.TexCoord.x * SamplerSize.x) + 0.5);
	float row = round((p.TexCoord.y * SamplerSize.y) + 0.5);

	//中心から奥にかけてマスクをかける
	float range = SamplerSize.x * 0.5625f;
	col.rgb += distance(origin, float2(column, row)) / range;
	col.a = distance(origin, float2(column, row)) / range;

	//col.rgb -= 0.2;
	//col.rgb = (col.r + col.g + col.b) * 0.3333f;

	return col;
}

float4 SketchEffect2(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(Sampler, p.TexCoord.xy);

	float blur = sin(radians(p.TexCoord.x * 270)) * 0.002f;
	float blur2 = sin(radians(p.TexCoord.y * 270)) * 0.002f;

	col += tex2D(Sampler, float2(p.TexCoord.x + blur, p.TexCoord.y + blur2));
	col += tex2D(Sampler, float2(p.TexCoord.x - blur, p.TexCoord.y - blur2));
	col *= 0.3333f;

	//シーンの色を調整して、非常に暗い値を削除し、コントラストを上げる
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//スケッチパターンのオーバーレイテクスチャを検索
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//結果を正の色空間のグレースケール値に変換する
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	col *= sketchResult;

	//scene = sketchResult;

	return col;
}

float4 Monotone(PixelShader_Input p) : COLOR0
{
	p.TexCoord.x += sin(radians(p.TexCoord.y * 270)) * 0.1f;
	p.TexCoord.y += cos(radians(p.TexCoord.x * 270)) * 0.1f;

	float4 col = tex2D(Sampler, p.TexCoord.xy);
	col.rgb = (col.r + col.g + col.b) * 0.3333f;

	return col;
}

float4 Sepiatone(PixelShader_Input p) : COLOR0
{
	float4 sepiaTone = float4(0.8f, 0.45f, 0.1f, 1.0f);
	float4 col = tex2D(Sampler, p.TexCoord);

	col.rgb = (col.r + col.g + col.b) * 0.3333f;
	
	col *= sepiaTone;

	return col;
}

float4 Distortion(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(NoiseSampler, p.TexCoord.xy);
	col.xy = (col.xy - 0.5f) * 2.0f;

	return tex2D(Sampler, p.TexCoord.xy - (col.xy) * col.z * 0.03f);
}

technique Sketch
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 SketchEffect();
	}
}

technique Flip
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 SketchEffect2();
	}
}

technique Mono
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 Monotone();
	}
}

technique Sepia
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 Sepiatone();
	}
}

technique Noise
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 Distortion();
	}
}