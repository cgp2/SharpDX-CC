struct TransformMatrices
{
    float4x4 World;
    float4x4 View;
    float4x4 Proj;
};

cbuffer MATRICESBUF : register(b0)
{
    TransformMatrices matrices;
};

Texture2D DiffuseMap : register(t0);
SamplerState samplLinear 
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
}; 

struct VS_IN
{
	float4 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

    float4x4 viewProj = mul(matrices.View, matrices.Proj);
    float4x4 worldViewProj = mul(matrices.World, viewProj);
	output.pos = mul(input.pos, worldViewProj);

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float depthValue = input.pos.z;
	float4 color = float4(depthValue, depthValue, depthValue, 1.0f);

	return color;
}