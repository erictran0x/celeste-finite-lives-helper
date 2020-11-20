using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.FiniteLives
{
    public class FiniteLivesDisplay : Entity
    {
        private readonly Level level;
        private readonly MTexture heartgem, bg, x;
        private string text;
        private float width;
        private const float TEXT_PAD_L = 144;
        private const float TEXT_PAD_R = 12;
        private bool enabled = true;

        /// <summary>
        /// Constructor. Load textures and set tags and y-position.
        /// </summary>
        /// <param name="l">Level object.</param>
        public FiniteLivesDisplay(Level l)
        {
            level = l;
            heartgem = GFX.Gui[$"collectables/heartgem/{(int)level.Session.Area.Mode}/spin00"];
            bg = GFX.Gui["strawberryCountBG"];
            x = GFX.Gui["x"];
            Y = GetYPos();

            Depth = -100;
            Tag = Tags.HUD | Tags.TransitionUpdate | Tags.PauseUpdate | Tags.Global;
        }

        /// <summary>
        /// Setter function for display text.
        /// </summary>
        /// <param name="s">Display text.</param>
        public void SetDisplayText(string s)
        {
            text = s;
            width = ActiveFont.Measure(text).X + TEXT_PAD_L + TEXT_PAD_R;
        }

        /// <summary>
        /// Setter function for enabled.
        /// </summary>
        /// <param name="flag">Enabled.</param>
        public void SetEnabled(bool flag)
        {
            enabled = flag;
        }

        /// <summary>
        /// Calculate the y-position based on speedrun clock type.
        /// </summary>
        /// <returns>Y-position of the display object.</returns>
        private float GetYPos()
        {
            float y = 200f;  // TODO: make this a user setting
            if (!level.TimerHidden && Settings.Instance.SpeedrunClock != SpeedrunType.Off)
                y += Settings.Instance.SpeedrunClock == SpeedrunType.File ? 78f : 58f;
            return y;
        }

        /// <summary>
        /// Update function.
        /// </summary>
        public override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Render function.
        /// </summary>
        public override void Render()
        {
            if (!enabled)
                return;

            Vector2 basePos = new Vector2(0, Y).Round();
            bg.Draw(new Vector2(basePos.X + width - bg.Width, basePos.Y));
            heartgem.Draw(new Vector2(basePos.X + 26, basePos.Y - 24), Vector2.Zero, Color.White, new Vector2(0.29f, 0.29f));
            x.Draw(new Vector2(basePos.X + 94, basePos.Y - 15));
            ActiveFont.DrawOutline(text, new Vector2(basePos.X + TEXT_PAD_L, basePos.Y - 23), Vector2.Zero, Vector2.One, Color.White, 2, Color.Black);
        }
    }
}
