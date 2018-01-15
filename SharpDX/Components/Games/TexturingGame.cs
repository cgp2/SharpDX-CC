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
        protected Direct3D11.Buffer constantBuffer;
        protected Texture2D depthBuffer = null;
        protected DepthStencilView depthView = null;

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

        List<AbstractComponent> components = new List<AbstractComponent>();
        List<AbstractComponent> collectedComponents = new List<AbstractComponent>();

        public TexturingGame()
        {
            InitializeShaders();
        }

        new protected void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("TextureShaders.hlsl", "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("TextureShaders.hlsl", "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);

            inputLayout = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayout;

            constantBuffer = new Direct3D11.Buffer(device, Utilities.SizeOf<Matrix>(), Direct3D11.ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

            renderTargetView = new RenderTargetView(device, backBuffer);
            depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = renderForm.ClientSize.Width,
                Height = renderForm.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });


            depthView = new DepthStencilView(device, depthBuffer);
        }

        public void Run()
        {
            deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
            deviceContext.OutputMerger.SetTargets(depthView, renderTargetView);

            trg = new TriangleComponent(device);
            trg.InitialPosition = new Vector3(-5, 4f, 0);
            trg.Update();
            //trg.Scaling = new Vector3(2f, 2f, 2f);
            trg.RotationCenter = trg.InitialPosition;      
          //  components.Add(trg);

            cube1 = new CubeComponentTextured(device, "1t.png");
            cube1.InitialPosition = new Vector3(5f, 4f, 2f);
            cube1.RotationCenter = cube1.InitialPosition;
            //cube1.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube1.Update();
            components.Add(cube1);

            cube2 = new CubeComponentTextured(device, "2.png");
            cube2.InitialPosition = new Vector3(10f, 4f, -12f);
            cube2.RotationCenter = cube2.InitialPosition;
       //  cube2.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube2.Update();
            components.Add(cube2);

            cube3 = new CubeComponentTextured(device, "3t.png");
            cube3.InitialPosition = new Vector3(-17f, 4f, 5f);
            cube3.RotationCenter = cube3.InitialPosition;
        ///   cube3.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube3.Update();
            components.Add(cube3);

            cube4 = new CubeComponentTextured(device, "2.jpg");
            cube4.InitialPosition = new Vector3(22f, 4f, 33f);
            cube4.RotationCenter = cube4.InitialPosition;
      //   cube4.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube4.Update();
            components.Add(cube4);

            cube5 = new CubeComponentTextured(device, "3t.png");
            cube5.InitialPosition = new Vector3(-12f, 4f, -20f);
            cube5.RotationCenter = cube5.InitialPosition;
        //  cube5.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube5.Update();
            components.Add(cube5);

            cube6 = new CubeComponentTextured(device, "0t.png");
            cube6.InitialPosition = new Vector3(40f, 4f, -2f);
            cube6.RotationCenter = cube6.InitialPosition;
        //    cube6.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube6.Update();
            components.Add(cube6);

            cube7 = new CubeComponentTextured(device, "download.png");
            cube7.InitialPosition = new Vector3(17f, 4f, -30f);
            cube7.RotationCenter = cube7.InitialPosition;
        //  cube7.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube7.Update();
            //components.Add(cube7);

            cube8 = new CubeComponentTextured(device, "text.png");
            cube8.InitialPosition = new Vector3(2f, 4f, 26f);
            cube8.RotationCenter = cube8.InitialPosition;
       //    cube8.Scaling = new Vector3(0.3f, 0.3f, 0.3f);
            cube8.Update();
            components.Add(cube8);

            grid = new GridComponentTextured(device);
            grid.InitialPosition = new Vector3(-50f, 0f, -50f);
            grid.Update();

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location).ToString() + "\\Cow.obj";
            cow = new ObjModelComponent(device, path, "download.png");
            cow.Translation = new Vector3(0f, 3f, 0f);
            cow.RotationCenter += new Vector3(0, 0.7f, -0.2f);
            cow.Scaling = new Vector3(0.002f, 0.002f, 0.002f);
            cow.Update();
            //cow.Rotation = Matrix.RotationY(MathUtil.DegreesToRadians(90f));
            //cow.Translation = new Vector3(1.2f, 0f, 0f);

            location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = Path.GetDirectoryName(location).ToString() + "\\Toilet.obj";
            toil = new ObjModelComponent(device, path, "2.png");
            toil.Scaling = new Vector3(0.02f, 0.02f, 0.02f);
            toil.Rotation = Matrix.RotationY(MathUtil.DegreesToRadians(180f));
            toil.Translation = new Vector3(2f, 2f, 0f);
            toil.Update();
            toil.RotationCenter = toil.InitialPosition;
            components.Add(toil);

            sphere = new SphereComponent(device, 3, 16);
            sphere.InitialPosition = new Vector3(0f, 2f, 0f);
            sphere.RotationCenter += new Vector3(0, 3f, 0f);
            sphere.Update();

            clock.Start();
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer, 0);

            Draw();
        }

        public void Draw()
        {
            CheckOVerlap();         
            var time = clock.ElapsedMilliseconds / 1000f;

            //cube.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), (float)Math.Cos(time / 2), (float)Math.Cos(time / 2));

            //trg.Rotation = Matrix.RotationYawPitchRoll((float)Math.Sin(time), 0, 0);

            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);

            trg.Draw(deviceContext, viewProj, constantBuffer);
            cube1.Draw(deviceContext, viewProj, constantBuffer);
            cube2.Draw(deviceContext, viewProj, constantBuffer);
            cube3.Draw(deviceContext, viewProj, constantBuffer);
            cube4.Draw(deviceContext, viewProj, constantBuffer);
            cube5.Draw(deviceContext, viewProj, constantBuffer);
            cube6.Draw(deviceContext, viewProj, constantBuffer);
            cube7.Draw(deviceContext, viewProj, constantBuffer);
            cube8.Draw(deviceContext, viewProj, constantBuffer);
            grid.Draw(deviceContext, viewProj, constantBuffer);

            toil.Draw(deviceContext, viewProj, constantBuffer);
            cow.Draw(deviceContext, viewProj, constantBuffer);

            sphere.Draw(deviceContext, viewProj, constantBuffer);

            swapChain.Present(1, PresentFlags.None);
        }

        public override void KeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.W:
                    Camera.MoveForward();
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
                    Camera.MoveBackward();
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
                    Camera.MoveLeft();
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
                    Camera.MoveRight();
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
            var UpBorder = sphere.WorldPosition.Y + sphere.Radius;
            var DownBorder = sphere.WorldPosition.Y - sphere.Radius;
            var FrontBorder = sphere.WorldPosition.Z + sphere.Radius;
            var BackBorder = sphere.WorldPosition.Z - sphere.Radius;
            foreach (AbstractComponent comp in components)
            {
                bool isOverlapped = false;
                foreach (VertexPositionNormalTexture vertex in comp.t)
                {
                    var vert = Vector4.Transform(vertex.Position, comp.transform); 
                    if (vertex.Position.X >= leftBorder && vertex.Position.X <= rightBorder && vertex.Position.Y >= DownBorder && vertex.Position.Y <= UpBorder && vertex.Position.Z >= BackBorder && vertex.Position.Z <= FrontBorder)
                    {
                        var t1 = (float)GetRandomNumber(-1, 1);
                        var t2 = (float)GetRandomNumber(-1, 1);
                        var t3 = (float)GetRandomNumber(-1, 1);
                        Console.WriteLine("OVERLAPPING");

                        comp.t = comp.initialVertices;
                        comp.InitialPosition = (Vector3)Vector3.Transform(sphere.InitialPosition, sphere.transform);
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
                        components.Remove(comp);
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
            Camera.ChangeTargetPosition(x, y);
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public void Dispose()
        {
            DisposeBase();
            constantBuffer.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
        }
    }
}

