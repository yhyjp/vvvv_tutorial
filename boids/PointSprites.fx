////////////////////////////////////////////////////////////////////////////////
//     Point sprite renders a screen aligned quad for every vertex streamed to
//     the shader, thus minimizing cpu cycles and  drawing calls to the gpu.
//     This technique is mostly used for rendering particles. Sprite size (in pixels)
//     is calculated from Projection Matrix and ViewportHeight(optional).
//     The maximum size is usualy 256 px on newer gpus.
//     This shader is based on Template.fx, all the default comments are deleted
//     so you can easily find how to implement this technique. By Viktor Vicsek 2008
////////////////////////////////////////////////////////////////////////////////

float4x4 tW: WORLD;
float4x4 tV: VIEW;
float4x4 tP: PROJECTION;
float4x4 tWVP: WORLDVIEWPROJECTION;
float4 color : COLOR;

//texture
texture Tex <string uiname="Texture";>;
sampler Samp = sampler_state
{
    Texture   = (Tex);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};
//float4x4 tTex: TEXTUREMATRIX <string uiname="Texture Transform";>;

/////this will define the size of the quads in pixels if calcPerspective is false
float Size  <string uiname="Sprite Size";>;
/////connect this input to the BackBufferHeight output of the renderer
float ViewportHeight;
/////decide if you want perspective
bool calcPerspective<string uiname="Calculate Scale From Perspective";>;
//yet to be done: allow rotating the TexCoords coz the quads cant be rotated
//float rotation<string uiname="Rotate Texcoords";>;

struct vs2ps
{
    float4 Pos  : POSITION;
    float2 TexCd : TEXCOORD0;
    float Size : PSIZE0;  //////////////////  this allows usage of pointsize
};

// --------------------------------------------------------------------------------------------------
// VERTEXSHADERS
// --------------------------------------------------------------------------------------------------
vs2ps VS(
    float4 PosO  : POSITION,
    float4 TexCd : TEXCOORD0)
{

    vs2ps Out = (vs2ps)0;


    Out.Pos = mul(PosO, tWVP);
    
    //transform texturecoordinates --- this should be put to pixelshader
    Out.TexCd = TexCd;
    if(calcPerspective){
                        Out.Size=Size * tP / Out.Pos.w * ViewportHeight/2;
                        }else{
                        Out.Size=Size;
                        }
    return Out;
}

// --------------------------------------------------------------------------------------------------
// PIXELSHADERS:
// --------------------------------------------------------------------------------------------------

float4 PS(vs2ps In): COLOR
{
    ///todo: fix texcoord rotation
    //float2 newTexCd=(-0.5,-0.5);//  In.TexCd-0.5;////needed to rotate from center
    //newTexCd=mul(newTexCd,tTex); ////actual rotation
    //newTexCd*=sqrt(2);  ////scale, so corners dont get lost
    //newTexCd+= 0.5;      ////return to origin
    float4 col = color * tex2D(Samp, In.TexCd);
    return col;
}

// --------------------------------------------------------------------------------------------------
// TECHNIQUES:
// --------------------------------------------------------------------------------------------------

technique TPointSprite
{
    pass P0
    {
        /////the next 3 statements are important:
        FillMode = POINT;
        PointScaleEnable = true;
        PointSpriteEnable = true;
        //AlphaBlendEnable = true;
        VertexShader = compile vs_1_0 VS();
        PixelShader  = compile ps_1_0 PS();
    }
}
