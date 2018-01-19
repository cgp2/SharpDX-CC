cbuffer Constants : register (b0)
{
	//Texture2D ShaderTexture;
	//SamplerState Sampler;
	float4x4 worldViewProj;
	float4x4 LightDiffuse;
	float4x4 LightAmbient;
};

Texture2D ShaderTexture : register(t0);
SamplerState Sampler : register(s0);
//
//cbuffer constantStruct
//{
//	Texture2D ShaderTexture : register(t0);
//	SamplerState Sampler : register(s0);
//	float4x4 worldViewProj;
//};


struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 text : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 text : TEXCOORD;
};
//
//float4x4 worldViewProj;

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(input.pos, worldViewProj);
	output.text = input.text.xy;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float4 color = ShaderTexture.Sample(Sampler, input.text.xy);
	return color;
}	