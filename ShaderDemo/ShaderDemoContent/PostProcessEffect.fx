float SketchThreshold = 1.0;
float SketchBrightness = 0.333;
float2 SketchJitter;
float2 ScreenResolution;

sampler samplerState;

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
	float4 scene = tex2D(samplerState, p.TexCoord.xy);

	//�V�[���̐F�𒲐����āA���ɈÂ��l���폜���A�R���g���X�g���グ��
	float3 saturatedScene = saturate((scene - SketchThreshold) * 2);

	//�X�P�b�`�p�^�[���̃I�[�o�[���C�e�N�X�`��������
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//���ʂ𐳂̐F��Ԃ̃O���[�X�P�[���l�ɕϊ�����
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	scene *= sketchResult;
	//scene = sketchResult;

	return scene;
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
	float4 sepiaTone = float4(0.6f, 0.4f, 0.4f, 1.0f);
	float4 col = tex2D(samplerState, p.TexCoord.xy);
	col.rgb = (col.r + col.g + col.b) * 0.3333f;
	col = col * sepiaTone;
	
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