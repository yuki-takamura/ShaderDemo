float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 LightViewProj;
float3 LightDirection = float3(1,1,0);

float DepthBias = 0.001f;

// TODO: ここでエフェクトのパラメーターを追加します。
float4 AmbientColor = float4(1,1,1,1);
float AmbientIntensity = 0.1;

float4x4 WorldInverseTranspose;

float4 DiffuseColor = float4(1,1,1,1);
float DiffuseIntensity = 1.0;

float Shininess = 20;
float4 SpecularColor = float4(1,1,1,1);
float SpecularIntensity = 1.0;
float3 ViewVector = float3(1, 1, 0);

texture ModelTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (ModelTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

float BumpConstant = 1;
texture NormalMap;
sampler2D bumpSampler = sampler_state
{
	Texture = (NormalMap);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
	Texture = <ShadowMap>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: ここにテクスチャー座標および頂点カラーなどの
    // 入力チャンネルを追加します。
	float4 Normal : NORMAL0;
	float3 Tangent : TANGENT0;
	float3 Binormal : BINORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

    // TODO: ここにカラーおよびテクスチャー座標などの頂点シェーダーの
    // 出力要素を追加します。これらの値は該当する三角形上で自動的に補間されて、
    // ピクセル シェーダーへの入力として提供されます。
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 Tangent : TEXCOORD2;
	float3 Binormal : TEXCOORD3;
	float4 WorldPos : TEXCOORD4;
};

struct CreateShadowMap_VSOutput
{
	float4 Position : POSITION;
	float Depth     : TEXCOORD0;
};

//モデルをライト空間にトランスフォームし、オブジェクトの深度外をレンダリングする
CreateShadowMap_VSOutput CreateShadowMap_VertexShader(float4 Position : POSITION)
{
	CreateShadowMap_VSOutput Out;
	Out.Position = mul(Position, mul(World, LightViewProj));
	Out.Depth = Out.Position.z / Out.Position.w;
	return Out;
}

float4 CreateShadowMap_PixelShader(CreateShadowMap_VSOutput input) : COLOR
{
	return float4(input.Depth, 0, 0, 0);
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4x4 WorldViewProj = mul(mul(World, View), Projection);
    output.Position = mul(input.Position, WorldViewProj);

    // TODO: ここで頂点シェーダー コードを追加します。
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.Tangent = normalize(mul(input.Tangent, WorldInverseTranspose));
	output.Binormal = normalize(mul(input.Binormal, WorldInverseTranspose));
	output.TextureCoordinate = input.TextureCoordinate;
	output.WorldPos = mul(input.Position, World);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
   float3 bump = BumpConstant * (tex2D(bumpSampler, input.TextureCoordinate) - (0.5, 0.5, 0.5));
   float3 bumpNormal = input.Normal + (bump.x * input.Tangent + bump.y * input.Binormal);
   bumpNormal = normalize(bumpNormal);

   float diffuseIntensity = dot(normalize(LightDirection), bumpNormal);
   if(diffuseIntensity < 0)
		diffuseIntensity = 0;

   float3 light = normalize(LightDirection);
   float3 r = normalize(2 * dot(light, bumpNormal) * bumpNormal - light);
   float3 v = normalize(mul(normalize(ViewVector), World));
   float dotProduct = dot(r, v);

   float4 specular = SpecularIntensity * SpecularColor * max(pow(dotProduct, Shininess), 0) * diffuseIntensity;

   float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
   textureColor.a = 1;

   float4 lightingPosition = mul(input.WorldPos, LightViewProj);
   float2 ShadowTexCoord = 0.5f * lightingPosition.xy /
							lightingPosition.w + float2(0.5f, 0.5f);
   ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;
   float shadowColor = 1;
   float shadowDepth = tex2D(ShadowMapSampler, ShadowTexCoord).r;
   float ourDepth  = (lightingPosition.z / lightingPosition.w) - DepthBias;
   if(shadowDepth < ourDepth)
		shadowColor = 0.25f;

   return saturate((textureColor * (diffuseIntensity) + AmbientColor * AmbientIntensity + specular) * shadowColor);
}

technique CreateShadowMap
{
	pass Pass1
	{
		VertexShader = compile vs_2_0 CreateShadowMap_VertexShader();
		PixelShader = compile ps_2_0 CreateShadowMap_PixelShader();
	}
}

technique DrawWithShadowMap
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}