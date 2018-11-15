using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Games;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;


namespace SharpDX.Components
{
    class ConsoleComponent : IDisposable
    {
        public delegate void CommandFucntionDelegate();

        private Factory d2dFactory;
        private RenderTarget d2dRenderTarget;
        private ShadowMapLightingGame gameInst;
        private SolidColorBrush TextRectangleBrush;
        private SolidColorBrush LayoutBrush;
        private Brush TextBrush;
        private Geometry TextRect;
        private Geometry LayoutRect;
        private TextLayout ConsoleLayout;
        private TextLayout HistoryLayout;
        private DirectWrite.Factory FontFactory;
        public bool IsEnabled = false;

        TextFormat consoleTextFormat;
        TextFormat historyTextFormat;
        public string ConsoleString = "' '";
        private string HistoryText = "";
        public Dictionary<string, CommandFucntionDelegate> CommandList;
        public List<string> LuaCommandsList;
        private int CaretkaPos;

        public ConsoleComponent()
        {
            d2dFactory = new Direct2D1.Factory();
        }
        
        public void Initialize(Game game)
        {
            gameInst = (ShadowMapLightingGame)game;
            Surface surface = gameInst.BackBufer.QueryInterface<Surface>();
            d2dRenderTarget = new RenderTarget(d2dFactory, surface,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
            CommandList = new Dictionary<string, CommandFucntionDelegate>();
            CommandList.Add("exit", CloseGame);
            CommandList.Add("funny", FunnyText);
            CommandList.Add("change", ChangeDefferedTexture);
            LuaCommandsList = new List<string>();
            LuaCommandsList.Add("kek");
            LuaCommandsList.Add("funny");
        }

        public void ToogleVisibility()
        {
            if(IsEnabled)
            {
                IsEnabled = false;
                TextRect.Dispose();
                LayoutRect.Dispose();
                FontFactory.Dispose();
                TextRectangleBrush.Dispose();
                consoleTextFormat.Dispose();
                historyTextFormat.Dispose();
                LayoutBrush.Dispose();
                TextBrush.Dispose();
            }
            else
            {
                TextRect = new RoundedRectangleGeometry(d2dFactory, new RoundedRectangle()
                {
                    RadiusX = 1,
                    RadiusY = 1,
                    Rect = new RectangleF(10, gameInst.RenderForm.Height - 80, gameInst.RenderForm.Width -  10*4, 30)
                });

                LayoutRect= new RoundedRectangleGeometry(d2dFactory, new RoundedRectangle()
                {
                    RadiusX = 0,
                    RadiusY = 0,
                    Rect = new RectangleF(0, 0, gameInst.RenderForm.Width, gameInst.RenderForm.Height)
                });

                FontFactory = new DirectWrite.Factory();

                consoleTextFormat = new TextFormat(FontFactory, "Segoe UI", 20.0f);
                ConsoleLayout = new TextLayout(FontFactory, ConsoleString, consoleTextFormat, gameInst.RenderForm.Width - 40, 40);

                historyTextFormat = new TextFormat(FontFactory, "Segoe UI", 12.0f);
                HistoryLayout = new TextLayout(FontFactory, HistoryText, historyTextFormat, gameInst.RenderForm.Width - 40, gameInst.RenderForm.Height - 150.0f);

                TextRectangleBrush = new SolidColorBrush(d2dRenderTarget, Color.Black);
                LayoutBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0.1f, 0.1f, 0.1f, 0.85f));
                TextBrush = new SolidColorBrush(d2dRenderTarget, Color.Red);

                IsEnabled = true;
            }
        }

        private void CloseGame()
        {
            gameInst.ShutdownGame();
        }

        public void FunnyText()
        {
            HistoryText += DateTime.Now.ToLongTimeString() + ": " + "it is very funny" + "\n";
        }

        private void ChangeDefferedTexture()
        {
            HistoryText += DateTime.Now.ToLongTimeString() + ": " + "Brand new texture on the plane!" + "\n";
            gameInst.ChangeDefferedTarget();
        }

        public void ConsumeKey(Keys key)
        {
            switch (key)
            {
                case Keys.Enter:
                    if(ConsoleString!="' '")
                    {
                        CheckStringCommand(ConsoleString);
                        ConsoleString = "' '";
                        CaretkaPos = 1;
                        BuildConsoleTextLayout();
                        BuildHistoryTextLayout();
                    }
                    break;
                case Keys.Back:
                    if(ConsoleString!="' '")
                    {
                        ConsoleString = ConsoleString.Substring(0, ConsoleString.Length - 1);
                        CaretkaPos--;
                        if (ConsoleString == "")
                        {
                            ConsoleString = "' '";
                            CaretkaPos = 1;
                        }

                        BuildConsoleTextLayout();
                    }
                    break;
                case Keys.Escape:
                    gameInst.ShutdownGame();
                    break;
                default:
                    if (ConsoleString == "' '")
                    {
                        ConsoleString = key.ToString();
                    }
                    else
                    {
                        ConsoleString += key;
                        CaretkaPos++;
                    }
                    BuildConsoleTextLayout();
                    break;
            }
        }

        private void CheckStringCommand(string command)
        {
            command = command.ToLower();
            if (LuaCommandsList.Contains(command))
            {
                command += "()";
                gameInst.LuaManager.ExecuteLuaLine(command);
              //  HistoryText += DateTime.Now.ToLongTimeString() + ": " + CommandList[command] + "\n";
            }
            else
            {
                HistoryText += DateTime.Now.ToLongTimeString() + ": " + "Unknow command" + "\n";
            }
        }

        private void BuildConsoleTextLayout()
        {
            ConsoleLayout = new TextLayout(FontFactory, ConsoleString, consoleTextFormat, gameInst.RenderForm.Width - 40, 40);
        }
        private void BuildHistoryTextLayout()
        {
            HistoryLayout = new TextLayout(FontFactory, HistoryText, historyTextFormat, gameInst.RenderForm.Width - 40, gameInst.RenderForm.Height - 150.0f);
        }

        public void Draw()
        {
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.FillGeometry(LayoutRect, LayoutBrush);
            d2dRenderTarget.FillGeometry(TextRect, TextRectangleBrush, null);
          //  d2dRenderTarget.DrawText("eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", TextFormat, new RawRectangleF(10, gameInst.RenderForm.Height - 40, gameInst.RenderForm.Width - 10 * 4, gameInst.RenderForm.Height-80), TextBrush, DrawTextOptions.Clip);
            d2dRenderTarget.DrawTextLayout(new Vector2(10.0f, gameInst.RenderForm.Height - 80.0f), ConsoleLayout, TextBrush, DrawTextOptions.Clip);
            d2dRenderTarget.DrawTextLayout(new Vector2(10.0f, 40), HistoryLayout, TextBrush, DrawTextOptions.Clip);
            d2dRenderTarget.EndDraw();
        }

        public void Dispose()
        {
            TextRect.Dispose();
            LayoutRect.Dispose();
            FontFactory.Dispose();
            TextRectangleBrush.Dispose();
            consoleTextFormat.Dispose();
            historyTextFormat.Dispose();
            LayoutBrush.Dispose();
            TextBrush.Dispose();
            d2dRenderTarget.Dispose();
            d2dFactory.Dispose();
        }
    }
}
