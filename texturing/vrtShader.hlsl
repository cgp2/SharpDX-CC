struct VSOut 
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

VSOut main(float4 position : POSITION, float4 color : COLOR) : SV_POSITION
{
	VSOut ret;
	ret.position = position;
	ret.color = color;

	return ret;
}

