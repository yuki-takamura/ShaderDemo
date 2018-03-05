
namespace GameBaseLibrary
{
    public class Component
    {
        bool isActive = true;
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

        bool isInitialized = false;

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

            isInitialized = true;
        }

        public virtual void Update()
        {
            if (!isActive)
                return;

            if (!isInitialized)
                return;
        }
    }
}