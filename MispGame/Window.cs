using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MispGame
{
    public class Window : MISP.NetModule
    {
        public override void BindModule(MISP.Engine engine)
        {
            engine.AddFunction("mg-create-window", "Creates a window with the specified dimensions.",
                (context, arguments) =>
                {
                    var width = arguments[0] as int?;
                    var height = arguments[1] as int?;
                    if (width == null || height == null) throw new MISP.ScriptError("Invalid arguments", context.currentNode);
                    var window = new GameForm(width.Value, height.Value);
                    var WindowObject = new MISP.GenericScriptObject();
                    WindowObject.SetProperty("window", window);
                    WindowObject.SetProperty("width", width.Value);
                    WindowObject.SetProperty("height", height.Value);

                    WindowObject.SetProperty("run", engine.MakeLambda("run", "Enter the window loop",
                        (interior_context, interior_arguments) =>
                        {
                            window.Run(engine, WindowObject);
                            return null;
                        }));

                    WindowObject.SetProperty("load", engine.MakeLambda("load", "Load an image",
                        (interior_context, interior_arguments) =>
                        {
                            return window.LoadTexture(MISP.ScriptObject.AsString(interior_arguments[0]));
                        }, "string filename"));

                    WindowObject.SetProperty("draw-sprite", engine.MakeLambda("draw-sprite", "Draw a sprite",
                        (interior_context, interior_arguments) =>
                        {
                            window.spriteBatch.Draw(interior_arguments[0] as Texture2D,
                                new Vector2((interior_arguments[1] as float?).Value,
                                    (interior_arguments[2] as float?).Value),
                                    Microsoft.Xna.Framework.Color.White);
                            return null;
                        }, "texture", "float x", "float y"));

                    WindowObject.SetProperty("bind-down", engine.MakeLambda("bind-down", "Bind a key down event",
                        (interior_context, interior_arguments) =>
                        {
                            window.keyDownBindings.Upsert(
                                (System.Windows.Forms.Keys)MISP.ScriptObject.AsString(interior_arguments[0])[0],
                                interior_arguments[1] as MISP.Function);
                            return null;
                        }, "string key", "function func"));

                    WindowObject.SetProperty("bind-up", engine.MakeLambda("bind-up", "Bind a key up event",
                        (interior_context, interior_arguments) =>
                        {
                            window.keyUpBindings.Upsert(
                                (System.Windows.Forms.Keys)MISP.ScriptObject.AsString(interior_arguments[0])[0],
                                interior_arguments[1] as MISP.Function);
                            return null;
                        }, "string key", "function func"));

                    return WindowObject;
                },
                    "integer width", "integer height");
        }
    }
}
