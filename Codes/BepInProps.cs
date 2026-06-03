
using UnityEngine;
namespace s649_DummyPracticeMod.Codes
{
    public class BepInProps
    {

        //public static int sleeinessExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeBaseRate.Value, 0, 100);
        public static int sleeinessExchangeUpperLimit => Mathf.Clamp(MainPlugin.CE_SleepinessExchangeUpperLimit.Value, 0, 100);
        public static int sleeinessExchangeLowerLimit = 0;// (int)Mathf.Clamp(MainPlugin.CE_SleepinessExchangeLowerLimit.Value, 0, 100);
        //public static int sleeinessExchangeRate => Mathf.Clamp(MainPlugin.CE_SleepinessExchangeRate.Value, 1, 100);
        //public static float sleeinessExchangeDecay => Mathf.Clamp(MainPlugin.CE_SleepinessExchangeDecay.Value, 0.1f, 10f);
       // public static bool slepinessExchangeNoDecay => MainPlugin.CE_SleepinessExchangeNoDecay.Value;

        //public static int hungerExchangeBaseRate = (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeBaseRate.Value, 0, 100);
        public static int hungerExchangeUpperLimit => Mathf.Clamp(MainPlugin.CE_HungerExchangeUpperLimit.Value, 0, 100);
        public static int hungerExchangeLowerLimit = 0;// (int)Mathf.Clamp(MainPlugin.CE_HungerExchangeLowerLimit.Value, 0, 100);
        //public static int hungerExchangeRate => Mathf.Clamp(MainPlugin.CE_HungerExchangeRate.Value, 1, 100);
        //public static float hungerExchangeDecay => Mathf.Clamp(MainPlugin.CE_HungerExchangeDecay.Value, 0.1f, 10f);
        //public static bool hungerExchangeNoDecay => MainPlugin.CE_HungerExchangeNoDecay.Value;


        //public static bool sleepinessExchangeScale => MainPlugin.CE_SleepinessExchangeScale.Value;
        //public static bool hungerExchangeScale => MainPlugin.CE_HungerExchangeScale.Value;

    }
}
