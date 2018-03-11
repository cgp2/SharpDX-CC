using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Windows;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Components;
using System.IO;

namespace SharpDX.Games
{
    class TexturingGame : Game, IDisposable
    {
        Texture2D backBuffer = null;

        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 32, 0)
        };
        protected Direct3D11.Buffer ConstantBuffer;
        protected Texture2D DepthBuffer = null;
        protected DepthStencilView DepthView = null;

        TriangleComponent trg;

        CubeComponentTextured cube1;
        CubeComponentTextured cube2;
        CubeComponentTextured cube3;
        CubeComponentTextured cube4;
        CubeComponentTextured cube5;
        CubeComponentTextured cube6;
        CubeComponentTextured cube7;
        CubeComponentTextured cube8;

        GridComponentTextured grid;
        ObjModelComponent toil;
        ObjModelComponent cow;
        SphereComponent sphere;
    
        List<AbstractComponent> collectedComponents = new List<AbstractComponent>();

        public TexturingGame()
        {
            InitializeShaders();
            
            Camera = new CameraComponent((float)RenderForm.Width / RenderForm.Height, new Vector3(0, 6, -3), 180f, -30f);
        }

        new protected void InitializeShaders()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Shaders\\TextureShaders.hlsl";
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(path, "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                InputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                VertexShader = new Direct3D11.VertexShader(GameDevice, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(path, "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                PixelShader = new Direct3D11.PixelShader(GameDevice, pixelShaderByteCode);
            }

            DeviceContext.VertexShader.Set(VertexShader);
            DeviceContext.PixelShader.Set(PixelShader);

            InputLayoutMain = new InputLayout(GameDevice, InputSignature, inputElements);
            DeviceContext.InputAssembler.InputLayout = InputLayoutMain;

            ConstantBuffer = new Direct3D11.Buffer(GameDevice, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);

            RenderTargetView = new RenderTargetView(GameDevice, backBuffer);
            DepthBuffer = new Texture2D(GameDevice, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = RenderForm.ClientSize.Width,
                Height = RenderForm.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            DepthView = new DepthStencilView(GameDevice, DepthBuffer);
        }

        public void Run()
        {
            DeviceContext.Rasterizer.SetViewport(new Viewport(0, 0, RenderForm.ClientSize.Width, RenderForm.ClientSize.Height, 0.0f, 1.0f));
            DeviceContext.OutputMerger.SetTargets(DepthView, RenderTargetView);

            trg = new TriangleComponent(GameDevice);
            trg.InitialPosition = new Vector3(-5, 4f, 0);
            trg.Update();
            //trg.Scaling = new Vector3(2f, 2f, 2f);
            trg.RotationCenter = trg.InitialPosition;      
            Components.Add(trg);

            cube1 = new CubeComponentTextured(GameDevice, "1t.png");
            cube1.InitialPosition = new Vector3(5f, 4f, 2f);
            cube1.RotationCenter = cube1.InitialPosition;
            //cube1.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube1.Update();
            Components.Add(cube1);

            cube2 = new CubeComponentTextured(GameDevice, "2.png");
            cube2.InitialPosition = new Vector3(10f, 4f, -12f);
            cube2.RotationCenter = cube2.InitialPosition;
       //  cube2.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube2.Update();
            Components.Add(cube2);

            cube3 = new CubeComponentTextured(GameDevice, "3t.png");
            cube3.InitialPosition = new Vector3(-17f, 4f, 5f);
            cube3.RotationCenter = cube3.InitialPosition;
        ///   cube3.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube3.Update();
            Components.Add(cube3);

            cube4 = new CubeComponentTextured(GameDevice, "2.jpg");
            cube4.InitialPosition = new Vector3(22f, 4f, 33f);
            cube4.RotationCenter = cube4.InitialPosition;
      //   cube4.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube4.Update();
            Components.Add(cube4);

            cube5 = new CubeComponentTextured(GameDevice, "download.png");
            cube5.InitialPosition = new Vector3(-12f, 4f, -20f);
            cube5.RotationCenter = cube5.InitialPosition;
        //  cube5.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube5.Update();
            Components.Add(cube5);

            cube6 = new CubeComponentTextured(GameDevice, "0t.png");
            cube6.InitialPosition = new Vector3(40f, 4f, -2f);
            cube6.RotationCenter = cube6.InitialPosition;
        //    cube6.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube6.Update();
            Components.Add(cube6);

            cube7 = new CubeComponentTextured(GameDevice, "download.png");
            cube7.InitialPosition = new Vector3(17f, 4f, -30f);
            cube7.RotationCenter = cube7.InitialPosition;
        //  cube7.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube7.Update();
            Components.Add(cube7);

            cube8 = new CubeComponentTextured(GameDevice, "text.png");
            cube8.InitialPosition = new Vector3(2f, 4f, 26f);
            cube8.RotationCenter = cube8.InitialPosition;
       //    cube8.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube8.Update();
            Components.Add(cube8);

            grid = new GridComponentTextured(GameDevice,1, "1t.png");
            grid.InitialPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Cow.obj";
            cow = new ObjModelComponent(GameDevice, path, "download.png");
            cow.Translation = new Vector3(0f, 3f, 0f);
            cow.RotationCenter += new Vector3(0, 0.7f, -0.2f);
            cow.Scaling = new Vector3(0.05f, 0.002f, 0.002f);
            cow.Update();
            //cow.Rotation = Matrix.RotationY(MathUtil.DegreesToRadians(90f));
            //cow.Translation = new Vector3(1.2f, 0f, 0f);

            location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(location).ToString() + "\\Toilet.obj";
            toil = new ObjModelComponent(GameDevice, path, "2.png");
            toil.Scaling = new Vector3(0.2f, 0.02f, 0.02f);
            toil.Rotation = Matrix.RotationY(MathUtil.DegreesToRadians(180f));
            toil.Translation = new Vector3(10f, 2f, 0f);
            toil.Update();
            toil.RotationCenter = toil.InitialPosition;
            Components.Add(toil);

            sphere = new SphereComponent(GameDevice, 3, 16);
            sphere.InitialPosition = new Vector3(0f, 2f, 0f);
            sphere.RotationCenter += new Vector3(0, 3f, 0f);
            sphere.Update();

            clock.Start();
            RenderLoop.Run(RenderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            DeviceContext.UpdateSubresource(ref worldViewProj, ConstantBuffer, 0);

            Draw();
        }

        public void Draw()
        {
            CheckOVerlap();         
            var time = clock.ElapsedMilliseconds / 1000f;

            //cube.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), (float)Math.Cos(time / 2), (float)Math.Cos(time / 2));

            //trg.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), 0, 0);

            DeviceContext.ClearDepthStencilView(DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            DeviceContext.ClearRenderTargetView(RenderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);

            trg.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube1.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube2.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube3.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube4.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube5.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube6.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube7.Draw(DeviceContext, Camera.Proj, Camera.View);
            cube8.Draw(DeviceContext, Camera.Proj, Camera.View);
            grid.Draw(DeviceContext, Camera.Proj, Camera.View);

            toil.Draw(DeviceContext, Camera.Proj, Camera.View);
            cow.Draw(DeviceContext, Camera.Proj, Camera.View);

            sphere.Draw(DeviceContext, Camera.Proj, Camera.View);

            SwapChain.Present(1, PresentFlags.None);
        }

        public override void KeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    Camera.MoveForwardAxis();
                    cow.Rotation *= Matrix.RotationYawPitchRoll(0, 0.3f, 0);
                    cow.Translation += new Vector3(0, 0, 1.3f);
                    sphere.Rotation *= Matrix.RotationYawPitchRoll(0, 0.3f, 0);
                    sphere.Translation += new Vector3(0, 0, 1.3f);
                    foreach (AbstractComponent comp in collectedComponents)
                    {
                        comp.Rotation *= Matrix.RotationYawPitchRoll(0.2f, 0.3f, 0);
                        comp.Translation += new Vector3(0, 0, 1.3f);
                    }
                    break;
                case Keys.S:
                    Camera.MoveBackwardAxis();
                    cow.Rotation *= Matrix.RotationYawPitchRoll(0, -0.3f, 0);
                    cow.Translation += new Vector3(0, 0, -1.3f);
                    sphere.Rotation *= Matrix.RotationYawPitchRoll(0, -0.3f, 0);
                    sphere.Translation += new Vector3(0, 0, -1.3f);
                    foreach (AbstractComponent comp in collectedComponents)
                    {
                       comp.Rotation *= Matrix.RotationYawPitchRoll(0.2f, 0.3f, 0);
                       comp.Translation += new Vector3(0, 0, -1.3f);
                    }
                    break;
                case Keys.A:
                    Camera.MoveLeftAxis();
                    cow.Rotation *= Matrix.RotationYawPitchRoll(0, 0, 0.3f);
                    cow.Translation += new Vector3(-1.3f, 0, 0);
                    sphere.Rotation *= Matrix.RotationYawPitchRoll(0, 0, 0.3f);
                    sphere.Translation += new Vector3(-1.3f, 0, 0);
                    foreach (AbstractComponent comp in collectedComponents)
                    {
                        comp.Rotation *= Matrix.RotationYawPitchRoll(0.2f, 0.3f, 0);
                        comp.Translation += new Vector3(-1.3f, 0, 0);
                    }
                    break;
                case Keys.D:
                    Camera.MoveRightAxis();
                    cow.Rotation *= Matrix.RotationYawPitchRoll(0, 0, -0.3f);
                    cow.Translation += new Vector3(1.3f, 0, 0);
                    sphere.Rotation *= Matrix.RotationYawPitchRoll(0, 0, -0.3f);
                    sphere.Translation += new Vector3(1.3f, 0, 0);
                    foreach (AbstractComponent comp in collectedComponents)
                    {           
                        comp.Rotation *= Matrix.RotationYawPitchRoll(0.2f, 0.3f, 0);
                        comp.Translation += new Vector3(1.3f, 0, 0);
                    }
                    break;
                case Keys.Escape:
                    Dispose();
                    break;
            }
        }

        public void CheckOVerlap()
        {
            var leftBorder = sphere.WorldPosition.X - sphere.Radius;
            var rightBorder = sphere.WorldPosition.X + sphere.Radius;
            var upBorder = sphere.WorldPosition.Y + sphere.Radius;
            var downBorder = sphere.WorldPosition.Y - sphere.Radius;
            var frontBorder = sphere.WorldPosition.Z + sphere.Radius;
            var backBorder = sphere.WorldPosition.Z - sphere.Radius;
            foreach (AbstractComponent comp in Components)
            {
                bool isOverlapped = false;
                foreach (VertexPositionNormalTexture vertex in comp.Vertices)
                {
                    var vert = Vector4.Transform(vertex.Position, comp.Transform); 
                    if (vertex.Position.X >= leftBorder && vertex.Position.X <= rightBorder && vertex.Position.Y >= downBorder && vertex.Position.Y <= upBorder && vertex.Position.Z >= backBorder && vertex.Position.Z <= frontBorder)
                    {
                        var t1 = (float)GetRandomNumber(-1, 1);
                        var t2 = (float)GetRandomNumber(-1, 1);
                        var t3 = (float)GetRandomNumber(-1, 1);
                        Console.WriteLine("OVERLAPPING");

                        comp.Vertices = comp.InitialVertices;
                        comp.InitialPosition = (Vector3)Vector3.Transform(sphere.InitialPosition, sphere.Transform);
                        comp.Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
                        comp.Translation = new Vector3(0f, 0f, 0f);
                        //comp.RotationCenter += new Vector3(0, 3f, 0f);
                        comp.RotationCenter += new Vector3(4f, 0f, 1f);
                        comp.Update();

                        collectedComponents.Add(comp);
                        //comp.Translation += new Vector3((sphere.WorldPosition - comp.WorldPosition).X + t1*sphere.Radius / 2, (sphere.WorldPosition - comp.WorldPosition).Y + t2 * sphere.Radius / 2, (sphere.WorldPosition - comp.WorldPosition).Z + t3*sphere.Radius / 2);
                        //comp.RotationCenter = (Vector3)(sphere.WorldPosition - comp.WorldPosition);
                        //comp.Rotation = Matrix.RotationYawPitchRoll(0.0f, 0.0f, 0.0f);
                        //comp.Scaling = new Vector3(1f, 1f, 1f);
                        //comp.InitialPosition = (Vector3)comp.WorldPosition;
                        //comp.Update();
                        Components.Remove(comp);
                        isOverlapped = true;
                      
                        //comp.Translation += new Vector3(50f, 0f, 50f);
                        break;
                    }
                }
                if(isOverlapped)
                    break;
            }
        }

        public void MovementForCollectedComponents()
        {
            foreach(AbstractComponent comp in collectedComponents)
            {
                comp.Rotation *= Matrix.RotationYawPitchRoll(0, 0, -0.3f);
                comp.Translation += new Vector3(1.3f, 0, 0);
            }
        }

        public override void MouseMoved(float x, float y)
        {
            //Camera.ChangeTargetPosition(x, y);
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public void Dispose()
        {
            foreach (AbstractComponent comp in Components)
                comp.Dispose();    
            DisposeBase();
            ConstantBuffer.Dispose();
            DepthBuffer.Dispose();
            DepthView.Dispose();

        }
    }
}

