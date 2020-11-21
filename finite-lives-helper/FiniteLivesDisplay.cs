using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.FiniteLives
{
    public class FiniteLivesDisplay : Entity
    {
        private readonly Level level;
        private readonly MTexture heartgem, bg, x;
        private string text = "";
        private float width;
        private float timer = 0;
        private float lerp = 0;
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
        /// Setter function for display text. Also adjust width accordingly and reset timer.
        /// </summary>
        /// <param name="s">Display text.</param>
        public void SetDisplayText(string s)
        {
            // Don't update values if text doesn't change
            if (text.Equals(s))
                return;

            text = s;
            width = ActiveFont.Measure(text).X + TEXT_PAD_L + TEXT_PAD_R;
            timer = 3;
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
        /// Update function for fade left/right transition.
        /// </summary>
        public override void Update()
        {
            base.Update();          
            if (timer > 0)
            {
                // Fade right
                timer -= Engine.RawDeltaTime;
                lerp = Calc.Approach(lerp, 1, 3.25f * Engine.RawDeltaTime);
            }
            else
            {
                // Fade left
                lerp = Calc.Approach(lerp, 0, 1.75f * Engine.RawDeltaTime);
            }
        }

        /// <summary>
        /// Render function for display object.
        /// </summary>
        public override void Render()
        {
            if (!enabled)
                return;

            Vector2 basePos = Vector2.Lerp(new Vector2(-width, Y), new Vector2(0, Y), Ease.CubeOut(lerp)).Round();
            bg.Draw(new Vector2(basePos.X + width - bg.Width, basePos.Y));
            heartgem.Draw(new Vector2(basePos.X + 26, basePos.Y - 24), Vector2.Zero, Color.White, new Vector2(0.29f, 0.29f));
            x.Draw(new Vector2(basePos.X + 94, basePos.Y - 15));
            ActiveFont.DrawOutline(text, new Vector2(basePos.X + TEXT_PAD_L, basePos.Y - 23), Vector2.Zero, Vector2.One, Color.White, 2, Color.Black);
        }
    }
}
