using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MispGame
{
    public class ServiceProvider : IServiceProvider, IGraphicsDeviceService
    {
        GraphicsDevice device;

        public ServiceProvider(GraphicsDevice device)
        {
            this.device = device;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IGraphicsDeviceService))
            {
                return this;
            }

            return null;
        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return device; }
        }
    }
}
