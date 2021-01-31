using Dalamud.Plugin;

namespace HousingPos.Gui
{
    public abstract class Window<T> where T : IDalamudPlugin
    {
        protected bool WindowVisible;
        public virtual bool Visible
        {
            get => WindowVisible;
            set => WindowVisible = value;
        }

        protected T Plugin { get; }

        protected Window(T plugin)
        {
            Plugin = plugin;
        }

        public void Draw()
        {
            if (Visible)
                DrawUi();
            DrawScreen();
        }

        protected abstract void DrawUi();
        protected abstract void DrawScreen();
    }
}