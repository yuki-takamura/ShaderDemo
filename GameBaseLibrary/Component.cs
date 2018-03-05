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

namespace GameBaseLibrary
{
    public class Component
    {
        public bool isActive = true;
        public bool IsActive
        { 
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
            }
        }

        public Component()
        {
        }

        ~Component()
        {
        }

        public virtual void Initialize()
        {
            if (!isActive)
                return;
        }

        public virtual void Update()
        {
            if (!isActive)
                return;
        }
    }
}
