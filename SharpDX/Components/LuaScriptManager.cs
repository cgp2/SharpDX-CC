using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;
using SharpDX.Games;

namespace SharpDX.Components
{
    class LuaScriptManager
    {
        Lua LuaInst;
        private Game gameInst;

        public LuaScriptManager(Game game)
        {
            this.gameInst = game;
            LuaInst = new Lua();
            LuaInst.RegisterFunction("kek", ((ShadowMapLightingGame) gameInst),
                typeof(ShadowMapLightingGame).GetMethod("ChangeDefferedTarget"));
            LuaInst.RegisterFunction("funny", ((ShadowMapLightingGame)gameInst).consoleComponent,
                typeof(ConsoleComponent).GetMethod("FunnyText"));
        }

        public void ExecuteLuaLine(string line)
        {
            LuaInst.DoString(line);
        }
    }
}
