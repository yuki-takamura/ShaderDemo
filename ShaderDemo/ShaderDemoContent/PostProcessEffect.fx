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

	//�ȈՃu���[
	col += tex2D(Sampler, p.TexCoord.xy - (noise.xy) * noise.z * 0.015 - blur);
	col += tex2D(Sampler, p.TexCoord.xy - (noise.xy) * noise.z * 0.015 + blur2);
	col *= 0.3333f;

	//�V�[���̐F�𒲐����āA���ɈÂ��l���폜���A�R���g���X�g���グ��
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//�X�P�b�`�p�^�[���̃I�[�o�[���C�e�N�X�`��������
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//���ʂ𐳂̐F��Ԃ̃O���[�X�P�[���l�ɕϊ�����
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	col *= sketchResult;
	//col = sketchResult;

	//�摜�̒��S���W���擾����
	float2 origin = SamplerSize * 0.5;
	float column = round((p.TexCoord.x * SamplerSize.x) + 0.5);
	float row = round((p.TexCoord.y * SamplerSize.y) + 0.5);

	//���S���牜�ɂ����ă}�X�N��������
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

	//�V�[���̐F�𒲐����āA���ɈÂ��l���폜���A�R���g���X�g���グ��
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//�X�P�b�`�p�^�[���̃I�[�o�[���C�e�N�X�`��������
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//���ʂ𐳂̐F��Ԃ̃O���[�X�P�[���l�ɕϊ�����
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