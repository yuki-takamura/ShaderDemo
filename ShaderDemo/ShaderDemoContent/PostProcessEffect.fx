#include "CalculationMacro.h"
#include "PostEffectMacro.h"

float SketchThreshold;
float SketchBrightness;
float2 SketchJitter;

sampler Sampler;
float2 SamplerSize;

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

	//�m�C�Y�e�N�X�`���̏�񂩂�A�c�܂���ʂ����肷��
	float4 noise = tex2D(NoiseSampler, p.TexCoord.xy);
	noise.xy = (noise.xy - noiseOffset) * noiseIntensity;

	float blur[2];
	blur[0] = blurIntensity[0];
	blur[1] = sin(radians(p.TexCoord.x * theta)) * blurIntensity[1];

	//�ȈՃu���[
	noise.xy *= noise.z * noiseCoefficient;
	col += tex2D(Sampler, p.TexCoord.xy - noise.xy - blur[0]);
	col += tex2D(Sampler, p.TexCoord.xy - noise.xy + blur[1]);
	col *= aThird;

	//�V�[���̐F�𒲐����āA���ɈÂ��l���폜���A�R���g���X�g���グ��
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//�X�P�b�`�p�^�[���̃I�[�o�[���C�e�N�X�`��������
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//���ʂ𐳂̐F��Ԃ̃O���[�X�P�[���l�ɕϊ�����
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);
	col *= sketchResult;

	//�摜�̒��S���W���擾����
	float2 origin = SamplerSize * aHalf;
	float column = round((p.TexCoord.x * SamplerSize.x));
	float row = round((p.TexCoord.y * SamplerSize.y));

	//���S���牜�ɂ����ĔZ���Ȃ�}�X�N��������
	float range = SamplerSize.x * percentage;
	col.rgb += distance(origin, float2(column, row)) / range;
	col.a = distance(origin, float2(column, row)) / range;

	return col;
}

float4 SketchWithSceneFringe(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(Sampler, p.TexCoord.xy);

	float blur[2];
	blur[0] = sin(radians(p.TexCoord.x * theta)) * sketchBlurIntensity;
	blur[1] = sin(radians(p.TexCoord.y * theta)) * sketchBlurIntensity;

	//�ȈՃu���[
	col += tex2D(Sampler, float2(p.TexCoord.x + blur[0], p.TexCoord.y + blur[1]));
	col += tex2D(Sampler, float2(p.TexCoord.x - blur[0], p.TexCoord.y - blur[1]));
	col *= aThird;

	//�F����
	//�ȈՃu���[�Ɠ��l�ɐF�����̋��x�������W�����炵�A�s�v�ȐF�̒������s��
	float4 fringeCol[2];
	fringeCol[0] = tex2D(Sampler, p.TexCoord.xy - fringeIntensity);
	fringeCol[0].g = colorAdjustValue;
	fringeCol[1] = tex2D(Sampler, p.TexCoord.xy + fringeIntensity);
	fringeCol[1].r = colorAdjustValue;

	//�ł����F������
	col += fringeCol[0] + fringeCol[1];
	col *= aThird;

	//�V�[���̐F�𒲐����āA���ɈÂ��l���폜���A�R���g���X�g���グ��
	float3 saturatedScene = saturate((col - SketchThreshold) * 2);

	//�X�P�b�`�p�^�[���̃I�[�o�[���C�e�N�X�`��������
	float3 sketchPattern = tex2D(SketchSampler, p.TexCoord + SketchJitter);

	float3 negativeSketch = (1 - saturatedScene) * (1 - sketchPattern);
	//���ʂ𐳂̐F��Ԃ̃O���[�X�P�[���l�ɕϊ�����
	float sketchResult = dot(1 - negativeSketch, SketchBrightness);

	col *= sketchResult;

	return col;
}

float4 Monotone(PixelShader_Input p) : COLOR0
{
	//�O�p�֐��Řc�܂���
	p.TexCoord.x += sin(radians(p.TexCoord.y * theta)) * curveIntensity;
	p.TexCoord.y += cos(radians(p.TexCoord.x * theta)) * curveIntensity;

	float4 col = tex2D(Sampler, p.TexCoord.xy);
	col.rgb = (col.r + col.g + col.b) * aThird;

	return col;
}

float4 Sepiatone(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(Sampler, p.TexCoord);

	col.rgb = (col.r + col.g + col.b) * aThird;
	
	col *= sepiaTone;

	return col;
}

float4 Distortion(PixelShader_Input p) : COLOR0
{
	float4 col = tex2D(NoiseSampler, p.TexCoord.xy);
	col.xy = (col.xy + distortionOffset) * distortionIntensity;

	return tex2D(Sampler, p.TexCoord.xy - (col.xy) * col.z * distortionCoefficient);
}

technique Sketch
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 SketchEffect();
	}
}

technique Fringe
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 SketchWithSceneFringe();
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