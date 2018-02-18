using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Diagnostics;
using SharpDX.Windows;
using System.Windows.Forms;
using SharpDX.Components;

namespace SharpDX.Games
{
    class PingPong : Game, IDisposable
    {
        Texture2D backBuffer = null;

        private Stopwatch clock = new Stopwatch();
        private Direct3D11.InputElement[] inputElements = new Direct3D11.InputElement[]
        {
            new Direct3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new Direct3D11.InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
        };
        protected Direct3D11.Buffer constantBuffer;
        protected Texture2D depthBuffer = null;
        protected DepthStencilView depthView = null;

        BoxComponent box;

        private float boxUpBorder;
        private float boxBottomBorder;
        private float boxLeftBorder;
        private float boxRightBorder;

        BarComponent barBottom;
        BarComponent barTop;
        CircleComponent crcl1;

        private Vector3 circleMovementDirection;

        public PingPong()
        {
            InitializeShaders();
            renderForm.Name = "PingPong";

            Random Rd = new Random();
            float x = Rd.NextFloat(0.3f, 5f);
            float y = Rd.NextFloat(1f, 5f);

            circleMovementDirection = new Vector3(x, y, 0f);
        }

        new protected void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "VS", "vs_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                vertexShader = new Direct3D11.VertexShader(device, vertexShaderByteCode);
            }

            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniCube.fx", "PS", "ps_5_0", ShaderFlags.PackMatrixRowMajor))
            {
                pixelShader = new Direct3D11.PixelShader(device, pixelShaderByteCode);
            }

            deviceContext.VertexShader.Set(vertexShader);
            deviceContext.PixelShader.Set(pixelShader);


            inputLayoutMain = new InputLayout(device, inputSignature, inputElements);
            deviceContext.InputAssembler.InputLayout = inputLayoutMain;


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
            deviceContext.VertexShader.SetConstantBuffer(0, constantBuffer);

            deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height, 0.0f, 1.0f));
            deviceContext.OutputMerger.SetTargets(depthView, renderTargetView);

            box = new BoxComponent(device);
            boxUpBorder = box.GlobalVertices[0].Y - 0.01f;
            boxBottomBorder = box.GlobalVertices[4].Y + 0.01f;
            boxLeftBorder = box.GlobalVertices[8].X + 0.01f;
            boxRightBorder = box.GlobalVertices[12].X - 0.01f;

            barBottom = new BarComponent(device);
            barBottom.InitialPosition = new Vector3(0f, 0.1f, 0f);
            barBottom.Update();

            barTop = new BarComponent(device);
            barTop.Rotation = Matrix.RotationYawPitchRoll(MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(0), MathUtil.DegreesToRadians(180));
            barTop.InitialPosition = new Vector3(0f, 3.9f, 0f);
            barTop.Update();

            crcl1 = new CircleComponent(device);
            crcl1.InitialPosition = new Vector3(0f, 2f, 0f);
            //crcl1.Translation = Matrix.Translation(crcl1.WorldPosition);
            crcl1.Update();


            clock.Start();
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            var worldViewProj = viewProj;
            deviceContext.UpdateSubresource(ref worldViewProj, constantBuffer);

            Draw();
        }

        bool isGoalHit = false;
        public void Draw()
        {
            var viewProj = Matrix.Multiply(Camera.View, Camera.Proj);
            if (isGoalHit == true)
            {
                var time = clock.ElapsedMilliseconds;
                while (time + 1000 > clock.ElapsedMilliseconds) { }
                Random Rd = new Random();
                float x = Rd.NextFloat(0.3f, 5f);
                float y = Rd.NextFloat(1f, 5f);

                circleMovementDirection = new Vector3(x, y, 0f);

                isGoalHit = false;
            }

            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

            box.Draw(deviceContext, Camera.Proj, Camera.View, true);
            barBottom.Draw(deviceContext, Camera.Proj, Camera.View, true);
            barTop.Draw(deviceContext, Camera.Proj, Camera.View, true);

            CheckCircleOverlap();
            ChechDirection();

            crcl1.InitialPosition += circleMovementDirection * 0.01f;

            crcl1.Translation = crcl1.InitialPosition - new Vector3(0,2,0);
            //crcl1.Translation = Matrix.Translation(crcl1.WorldPosition);         
            crcl1.Draw(deviceContext, Camera.Proj, Camera.View, true);

            swapChain.Present(1, PresentFlags.None);
        }

        private void ChechDirection()
        {
            float mult = 0.5f * (barTop.GlobalVertices[0].Y - crcl1.InitialPosition.Y);

            var crossingPoint = Vector3.Multiply(circleMovementDirection, mult);

            float topBarBottom = barTop.GlobalVertices[0].Y;
            float topBarLeft = barTop.InitialPosition.X - 0.2f; 
            float topBarRight = barTop.InitialPosition.X + 0.2f;

            if (crossingPoint.X >= topBarRight && topBarRight < boxRightBorder)
            {
                barTop.InitialPosition += new Vector3(0.05f, 0f, 0f);
                barTop.Translation = barTop.InitialPosition - new Vector3(0f, 3.9f, 0f);
            }

            else if  (crossingPoint.X <= topBarLeft && topBarLeft > boxLeftBorder)
            {
                barTop.InitialPosition += new Vector3(-0.05f, 0f, 0f);
                barTop.Translation = barTop.InitialPosition - new Vector3(0f, 3.9f, 0f);
            }
        }

        private void CheckCircleOverlap()
        {
            //Box overlapping
            if (crcl1.InitialPosition.Y + crcl1.Radius >= boxUpBorder)
            {
                isGoalHit = true;

                crcl1.InitialPosition = new Vector3(0f, 2f, 0f);
                crcl1.Update();

                Random rd = new Random();
                var x = rd.NextFloat(0.4f, 5f);
                var y = rd.NextFloat(0.4f, 5f);
                circleMovementDirection = new Vector3(x, y, 0f);

                barTop.InitialPosition = new Vector3(0f, 3.9f, 0f);
                barTop.Update();
                barBottom.InitialPosition = new Vector3(0f, 0.1f, 0f);
                barBottom.Update();
            }
            else if (crcl1.InitialPosition.Y - crcl1.Radius <= boxBottomBorder)
            {
                isGoalHit = true;

                crcl1.InitialPosition = new Vector3(0f, 2f, 0f);
                crcl1.Update();

                Random rd = new Random();
                var x = rd.NextFloat(0.4f, 5f);
                var y = rd.NextFloat(0.4f, 5f);
                circleMovementDirection = new Vector3(x, y, 0f);

                barTop.InitialPosition = new Vector3(0f, 3.9f, 0f);
                barTop.Update();
                barBottom.InitialPosition = new Vector3(0f, 0.1f, 0f);
                barBottom.Update();

            }
            else if (crcl1.InitialPosition.X + crcl1.Radius >= boxRightBorder)
            {
                circleMovementDirection = Vector3.Reflect(circleMovementDirection, Vector3.Left);
                circleMovementDirection = Vector3.Multiply(circleMovementDirection, 1.02f);
            }
            else if (crcl1.InitialPosition.X - crcl1.Radius <= boxLeftBorder)
            {
                circleMovementDirection = Vector3.Reflect(circleMovementDirection, Vector3.Right);
                circleMovementDirection = Vector3.Multiply(circleMovementDirection, 1.02f);
            }

            //Bars overlap
            float bottomBarTop = barBottom.GlobalVertices[0].Y + 0.02f;
            float bottomBarLeft = barBottom.InitialPosition.X - 0.2f;
            float bottomBarRight = barBottom.InitialPosition.X + 0.2f;

            float topBarBottom = barTop.GlobalVertices[0].Y + 0.02f;
            float topBarLeft = barTop.InitialPosition.X + 0.2f;
            float topBarRight = barTop.InitialPosition.X - 0.2f;

            if ((crcl1.InitialPosition.Y - crcl1.Radius <= bottomBarTop && (crcl1.InitialPosition.X - crcl1.Radius <= bottomBarRight) &&
                 crcl1.InitialPosition.X + crcl1.Radius >= bottomBarLeft))
            {
                circleMovementDirection = Vector3.Reflect(circleMovementDirection, Vector3.Up);
                circleMovementDirection = Vector3.Multiply(circleMovementDirection, 1.05f);
            }
            else if ((crcl1.InitialPosition.Y + crcl1.Radius >= topBarBottom && (crcl1.InitialPosition.X - crcl1.Radius <= topBarLeft) &&
                 crcl1.InitialPosition.X + crcl1.Radius >= topBarRight))
            {
                circleMovementDirection = Vector3.Reflect(circleMovementDirection, Vector3.Up);
                circleMovementDirection = Vector3.Multiply(circleMovementDirection, 1.05f);
            }

        }
        public override void KeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    if (barBottom.GlobalVertices[8].X - 0.03 >= boxLeftBorder)
                    {
                        //barBottom.Translation += Matrix.Translation(-0.06f, 0f, 0f);
                        barBottom.InitialPosition += new Vector3(-0.06f, 0f, 0f);
                        barBottom.Update();
                    }
                    break;
                case Keys.D:
                    if (barBottom.GlobalVertices[12].X + 0.03 <= boxRightBorder)
                    {
                        //barBottom.Translation += Matrix.Translation(0.06f, 0f, 0f);
                        barBottom.InitialPosition += new Vector3(0.06f, 0f, 0f);
                        barBottom.Update();
                    }
                    break;
            }
        }

        public void Dispose()
        {
            DisposeBase();
            constantBuffer.Dispose();
            depthBuffer.Dispose();
            depthView.Dispose();
        }

        public override void MouseMoved(float x, float y)
        {
            throw new NotImplementedException();
        }
    }
}
