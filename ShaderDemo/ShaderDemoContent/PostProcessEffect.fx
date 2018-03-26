sampler samplerState;

struct PixelShader_Input
{
	float2 TexCoord : TEXCOORD0;
};

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
	float4 sepiaTone = float4(0.6f, 0.4f, 0.4f, 1.0f);
	float4 col = tex2D(samplerState, p.TexCoord.xy);
	col.rgb = (col.r + col.g + col.b) * 0.3333f;
	col = col * sepiaTone;
	
	return col;
}

technique Flip
{
	pass Pass0
	{
		PixelShader = compile ps_3_0 Negative();
	}
}

technique Mono
{
	pass Pass0
	{
		PixelShader = compile ps_3_0 Monotone();
	}
}

technique Sepia
{
	pass Pass0
	{
		PixelShader = compile ps_3_0 Sepiatone();
	}
}