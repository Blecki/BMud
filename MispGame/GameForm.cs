using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MispGame
{
    public class GameForm
    {
        public System.Windows.Forms.Form form;
        public GraphicsDevice GraphicsDevice;
        public MISP.Engine mispEngine;
        public MISP.GenericScriptObject windowObject;
        public SpriteBatch spriteBatch;
        public DateTime previousTime;
        public float ElapsedSeconds = 0;
        public Dictionary<Keys, MISP.Function> keyDownBindings = new Dictionary<Keys, MISP.Function>();
        public Dictionary<Keys, MISP.Function> keyUpBindings = new Dictionary<Keys, MISP.Function>();

        public GameForm(int Width, int Height)
        {
            form = new Form();
            form.ClientSize = new Size(Width, Height);
            form.MainMenuStrip = null;

            form.Show();

            PresentationParameters pp = new PresentationParameters();
            pp.DeviceWindowHandle = form.Handle;

            pp.BackBufferFormat = SurfaceFormat.Color;
            pp.BackBufferWidth = Width;
            pp.BackBufferHeight = Height;
            pp.RenderTargetUsage = RenderTargetUsage.DiscardContents;
            pp.IsFullScreen = false;


            pp.MultiSampleCount = 16;

            pp.DepthStencilFormat = DepthFormat.Depth24Stencil8;

            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                                                      GraphicsProfile.Reach,
                                                      pp);

            previousTime = DateTime.Now;

            form.KeyDown += (sender, args) =>
                {
                    if (keyDownBindings.ContainsKey(args.KeyCode))
                        keyDownBindings[args.KeyCode].Invoke(mispEngine, new MISP.Context(), new MISP.ScriptList());
                };

            form.KeyUp += (sender, args) =>
            {
                if (keyUpBindings.ContainsKey(args.KeyCode))
                    keyUpBindings[args.KeyCode].Invoke(mispEngine, new MISP.Context(), new MISP.ScriptList());
            };
        }

        public void Run(MISP.Engine mispEngine, MISP.GenericScriptObject windowObject)
        {
            
            Application.Idle += new EventHandler(Application_Idle);

            this.mispEngine = mispEngine;
            this.windowObject = windowObject;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //AppThread = new System.Threading.Thread(() => { Application.Run(form); });
            //AppThread.Start();
            Application.Run(form);
        }

        public Texture2D LoadTexture(String filename)
        {
            var image = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(filename));
            return CreateTexture(image);
        }

        public Texture2D CreateTexture(Bitmap bitmap)
        {
            var texture = new Texture2D(GraphicsDevice, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] bytes = new byte[data.Height * data.Width * 4];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            for (int i = 0; i < data.Height * data.Width * 4; i += 4)
            {
                var temp = bytes[i + 0];
                bytes[i + 0] = bytes[i + 2];
                bytes[i + 2] = temp;
            }
            texture.SetData<byte>(bytes, 0, data.Height * data.Width * 4);
            bitmap.UnlockBits(data);
            return texture;
        }


        private void Application_Idle(object pSender, EventArgs pEventArgs)
        {
            Message message;
            while (!PeekMessage(out message, IntPtr.Zero, 0, 0, 0))
            {
                var currentTime = DateTime.Now;
                ElapsedSeconds = (float)(currentTime - previousTime).TotalSeconds;
                previousTime = currentTime;

                windowObject.SetProperty("elapsed", ElapsedSeconds);

                GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Microsoft.Xna.Framework.Color.Black, 1.0f, 0);
                spriteBatch.Begin();
                var updateFunc = windowObject.GetProperty("update");
                if (updateFunc is MISP.Function)
                    (updateFunc as MISP.Function).Invoke(mispEngine, new MISP.Context(), new MISP.ScriptList());
                spriteBatch.End();
                GraphicsDevice.Present();
            }

        }

        

        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        {
            public IntPtr hWnd;
            public int msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [System.Security.SuppressUnmanagedCodeSecurity, DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint
            messageFilterMin, uint messageFilterMax, uint flags);
    }

}
