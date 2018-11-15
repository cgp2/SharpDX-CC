cbuffer LightPosition : register(b0)
{
    float4 LightPosition;
};

struct TransformMatrices
{
    float4x4 World;
    float4x4 View;
    float4x4 Proj;
};

cbuffer MATRICESBUF : register(b3)
{
    TransformMatrices matrices;
};

struct VS_IN
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float2 text : TEXCOORD;
};

struct GS_IN
{
	float4 pos : POSITION;
    float4 normal : NORMAL;
	float4 col : COLOR;
};


GS_IN VS(VS_IN input)
{
	GS_IN output = (GS_IN)0;

	output.pos = input.pos;
	output.col = float4(1, 1, 0, 0);
    output.normal = input.normal;

	return output;
}

[maxvertexcount(12)]
void GS(triangleadj GS_IN input[6], inout LineStream<GS_IN> stream)
{
	GS_IN p0 = input[0];
	GS_IN p1 = input[1];
	GS_IN p2 = input[2];
    GS_IN p3 = input[3];
    GS_IN p4 = input[4];
    GS_IN p5 = input[5];

    GS_IN temp = input[0];

    float3 dif1 = p2.pos - p4.pos;
    float3 dif2 = p2.pos - p0.pos;
    float3 norm1 = normalize(cross(dif1, dif2));
    //float3 v1 = normalize(p2.pos - LightPosition);
    float3 v1 = normalize(LightPosition);
    float angle1 = dot(v1, norm1);

    if(angle1 < 0)
    {
        dif1 = p1.pos - p2.pos;
        dif2 = p1.pos - p0.pos;
        norm1 = normalize(cross(dif1, dif2));
        v1 = normalize(p1.pos - LightPosition);
        angle1 = dot(v1, norm1);

        temp.pos = float4(dif1, 1);
        stream.Append(p1);
        stream.Append(p0);
      
        if (angle1 > 0)
        {
            stream.Append(p1);
            GS_IN t1 = p1;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
            stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(t2);
        }

        dif1 = p3.pos - p2.pos;
        dif2 = p3.pos - p4.pos;
        norm1 = normalize(cross(dif1, dif2));
      //  v1 = normalize(p3.pos - LightPosition);
        angle1 = dot(v1, norm1);

        if (angle1 >= 0)
        {
            stream.Append(p3);
            GS_IN t1 = p3;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
           // stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(p1);
        }

        dif1 = p5.pos - p0.pos;
        dif2 = p5.pos - p4.pos;
        norm1 = normalize(cross(dif1, dif2));
      //  v1 = normalize(p5.pos - LightPosition);
        angle1 = dot(v1, norm1);

        if (angle1 >= 0)
        {
            stream.Append(p5);
            GS_IN t1 = p5;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
            stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(t2);
        }
    }
    else
    {
        dif1 = p1.pos - p2.pos;
        dif2 = p1.pos - p0.pos;
        norm1 = normalize(cross(dif1, dif2));
      //  v1 = normalize(p1.pos - LightPosition);
        angle1 = dot(v1, norm1);

        if (angle1 < 0)
        {
            stream.Append(p1);
            GS_IN t1 = p1;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
            stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(t2);
        }

        dif1 = p3.pos - p2.pos;
        dif2 = p3.pos - p4.pos;
        norm1 = normalize(cross(dif1, dif2));
     //   v1 = normalize(p3.pos - LightPosition);
        angle1 = dot(v1, norm1);

        if (angle1 < 0)
        {
            stream.Append(p3);
            GS_IN t1 = p3;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
            stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(t2);
        }

        dif1 = p5.pos - p0.pos;
        dif2 = p5.pos - p4.pos;
        norm1 = normalize(cross(dif1, dif2));
    //    v1 = normalize(p5.pos - LightPosition);
        angle1 = dot(v1, norm1);

        if (angle1 < 0)
        {
            stream.Append(p5);
            GS_IN t1 = p5;
            t1.pos.w = 0;
            t1.pos = length(t1.pos) * t1.pos;
            stream.Append(t1);

            stream.Append(p2);
            GS_IN t2 = p2;
            t2.pos.w = 0;
            t2.pos = length(t2.pos) * t2.pos;
            stream.Append(t2);
        }
    }
   
    //stream.RestartStrip();
}
