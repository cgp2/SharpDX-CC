cbuffer View : register (b0)
{
	float4x4 View;
};

cbuffer Proj : register (b1)
{
	float4x4 Proj;
};

cbuffer World : register (b2)
{
	float4x4 World;
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
	float4 pos     : POSITION;
	//float4 normal  : NORMAL;
	//float2 tex      : TEXCOORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	//float2 tex  : TEXCOORD;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	float4x4 viewProj = mul(View, Proj);
	float4x4 worldViewProj = mul(World, viewProj);
	output.pos = mul(input.pos, worldViewProj);
	//output.tex = input.tex.xy;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	float depthValue;
	float4 color;

	depthValue = input.pos.z;
	color = float4(depthValue, depthValue, depthValue, 1.0f);

	/*color = float4(100, 100, 0, 1.0f);*/

	return color;
}


//void PS(PS_IN input)
//{
//	float4 diffuse = DiffuseMap.Sample(samplLinear, input.tex);
//
//	// Don't write transparent pixels to the shadow map.
//	clip(diffuse.a - 0.15f);
//}

//RasterizerState Depth
//{
//	DepthBias = 10000;
//	DepthBiasClamp = 0.0f;
//	SlopeScaledDepthBias = 1.0f;
//};