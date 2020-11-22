using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.FiniteLives
{
    public class FiniteLivesSettings : EverestModuleSettings
    {
        [SettingRange(0, 100)]
        [SettingSubText("Adjust the y-position of the life display")]
        public int DisplayPosition { get; set; } = 20;
    }
}
