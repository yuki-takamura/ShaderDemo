using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBaseLibrary
{
    public enum Map
    {
        Diffuse,
        Normal,
    }

    public class TextureMap
    {
        public List<Texture2D> textures = new List<Texture2D>();
        public Map map;
    }
}