using Dalamud.Plugin;

namespace HousingPos.Gui
{
    public abstract class Window<T> where T : IDalamudPlugin
    {
        protected bool WindowVisible;
        protected bool WindowCanUpload;
        public virtual bool Visible
        {
            get => WindowVisible;
            set => WindowVisible = value;
        }
        public virtual bool CanUpload
        {
            get => WindowCanUpload;
            set => WindowCanUpload = value;
        }
        protected T Plugin { get; }

        protected Window(T plugin)
        {
            Plugin = plugin;
        }
        public void Draw()
        {
            if (Visible)
            {
                DrawUi();
                if (CanUpload)
                {
                    DrawUploadUi();
                }
            }
            DrawScreen();
        }

        protected abstract void DrawUi();
        protected abstract void DrawScreen();
        protected abstract void DrawUploadUi();
    }
}