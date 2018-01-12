//Texture2D ShaderTexture : register(t0);
//SamplerState Sampler : register(s0);

cbuffer Constants
{
	float4x4 worldViewProj;
	Texture2D ShaderTexture : register(t0);
	SamplerState Sampler : register(s0);
}


struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 text : TEXCOORD0
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 text : TEXCOORD0
};

float4x4 worldViewProj;


PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(input.pos, worldViewProj);
	output.text = input.text;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return ShaderTexture.Sample(Sampler, input.TextureUV);
}