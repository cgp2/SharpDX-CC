using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.RawInput;
using SharpDX.Multimedia;
using System.Windows.Forms;

namespace PingPong
{
	public class InputDevice
	{
		public Game Game;

		public HashSet<Keys> PressedKeys = new HashSet<Keys>();

		public Vector2 MousePositionLocal	{ get; private set; }
		public Vector2 MouseOffset			{ get; private set; }

		public struct MouseMoveEventArgs
		{
			public Vector2 Position;
			public Vector2 Offset;
		}


		public event Action<MouseMoveEventArgs> MouseMove;

        
		public InputDevice(Game game)
		{
            
			Game = game;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
			Device.MouseInput += Device_MouseInput;

			Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
			Device.KeyboardInput += Device_KeyboardInput;
		}

        bool Break=false;
		private void Device_KeyboardInput(object sender, KeyboardInputEventArgs e)
		{
			Break	= e.ScanCodeFlags.HasFlag(ScanCodeFlags.Break);
          
			if (Break) {
				if (PressedKeys.Contains(e.Key)) RemovePressedKey(e.Key);
			} else {
				if (!PressedKeys.Contains(e.Key)) AddPressedKey(e.Key);
			}
		}

		private void Device_MouseInput(object sender, MouseInputEventArgs e)
		{
			var p = Game.renderForm.PointToClient(System.Windows.Forms.Cursor.Position);

            //Game.Camera.ChangeTargetPosition(MousePositionLocal.X - e.X, MousePositionLocal.Y - e.Y);


            MousePositionLocal	= new Vector2(p.X, p.Y);
			MouseOffset			= new Vector2(e.X, e.Y);


            if (MouseMove != null) {
				MouseMove(new MouseMoveEventArgs() { Position = MousePositionLocal, Offset = MouseOffset});

            }

            //Game.Camera.ChangeTargetPosition(MousePositionLocal.X, MousePositionLocal.Y);
        }


		/// <summary>
		/// Adds key to hash list and fires KeyDown event
		/// </summary>
		/// <param name="key"></param>
		void AddPressedKey(Keys key)
		{
			if (!Game.IsActive) {
				return;
			}

            switch (key)
            {
                case Keys.W:
                    Game.Camera.MoveForward();
                    break;
                case Keys.S:
                    Game.Camera.MoveBackward();
                    break;
                case Keys.A:
                    Game.Camera.MoveLeft();
                    break;
                case Keys.D:
                    Game.Camera.MoveRight();
                    break;
            }
            while(Break)
            {
                System.Threading.Thread.Sleep(TimeSpan.Zero);
                RemovePressedKey(key);
                AddPressedKey(key);
            }
			//PressedKeys.Add(key);
   //         if (!PressedKeys.Contains(key))
   //         {
   //             System.Threading.Thread.Sleep(1);
   //             RemovePressedKey(key);
   //             AddPressedKey(key);
   //         }
        }



        /// <summary>
        /// Removes key from hash list and fires KeyUp event
        /// </summary>
        /// <param name="key"></param>
        void RemovePressedKey(Keys key)
		{
			if (PressedKeys.Contains(key))
			{
				PressedKeys.Remove(key);
				//if (KeyUp != null)
				//{
				//	KeyUp(this, new KeyEventArgs() { Key = key });
				//}
			}
		}


		public bool IsKeyDown(Keys key, bool ignoreInputMode = true)
		{
			return (PressedKeys.Contains(key));
		}


		public bool IsKeyUp(Keys key)
		{
			return !IsKeyDown(key);
		}

	}
}
