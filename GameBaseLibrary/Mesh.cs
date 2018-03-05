using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBaseLibrary
{
    public class Mesh : Component
    {
        Model model;
        public Model Model
        {
            get
            {
                return model;
            }
        }

        public void Initialize(Model model)
        {
            base.Initialize();

            this.model = model;
        }
    }
}