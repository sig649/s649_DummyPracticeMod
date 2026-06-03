using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using s649.Logger;
using s649_Config;
using UnityEngine;
namespace s649_DummyPracticeMod
{
    public static class PluginSettings
    {
        public const string GUID = "s649_TrainAndRecovery";
        public const string MOD_TITLE = "Train And Recovery";
        public const string MOD_VERSION = "2.1.0.0";
        //public const int ID_PracticeFatigue = 64900100;
        /*
        public enum ExChangeMenu
        {
            None = 0,
            Only_Sleepiness = 1,
            Only_Hunger = 2,
            Sleepiness_priority = 3,
            Hunger_priority = 4
        }
        */
        public enum LogShowMode
        { 
            Never, OnSuccess, Always
        }
    }

    [BepInPlugin(PluginSettings.GUID, PluginSettings.MOD_TITLE, PluginSettings.MOD_VERSION)]
    public class MainPlugin : BaseUnityPlugin
    {
        public static MyLogger myLogger;
        //private ConfigEntry<int> ConfigVersion;

        //entry---------------------------------------------------
        //internal static ConfigEntry<PluginSettings.ExChangeMenu> CE_ExChangeMenu;      //スタミナ消費代替の選択肢
        //internal static ConfigEntry<int> CE_SleepinessExchangeBaseRate; //睡眠代替発生確率の基本値
        internal static ConfigEntry<int> CE_SleepinessExchangeUpperLimit; //睡眠代替の上限
        //internal static ConfigEntry<int> CE_SleepinessExchangeLowerLimit; //睡眠代替の下限

        //internal static ConfigEntry<int> CE_HungerExchangeBaseRate;     //空腹代替発生確率の基本値
        internal static ConfigEntry<int> CE_HungerExchangeUpperLimit; //空腹代替の上限
        //internal static ConfigEntry<int> CE_HungerExchangeLowerLimit; //睡眠代替の下限
        //internal static ConfigEntry<int> CE_SleepinessExchangeRate; //睡眠代替の成功率の基本値
        //internal static ConfigEntry<int> CE_HungerExchangeRate; //空腹代替の成功率の基本値

        //internal static ConfigEntry<float> CE_SleepinessExchangeDecay; //睡眠代替の成功率の減衰値
        //internal static ConfigEntry<float> CE_HungerExchangeDecay; //空腹代替の成功率の減衰値

        //internal static ConfigEntry<bool> CE_SleepinessExchangeNoDecay; //睡眠代替の減衰を無効化
        //internal static ConfigEntry<bool> CE_HungerExchangeNoDecay; //空腹代替を無効化



        //internal static ConfigEntry<bool> CE_AddDeepInfo; //ログ表示に詳細な情報を付加するかどうか。
        //internal static ConfigEntry<bool> CE_ShowLogTweet;//デバッグ用のメソッドを表示するかどうか
        //internal static ConfigEntry<bool> CE_ShowLogInfo;//通常のログを表示するかどうか
        //internal static ConfigEntry<bool> CE_ShowLogWarning;//警告のログを表示するかどうか
        //internal static ConfigEntry<bool> CE_ShowLogError;//エラーのログを表示するかどうか

        //internal static ConfigEntry<bool> CE_SleepinessExchangeScale;   //睡眠代替が睡眠値に応じてスケールするかどうか
        //internal static ConfigEntry<bool> CE_HungerExchangeScale;       //空腹代替が空腹値に応じてスケールするかどうか
        
        //v2 add  TR:訓練でのスタミナ回復
        internal static ConfigEntry<int> CE_UpperLimitStaminaExchange;//スタミナがこれを超えている場合は変換できない[0 ~ 100]
        //internal static ConfigEntry<bool> CE_FatigueDelete; //MODの内部疲労度を消す
        internal static ConfigEntry<PluginSettings.LogShowMode> CE_ShowLogOnTrainingRecovery; //TRが行われた時にログを表示
        //private static ConfigEntry<float> CE_ConversionRateSleep; //TRにかかる倍率:睡眠
        private static ConfigEntry<float> CE_ConversionRateHunger; //TRのスタミナ回復量にかかる倍率
        private static ConfigEntry<int> CE_ConversionFrequency; //TRのスタミナ回復の頻度
        private static ConfigEntry<int> CE_SleepinessAdditionFrequency; //TR時に追加で睡眠度が増える頻度

        internal static class PluginConfig
        {
            //internal static float ConversionRateSleep =>
            //    Mathf.Clamp(CE_ConversionRateSleep.Value, 0f, 10f);
            internal static float ConversionRateHunger =>
                Mathf.Clamp(CE_ConversionRateHunger.Value, 0f, 10f);
            internal static int ConversionFrequency =>
                Mathf.Clamp(CE_ConversionFrequency.Value, 0, 10);
            internal static int SleepinessAdditionFreq =>
                Mathf.Clamp(CE_SleepinessAdditionFrequency.Value, 0, 10);

        }
        
        //loading-------------------------------------------------
        private void Start()
        {
            myLogger = new MyLogger()
            {
                myLogSource = base.Logger
            };
            LoadConfig();
            new Harmony(this.GetType().Name).PatchAll();

        }
        
        private void LoadConfig() 
        {
            CE_SleepinessExchangeUpperLimit = Config.BindItem(
                new ConfigItem<int>
                {
                    Section = "Sleepiness",
                    Key = "SleepinessUpperLimitOnTR",
                    Value = 80,
                    Description = "If your sleep level exceeds this value, you cannot recover. [0–100]",
                    DescriptionJP = "睡眠度がこの値を超えている場合は回復出来ない。[0 ～ 100]"
                }
            );
            CE_HungerExchangeUpperLimit = Config.BindItem(
                new ConfigItem<int>
                {
                    Section = "Hunger",
                    Key = "HungerUpperLimitOnTR",
                    Value = 60,
                    Description = "If your hunger level exceeds this value, you cannot recover. [0–100]",
                    DescriptionJP = "空腹度がこの値を超えている場合は回復出来ない。[0 ～ 100]"
                }
            );
            CE_UpperLimitStaminaExchange = Config.BindItem(
                new ConfigItem<int>
                {
                    Section = "Stamina",
                    Key = "UpperLimitStaminaOnTR",
                    Value = 80,
                    Description = "If your stamina exceeds this percentage, it cannot be restored. [%]",
                    DescriptionJP = "スタミナがこの割合を超えている場合は回復できない。[%]"
                }
            );
            /*
            CE_FatigueDelete = Config.BindItem(
                new ConfigItem<bool>
                {
                    Section = "Debug",
                    Key = "FatigueDelete",
                    Value = true,
                    Description = "a",
                    DescriptionJP = "v2で廃止する内部疲労度の値をリセットする。"
                }
            );
            */
            CE_ShowLogOnTrainingRecovery = Config.BindItem(
                new ConfigItem<PluginSettings.LogShowMode>
                {
                    Section = "Debug",
                    Key = "ShowLogOnTrainingRecovery",
                    Value = PluginSettings.LogShowMode.OnSuccess,
                    Description = "Whether to output logs during training recovery.",
                    DescriptionJP = "訓練回復時にログを出力するかどうか。"
                }
            );
            /*
            CE_ConversionRateSleep = Config.BindItem(
                new ConfigItem<float>
                {
                    Section = "StaminaRecovery",
                    Key = "ConversionRateSleep",
                    Value = 0.5f,
                    Description = "a",
                    DescriptionJP = "[float]睡眠変換時のスタミナ回復量の補正値。"
                }
            );
            */
            CE_ConversionRateHunger = Config.BindItem(
                new ConfigItem<float>
                {
                    Section = "TrainingRecovery",
                    Key = "ConversionRateHunger",
                    Value = 1f,
                    Description = "[float]Modifier for stamina recovery when converting hunger.",
                    DescriptionJP = "[float]空腹変換時のスタミナ回復量の補正値。"
                }
            );
            CE_ConversionFrequency = Config.BindItem(
                new ConfigItem<int>
                {
                    Section = "TrainingRecovery",
                    Key = "ConversionFrequency",
                    Value = 4,
                    Description = "[int] The frequency at which stamina is restored.",
                    DescriptionJP = "[int]スタミナ回復を発生させる頻度。"
                }
            );
            CE_SleepinessAdditionFrequency = Config.BindItem(
                new ConfigItem<int>
                {
                    Section = "TrainingRecovery",
                    Key = "SleepinessAdditionFrequency",
                    Value = 2,
                    Description = "[int] The frequency at which sleepiness increases.",
                    DescriptionJP = "[int]睡眠度が増加する頻度。"
                }
            );
        }
       
    }
}
