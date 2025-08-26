
using UnityEngine;
namespace s649_DummyPracticeMod.Codes
{
    public class BepInProps
    {
        //public static bool cf_ModEnable => MainPlugin.CE_ModEnable.Value;
        public static PluginSettings.ExChangeMenu exchangeMenu => MainPlugin.CE_ExChangeMenu.Value;

        //public static int sleeinessExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeBaseRate.Value, 0, 100);
        public static int sleeinessExchangeUpperLimit = Mathf.Clamp(MainPlugin.CE_SleepinessExchangeUpperLimit.Value, 0, 100);
        public static int sleeinessExchangeLowerLimit = 0;// (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeLowerLimit.Value, 0, 100);
        public static int sleeinessExchangeRate = Mathf.Clamp(MainPlugin.CE_SleepinessExchangeRate.Value, 1, 100);
        public static float sleeinessExchangeDecay = Mathf.Clamp(MainPlugin.CE_SleepinessExchangeDecay.Value, 0.1f, 10f);

        //public static int hungerExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeBaseRate.Value, 0, 100);
        public static int hungerExchangeUpperLimit = Mathf.Clamp(MainPlugin.CE_HungerExchangeUpperLimit.Value, 0, 100);
        public static int hungerExchangeLowerLimit = 0;// (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeLowerLimit.Value, 0, 100);
        public static int hungerExchangeRate = Mathf.Clamp(MainPlugin.CE_HungerExchangeRate.Value, 1, 100);
        public static float hungerExchangeDecay = Mathf.Clamp(MainPlugin.CE_HungerExchangeDecay.Value, 0.1f, 10f);


        public static bool addDeepInfo => MainPlugin.CE_AddDeepInfo.Value;
        public static bool showLogTweet => MainPlugin.CE_ShowLogTweet.Value;
        public static bool showLogInfo => MainPlugin.CE_ShowLogInfo.Value;
        public static bool showLogWarning => MainPlugin.CE_ShowLogWarning.Value;

        //public static bool sleepinessExchangeScale => MainPlugin.CE_SleepinessExchangeScale.Value;
        //public static bool hungerExchangeScale => MainPlugin.CE_HungerExchangeScale.Value;

    }
}
