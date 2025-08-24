using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using s649.Logger;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
//using LovLevel = s649.Logger.MyLogger.LogLevel;
namespace s649_DummyPracticeMod
{
    public static class PluginSettings
    {
        internal const int CurrentConfigVersion = 1;
        public const string GUID = "s649_DummyPracticeMod";
        public const string MOD_TITLE = "Dummy Practice Mod";
        public const string MOD_VERSION = "1.0.0.11";
        public const string ModNS = "DPM";
        public const int ID_PracticeFatigue = 64900100;
        public enum ExChangeMenu
        {
            None = 0,
            Only_Sleepiness = 1,
            Only_Hunger = 2,
            Sleepiness_priority = 3,
            Hunger_priority = 4
        }
    }

    [BepInPlugin(PluginSettings.GUID, PluginSettings.MOD_TITLE, PluginSettings.MOD_VERSION)]
    public class MainPlugin : BaseUnityPlugin
    {
        public static MyLogger myLogger;
        private ConfigEntry<int> ConfigVersion;

        //entry---------------------------------------------------
        internal static ConfigEntry<MyLogger.LogLevel> CE_LogLevel;//デバッグ用のログの出力LV　1:Errorのみ 
        //public static MyLogger.LogLevel cf_LogLevel => CE_LogLevel.Value;
        internal static ConfigEntry<PluginSettings.ExChangeMenu> CE_ExChangeMenu;      //スタミナ消費代替の選択肢
        //internal static ConfigEntry<int> CE_SleepinessExchangeBaseRate; //睡眠代替発生確率の基本値
        internal static ConfigEntry<int> CE_SleepinessExchangeUpperLimit; //睡眠代替の上限
        //internal static ConfigEntry<int> CE_SleepinessExchangeLowerLimit; //睡眠代替の下限

        //internal static ConfigEntry<int> CE_HungerExchangeBaseRate;     //空腹代替発生確率の基本値
        internal static ConfigEntry<int> CE_HungerExchangeUpperLimit; //睡眠代替の上限
        //internal static ConfigEntry<int> CE_HungerExchangeLowerLimit; //睡眠代替の下限
        internal static ConfigEntry<int> CE_SleepinessExchangeDifficulty; //睡眠代替の成功難度
        internal static ConfigEntry<int> CE_HungerExchangeDifficulty; //空腹代替の成功難度

        //internal static ConfigEntry<bool> CE_SleepinessExchangeScale;   //睡眠代替が睡眠値に応じてスケールするかどうか
        //internal static ConfigEntry<bool> CE_HungerExchangeScale;       //空腹代替が空腹値に応じてスケールするかどうか
        //public static ConfigEntry<bool> CE_ModEnable;
        //public static bool cf_ModEnable => CE_ModEnable.Value;
        //config descriptions-----------------------------------------------------------------
        public string configVersionDesc = (Lang.isJP) ?
            "コンフィグファイルのバージョンです。この項目はいじらないでください。" :
            "This is the version of the configuration file. Please do not modify this item.";
        public string ce_loglevel_desc = (Lang.isJP) ?
            "BepInExのログ出力の制御。" :
            "Controlling BepInEx log output.";
        public string ce_exchangemenu_desc = (Lang.isJP) ?
            "スタミナ消費の代替モードを選択できます。" :
            "You can select an alternative mode for stamina consumption.";
        public string ce_SleepinessExchangeUpperLimit_desc = (Lang.isJP) ?
            "睡眠値の条件の上限。" :
            "Upper limit of sleep value conditions.";
        
        public string ce_HungerExchangeUpperLimit_desc = (Lang.isJP) ?
            "空腹値の条件の上限。" :
            "Upper limit of hunger value conditions.";
        public string ce_SleepinessExchangeDifficulty_desc = (Lang.isJP) ?
            "睡眠変換の成功難度。" :
            "Difficulty of successful sleep conversion.";
        public string ce_HungerExchangeDifficulty_desc = (Lang.isJP) ?
            "空腹変換の成功難度。" :
            "Difficulty of successful converting hunger.";
        //init---------------------------------------------------
        MyLogger.LogLevel init_loglevel = MyLogger.LogLevel.Info;
        PluginSettings.ExChangeMenu init_exchangemenu = PluginSettings.ExChangeMenu.Hunger_priority;
        int init_SleepinessExchangeUpperLimit = 100;
        //int init_SleepinessExchangeLowerLimit = 0;
        int init_HungerExchangeUpperLimit = 80;
        int init_SleepinessExchangeDifficulty = 4;
        int init_HungerExchangeDifficulty = 10;
        //int init_HungerExchangeLowerLimit = 0;
        //loading-------------------------------------------------
        private void Start()
        {
            //if (Lang.isJP) { Debug.Log("JP!!!!!!!!!!!!!!!!!!!!!!"); }
            ConfigVersion = Config.Bind(
                "#System",
                "ConfigVersion",
                0, // デフォルトは 0 (まだ未設定相当)
                configVersionDesc
            );
            CE_LogLevel = Config.Bind("#zz-Debug", "LogLevel", init_loglevel, ce_loglevel_desc);
            //CE_ModEnable = Config.Bind("#general", "Mod_Enable", true, "Enable Mod function");
            CE_ExChangeMenu = Config.Bind("#00-ExchangeSelect", "ExChangeMenu", init_exchangemenu, ce_exchangemenu_desc);
            //CE_SleepinessExchangeBaseRate = Config.Bind("#01-Sleepiness", "SleepinessExchangeBaseRate", 50, "desc");
            CE_SleepinessExchangeUpperLimit = Config.Bind("#01-Sleepiness", "SleepinessExchangeUpperLimit", init_SleepinessExchangeUpperLimit, ce_SleepinessExchangeUpperLimit_desc);
            CE_SleepinessExchangeDifficulty = Config.Bind("#01-Sleepiness", "SleepinessExchangeDifficulty", init_SleepinessExchangeDifficulty, ce_SleepinessExchangeDifficulty_desc);

            //CE_HungerExchangeBaseRate = Config.Bind("#02-Hunger", "HungerExchangeBaseRate", 50, "desc");
            CE_HungerExchangeUpperLimit = Config.Bind("#02-Hunger", "HungerExchangeUpperLimit", init_HungerExchangeUpperLimit, ce_HungerExchangeUpperLimit_desc);
            CE_HungerExchangeDifficulty = Config.Bind("#02-Hunger", "HungerExchangeDifficulty", init_HungerExchangeDifficulty, ce_HungerExchangeDifficulty_desc);

            //CE_SleepinessExchangeScale = Config.Bind("#01-Sleepiness", "SleepinessExchangeScale", true, "desc");
            //CE_HungerExchangeScale = Config.Bind("#02-Hunger", "HungerExchangeScale", true, "desc");
            //PluginSettings.myLogger = new MyLogger(PluginSettings.ModNS);
            myLogger = new MyLogger()
            {
                myLogSource = base.Logger,
                callerClass = "",
                MyLogLevel = CE_LogLevel.Value,
                topMethod = "Start"
            };
            if (ConfigVersion.Value < PluginSettings.CurrentConfigVersion)
            {
                //Logger.LogInfo($"Config version outdated ({ConfigVersion.Value} < {PluginSettings.CurrentConfigVersion}). Running migration...");
                myLogger.LogInfo($"Config version outdated (" + ConfigVersion.Value + "< "+ PluginSettings.CurrentConfigVersion + "). Running migration...");

                RunMigration(ConfigVersion.Value, PluginSettings.CurrentConfigVersion);

                // バージョンを更新して保存
                ConfigVersion.Value = PluginSettings.CurrentConfigVersion;
                Config.Save();
            }
            //myLogger.SetLevel(CE_LogLevel.Value);
            //myLogger.SetPackageName(PluginSettings.ModNS);
            new Harmony(this.GetType().Name).PatchAll();

        }
        /// <summary>
        /// 古いバージョンからの移行処理
        /// </summary>
        private void RunMigration(int oldVersion, int newVersion)
        {
            if (oldVersion < newVersion)
            {
                
                InitConfig();
                //Config.Save();
                Logger.LogInfo("Migration from v0 to v1 complete.");
            }
            
        }
        private void InitConfig()
        {
            CE_LogLevel.Value = init_loglevel;
            CE_ExChangeMenu.Value = init_exchangemenu;
            CE_HungerExchangeDifficulty.Value = init_SleepinessExchangeDifficulty;
            CE_HungerExchangeUpperLimit.Value = init_SleepinessExchangeUpperLimit;
            CE_SleepinessExchangeDifficulty.Value = init_HungerExchangeDifficulty;
            CE_SleepinessExchangeUpperLimit.Value = init_HungerExchangeUpperLimit;
        }
    }
}
