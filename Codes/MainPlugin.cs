using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using s649.Logger;
//using System.Diagnostics;
//using UnityEngine;
//using Debug = UnityEngine.Debug;
//using LovLevel = s649.Logger.MyLogger.LogLevel;
namespace s649_DummyPracticeMod
{
    public static class PluginSettings
    {
        internal const int CurrentConfigVersion = 1;
        public const string GUID = "s649_DummyPracticeMod";
        public const string MOD_TITLE = "Dummy Practice Mod";
        public const string MOD_VERSION = "1.0.4.10";
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
        //internal static ConfigEntry<MyLogger.LogLevel> CE_LogLevel;//デバッグ用のログの出力LV　1:Errorのみ 
        //public static MyLogger.LogLevel cf_LogLevel => CE_LogLevel.Value;
        internal static ConfigEntry<PluginSettings.ExChangeMenu> CE_ExChangeMenu;      //スタミナ消費代替の選択肢
        //internal static ConfigEntry<int> CE_SleepinessExchangeBaseRate; //睡眠代替発生確率の基本値
        internal static ConfigEntry<int> CE_SleepinessExchangeUpperLimit; //睡眠代替の上限
        //internal static ConfigEntry<int> CE_SleepinessExchangeLowerLimit; //睡眠代替の下限

        //internal static ConfigEntry<int> CE_HungerExchangeBaseRate;     //空腹代替発生確率の基本値
        internal static ConfigEntry<int> CE_HungerExchangeUpperLimit; //睡眠代替の上限
        //internal static ConfigEntry<int> CE_HungerExchangeLowerLimit; //睡眠代替の下限
        internal static ConfigEntry<int> CE_SleepinessExchangeRate; //睡眠代替の成功率の基本値
        internal static ConfigEntry<int> CE_HungerExchangeRate; //空腹代替の成功率の基本値

        internal static ConfigEntry<float> CE_SleepinessExchangeDecay; //睡眠代替の成功率の減衰値
        internal static ConfigEntry<float> CE_HungerExchangeDecay; //空腹代替の成功率の減衰値

        internal static ConfigEntry<bool> CE_AddDeepInfo; //ログ表示に詳細な情報を付加するかどうか。
        internal static ConfigEntry<bool> CE_ShowLogTweet;//デバッグ用のメソッドを表示するかどうか
        internal static ConfigEntry<bool> CE_ShowLogInfo;//通常のログを表示するかどうか
        internal static ConfigEntry<bool> CE_ShowLogWarning;//警告のログを表示するかどうか
        //internal static ConfigEntry<bool> CE_ShowLogError;//エラーのログを表示するかどうか

        //internal static ConfigEntry<bool> CE_SleepinessExchangeScale;   //睡眠代替が睡眠値に応じてスケールするかどうか
        //internal static ConfigEntry<bool> CE_HungerExchangeScale;       //空腹代替が空腹値に応じてスケールするかどうか
        //public static ConfigEntry<bool> CE_ModEnable;
        //public static bool cf_ModEnable => CE_ModEnable.Value;
        //config descriptions-----------------------------------------------------------------
        public string configVersionDesc = (Lang.isJP) ?
            "コンフィグファイルのバージョンです。この項目はいじらないでください。" :
            "This is the version of the configuration file. Please do not modify this item.";
        //public string ce_loglevel_desc = (Lang.isJP) ?
        //    "BepInExのログ出力の制御。" :
        //    "Controlling BepInEx log output.";
        public string ce_exchangemenu_desc = (Lang.isJP) ?
            "スタミナ消費の代替モードを選択できます。" :
            "You can select an alternative mode for stamina consumption.";

        public string ce_SleepinessExchangeUpperLimit_desc = (Lang.isJP) ?
            "睡眠値の条件の上限。" :
            "Upper limit of sleep value conditions.";
        public string ce_HungerExchangeUpperLimit_desc = (Lang.isJP) ?
            "空腹値の条件の上限。" :
            "Upper limit of hunger value conditions.";
        public string ce_SleepinessExchangeRate_desc = (Lang.isJP) ?
            "睡眠変換の成功率に影響する。" :
            "Affects the success rate of sleep conversion.";
        public string ce_HungerExchangeRate_desc = (Lang.isJP) ?
            "空腹変換の成功率に影響する。" :
            "Affects the success rate of hunger conversion.";
        public string ce_SleepinessExchangeDecay_desc = (Lang.isJP) ?
            "睡眠変換の成功率の減衰に影響する。" :
            "It affects the decay of the sleep conversion success rate.";
        public string ce_HungerExchangeDecay_desc = (Lang.isJP) ?
            "空腹変換の成功率の減衰に影響する。" :
            "It affects the decay of the hunger conversion success rate.";

        public string desc_AddDeepInfo = (Lang.isJP) ?
            "ログに詳細な情報を追加するかどうか。" :
            "Whether to add detailed information to the log.";
        public string desc_ShowLogTweet = (Lang.isJP) ?
            "デバッグ用のログを出力するかどうか。" :
            "Whether to output debug logs.";
        public string desc_ShowLogInfo = (Lang.isJP) ?
            "通常のログを出力するかどうか。" :
            "Whether to output normal logs.";
        public string desc_ShowLogWarning = (Lang.isJP) ?
            "警告のログを出力するかどうか。" :
            "Whether to output warning logs.";

        //init---------------------------------------------------
        //MyLogger.LogLevel init_loglevel = MyLogger.LogLevel.Info;
        PluginSettings.ExChangeMenu init_exchangemenu = PluginSettings.ExChangeMenu.Hunger_priority;
        int init_SleepinessExchangeUpperLimit = 80;
        //int init_SleepinessExchangeLowerLimit = 0;
        int init_HungerExchangeUpperLimit = 60;
        int init_SleepinessExchangeRate = 80;
        int init_HungerExchangeRate = 50;
        float init_SleepinessExchangeDecay = 2.5f;
        float init_HungerExchangeDecay = 1f;

        bool init_AddDeepInfo = false;
        bool init_ShowLogTweet = false;
        bool init_ShowLogInfo = true;
        bool init_ShowLogWarning = true;
        //int init_HungerExchangeLowerLimit = 0;
        //loading-------------------------------------------------
        private void Start()
        {
            //if (Lang.isJP) { Debug.Log("JP!!!!!!!!!!!!!!!!!!!!!!"); }
            ConfigVersion = Config.Bind(
                "#99-System",
                "ConfigVersion",
                0, // デフォルトは 0 (まだ未設定相当)
                configVersionDesc
            );
            //CE_LogLevel = Config.Bind("#09-Debug", "LogLevel", init_loglevel, ce_loglevel_desc);
            //CE_ModEnable = Config.Bind("#general", "Mod_Enable", true, "Enable Mod function");
            CE_ExChangeMenu = Config.Bind("#00-ExchangeSelect", "ExChangeMenu", init_exchangemenu, ce_exchangemenu_desc);
            //CE_SleepinessExchangeBaseRate = Config.Bind("#01-Sleepiness", "SleepinessExchangeBaseRate", 50, "desc");
            CE_SleepinessExchangeUpperLimit = Config.Bind("#01-Sleepiness", "SleepinessExchangeUpperLimit", init_SleepinessExchangeUpperLimit, ce_SleepinessExchangeUpperLimit_desc);
            CE_HungerExchangeUpperLimit = Config.Bind("#02-Hunger", "HungerExchangeUpperLimit", init_HungerExchangeUpperLimit, ce_HungerExchangeUpperLimit_desc);

            CE_SleepinessExchangeRate = Config.Bind("#01-Sleepiness", "SleepinessExchangeRate", init_SleepinessExchangeRate, ce_SleepinessExchangeRate_desc);
            CE_HungerExchangeRate = Config.Bind("#02-Hunger", "HungerExchangeRate", init_HungerExchangeRate, ce_HungerExchangeRate_desc);

            CE_SleepinessExchangeDecay = Config.Bind("#01-Sleepiness", "SleepinessExchangeDecay", init_SleepinessExchangeDecay, ce_SleepinessExchangeDecay_desc);
            CE_HungerExchangeDecay = Config.Bind("#02-Hunger", "HungerExchangeDecay", init_HungerExchangeDecay, ce_HungerExchangeDecay_desc);


            CE_AddDeepInfo = Config.Bind("#99-System", "AddDeepInfo", init_AddDeepInfo, desc_AddDeepInfo);
            CE_ShowLogTweet = Config.Bind("#99-System", "ShowLogTweet", init_ShowLogTweet, desc_ShowLogTweet);
            CE_ShowLogInfo = Config.Bind("#99-System", "ShowLogInfo", init_ShowLogInfo, desc_ShowLogInfo);
            CE_ShowLogWarning = Config.Bind("#99-System", "ShowLogWarning", init_ShowLogWarning, desc_ShowLogWarning);
            //CE_SleepinessExchangeScale = Config.Bind("#01-Sleepiness", "SleepinessExchangeScale", true, "desc");
            //CE_HungerExchangeScale = Config.Bind("#02-Hunger", "HungerExchangeScale", true, "desc");
            //PluginSettings.myLogger = new MyLogger(PluginSettings.ModNS);
            myLogger = new MyLogger()
            {
                myLogSource = base.Logger,
                callerClass = "",
                //MyLogLevel = CE_LogLevel.Value,
                topMethod = "Start"
            };
            //Components.MyLogLevel = CE_LogLevel.Value;

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
                myLogger.LogInfo("Migration from v0 to v1 complete.");
            }
            
        }
        private void InitConfig()
        {
            //CE_LogLevel.Value = init_loglevel;
            CE_ExChangeMenu.Value = init_exchangemenu;
            CE_HungerExchangeRate.Value = init_HungerExchangeRate;
            CE_HungerExchangeUpperLimit.Value = init_HungerExchangeUpperLimit;
            CE_HungerExchangeDecay.Value = init_HungerExchangeDecay;

            CE_SleepinessExchangeRate.Value = init_SleepinessExchangeRate;
            CE_SleepinessExchangeUpperLimit.Value = init_SleepinessExchangeUpperLimit;
            CE_SleepinessExchangeDecay.Value = init_SleepinessExchangeDecay;

            CE_AddDeepInfo.Value = init_AddDeepInfo;
            CE_ShowLogTweet.Value = init_ShowLogTweet;
            CE_ShowLogInfo.Value = init_ShowLogInfo;
            CE_ShowLogWarning.Value = init_ShowLogWarning;
        }
    }
}
