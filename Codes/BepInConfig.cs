
using UnityEngine;
namespace s649_DummyPracticeMod.Codes
{
    public class BepInConfig
    {
        //public static bool cf_ModEnable => MainPlugin.CE_ModEnable.Value;
        public static PluginSettings.ExChangeMenu exchangeMenu => MainPlugin.CE_ExChangeMenu.Value;

        //public static int sleeinessExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeBaseRate.Value, 0, 100);
        public static int sleeinessExchangeUpperLimit = (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeUpperLimit.Value, 0, 100);
        public static int sleeinessExchangeLowerLimit = (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeLowerLimit.Value, 0, 100);

        //public static int hungerExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeBaseRate.Value, 0, 100);
        public static int hungerExchangeUpperLimit = (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeUpperLimit.Value, 0, 100);
        public static int hungerExchangeLowerLimit = (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeLowerLimit.Value, 0, 100);

        //public static bool sleepinessExchangeScale => MainPlugin.CE_SleepinessExchangeScale.Value;
        //public static bool hungerExchangeScale => MainPlugin.CE_HungerExchangeScale.Value;

    }
}
