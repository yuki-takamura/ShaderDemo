﻿using System;
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
    public class Mesh : Component
    {
        Model model;
        Model modelSetter;

        public Model Model
        {
            get
            {
                return model;
            }
        }

        public Model ModelSetter
        {
            get
            {
                return modelSetter;
            }
            set
            {
                modelSetter = value;
            }
        }

        public override void Initialize()
        {
            model = modelSetter;
        }
    }
}
