float SketchThreshold = 1.0;
float SketchBrightness = 0.333;
float2 SketchJitter;
float2 ScreenResolution;

sampler samplerState;
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

struct PixelShader_Input
{
	float2 TexCoord : TEXCOORD0;
};

float4 SketchEffect(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(samplerState, p.TexCoord.xy);

	float blur = 0.0015;
	float blur2 = sin(radians(p.TexCoord.x * 270)) * 0.001f;
	float blur3 = sin(radians(p.TexCoord.y * 270)) * 0.001f;

	for(int i = 0; i < 1; i++)
	{
		//col += tex2D(samplerState, float2(p.TexCoord.x - blur, p.TexCoord.y - blur));
		//col += tex2D(samplerState, float2(p.TexCoord.x + blur, p.TexCoord.y + blur));
		col += tex2D(samplerState, float2(p.TexCoord.x + blur2, p.TexCoord.y + blur3));
		col += tex2D(samplerState, float2(p.TexCoord.x - blur2, p.TexCoord.y - blur3));
	}

	col /= 3;

	//シーンの色を調整して、非常に暗い値を削除し、コントラストを上げる
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//スケッチパターンのオーバーレイテクスチャを検索
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - sketchPattern);
	//結果を正の色空間のグレースケール値に変換する
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	col *= sketchResult;

	//col = sketchResult;

	float2 origin = SamplerSize / 2;
	float column = round((p.TexCoord.x * SamplerSize.x) + 0.5);
	float row = round((p.TexCoord.y * SamplerSize.y) + 0.5);

	//if(abs(origin.x - column) > 600)
	//{
	//  col.rgb += lerp(600, origin.x, column);
	//  col.a = lerp(600, origin.x, column);
	//}
	//if(abs(origin.y - row) > 300)
	//{
	//  col.rgb += 0.1f;
	//  col.a = 0.1f;
	//}

	float num = 800;
	col.rgb += (distance(origin, float2(column, row)) / num);
	col.a = distance(origin, float2(column, row)) / num;

	return col;
}

float4 Negative(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(samplerState, p.TexCoord.xy);
	col.rgb = 1 - col.rgb;

	return col;
}

float4 Monotone(PixelShader_Input p) : COLOR0
{
	p.TexCoord.x += sin(radians(p.TexCoord.y * 270)) * 0.1f;
	p.TexCoord.y += cos(radians(p.TexCoord.x * 270)) * 0.1f;
	float4 col = tex2D(samplerState, p.TexCoord.xy);
	col.rgb = (col.r + col.g + col.b) * 0.3333f;

	return col;
}

float4 Sepiatone(PixelShader_Input p) : COLOR0
{
	float4 col = 0;

	//for(int i = 0; i < SampleCount; i++)
	//{
	//	col += tex2D(samplerState, p.TexCoord.xy + SampleOffsets[i]) * SampleWeights[i];
	//}
	
	col = tex2D(samplerState, p.TexCoord.xy);
	float blur = 0.0015;
	float blur2 = sin(radians(p.TexCoord.x * 270)) * 0.002f;
	float blur3 = sin(radians(p.TexCoord.y * 270)) * 0.002f;

	for(int i = 0; i < 1; i++)
	{
		//col += tex2D(samplerState, float2(p.TexCoord.x - blur, p.TexCoord.y - blur));
		//col += tex2D(samplerState, float2(p.TexCoord.x + blur, p.TexCoord.y + blur));
		col += tex2D(samplerState, float2(p.TexCoord.x + blur2, p.TexCoord.y + blur3));
		col += tex2D(samplerState, float2(p.TexCoord.x - blur2, p.TexCoord.y - blur3));
	}

	col /= 3;

	return col;
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
		PixelShader = compile ps_2_0 Negative();
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