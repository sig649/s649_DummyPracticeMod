using s649_DummyPracticeMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s649_DummyPracticeMod.Codes
{
    internal static class CharaExtension
    {
        internal static PracticeFatigue GetFatigue(this Chara chara)
        {
            return chara.GetObj<PracticeFatigue>(PluginSettings.ID_PracticeFatigue);
        }
        internal static int GetFatigueValue(this Chara chara)
        {
            return chara.GetObj<PracticeFatigue>(PluginSettings.ID_PracticeFatigue).value;
        }
        internal static void SetFatigue(this Chara chara, int num)
        {
            var fatigue = new PracticeFatigue(num);
            chara.SetObj<PracticeFatigue>(PluginSettings.ID_PracticeFatigue, fatigue);
        }
        internal static void ModFatigue(this Chara chara, int num)
        {
            var fatigue = chara.GetFatigue();
            fatigue.Mod(num);
            chara.SetObj<PracticeFatigue>(PluginSettings.ID_PracticeFatigue, fatigue);
        }
    }
}
