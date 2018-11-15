struct GS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
};

struct VS_IN
{
    float4 pos : POSITION;
    float4 normal : NORMAL;
    float2 text : TEXCOORD;
};

GS_IN VS(VS_IN input)
{
    GS_IN output = (GS_IN) 0;

    output.pos = input.pos;
    output.col = float4(1, 1, 0, 0);

    return output;
}

//[maxvertexcount(2)]
//void GS(line GS_IN input[2], inout LineStream<GS_IN> stream)
//{
//    GS_IN p = input[0];

//	GS_IN p0 = p;
//	GS_IN p1 = input[1];
//	GS_IN p2 = p;

//	//p1.pos += float4(2, 0, 0, 0);
//	//p2.pos += float4(0, 2, 0, 0);

//	stream.Append(p0);
//	stream.Append(p1);
//	//stream.Append(p2);

//	stream.RestartStrip();
//}

[maxvertexcount(4)]
void GS(triangleadj GS_IN input[6], inout LineStream<GS_IN> stream)
{
    GS_IN p0 = input[0];
    GS_IN p1 = input[0];
    GS_IN p2 = input[2];
    GS_IN p3 = input[3];
    GS_IN p4 = input[4];
    GS_IN p5 = input[5];

    float4 dif1 = p1.pos - p3.pos;
    float4 dif2 = p5.pos - p1.pos;

    float4 norm1 = dif1 * dif2;
    norm1 = normalize(norm1);
    //float4 v1 = LightPosition - p1.pos;
    //v1 = normalize(v1);

    //float angle1 = acos(dot(v1, norm1));

    //[branch]
    //    if (angle1 < 90)
    //{
    //    dif1 = p0.pos - p1.pos;
    //    dif2 = p0.pos - p5.pos;

    //    float4 norm1 = dif1 * dif2;
    //    norm1 = normalize(norm1);
    //    //float4 v1 = LightPosition - p1.pos;
    //    //v1 = normalize(v1);
    //    //float angle1 = acos(dot(v1, norm1));
    //    }
    //else
    //{

    //}
    

    p0.pos.w = 0;
    p0.pos = length(p0.pos) * p0.pos;
  //  p1.pos = p0.pos / 0;
    //p2.pos += float4(0, 2, 0, 0);

    stream.Append(p0);
    stream.Append(p1);

    stream.RestartStrip();
}