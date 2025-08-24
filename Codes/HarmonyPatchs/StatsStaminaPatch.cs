using System;
using System.Collections.Generic;
using HarmonyLib;
using s649.Logger;
using s649_DummyPracticeMod.Codes;
using static s649_DummyPracticeMod.PluginSettings;
//using static s649.Logger.MyLogger;
using UnityEngine;

namespace s649_DummyPracticeMod
{
    [HarmonyPatch]
    internal class StatsStaminaPatch
    {
        static Chara c_trainer;
        static AIAct aiAct;
        static int sleepiness;// = CC.sleepiness.GetValue();
        //int slpPhase;
        static int hunger;// = CC.hunger.GetValue();
        //int hngPhase;
        //int maxSleepiness;
        //int maxHunger;
        static int maxStamina;
        static int currentStamina;
        static int exchange;
        static int _fatigue;
        static int fatigueNext = PracticeFatigue.valueNext;
        static long fatigue;
        static bool doSleepinessExchange, doHungerExchange, sleepPriority;
        //private static readonly string modNS = "DPM";
        private static MyLogger myLogger => MainPlugin.myLogger;
        static List<object> checkThings;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatsStamina), "Mod")]
        public static bool Prefix(StatsStamina __instance, int a)
        {
            if (a >= 0 || BepInConfig.exchangeMenu == ExChangeMenu.None) return true;
            checkThings = new List<object>();
            ExChangeMenu menu = BepInConfig.exchangeMenu;
            myLogger.SetFookedMethod(nameof(StatsStamina.Mod));
            myLogger.SetCallClass(typeof(StatsStamina).Name);
            try
            {
                c_trainer = BaseStats.CC;
                aiAct = c_trainer.ai;
            }
            catch (NullReferenceException ex)
            {
                myLogger.LogError("CharaCheckFailed");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return true;
            }
            if (!(c_trainer.IsPC || c_trainer.IsPCParty || c_trainer.IsPCFaction) || !(aiAct is AI_PracticeDummy || aiAct is AI_Torture)) { return true; }

            
            //string checktext = "";

            try
            {
                
                sleepiness = c_trainer.sleepiness.GetValue();
                //slpPhase = c_trainer.sleepiness.GetPhase();
                hunger = c_trainer.hunger.GetValue();
                //hngPhase = c_trainer.hunger.GetPhase();
                maxStamina = c_trainer.stamina.max;
                currentStamina = c_trainer.stamina.value;
                checkThings = new List<object>
                {
                    
                    c_trainer,
                    aiAct,
                    sleepiness,
                    //slpPhase,
                    hunger,
                    a,
                    currentStamina,
                    maxStamina
                    //hngPhase
                };
                
                
            }
            catch (NullReferenceException ex)
            {
                myLogger.LogError("CharaInfoCheckFailed for NullPo");
                //checktext = myLogger.ArrayToString("/", checkThings);
                myLogger.LogError(checkThings);
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return true;
            }
            //先に疲労値を取得
            //fatigue蓄積
            //スタミナ消費割合に応じてfatigue増加(aが最大スタミナの1％につき1000の疲労)。fatigue1000(fatigueNextの値)毎に1の睡眠(空腹)値に変換
            exchange = 0;
            _fatigue = c_trainer.GetFatigueValue();
            //int fatigueNext = PracticeFatigue.valueNext;
            fatigue = (long)(Mathf.Abs(a) * fatigueNext * 100L / maxStamina);
            /*
             * 両方代替するモードで疲労蓄積を軽減するかどうかはとりあえず保留
            if (menu == ExChangeMenu.Sleepiness_priority || menu == ExChangeMenu.Hunger_priority)
            {
                fatigue /= 2;
            }
            */
           
            /*
            fatigue += _fatigue;
            if (fatigue >= fatigueNext)
            {
                exchange = fatigue / fatigueNext;
                fatigue -= exchange * fatigueNext;
            }
            
            c_trainer.SetFatigue(fatigue);
            if (exchange == 0)
            {
                checkThings.Add("fatigue:" + _fatigue + "->" + fatigue);
                myLogger.LogInfo(checkThings);
                return false;
            }
            */
            //sleepinessとhungerが代替可能かどうかチェック
            
            doSleepinessExchange = CanSleepinessExchange() && menu != ExChangeMenu.Only_Hunger;
            doHungerExchange = CanHungerExchange() && menu != ExChangeMenu.Only_Sleepiness;
            sleepPriority = (menu == ExChangeMenu.Only_Sleepiness || menu == ExChangeMenu.Sleepiness_priority);
            myLogger.LogDeep(new List<object> { doSleepinessExchange, doHungerExchange, sleepPriority });
            /*
            switch (menu)
            {
                case ExChangeMenu.None:
                    break;
                case ExChangeMenu.Only_Sleepiness:
                    doSleepinessExchange = CanSleepinessExchange();
                    sleepPriority = true;
                    break;
                case ExChangeMenu.Only_Hunger:
                    doHungerExchange = CanHungerExchange();
                    break;
                case ExChangeMenu.Sleepiness_priority:
                    doSleepinessExchange = CanSleepinessExchange();
                    doHungerExchange = CanHungerExchange();
                    sleepPriority = true;
                    break;
                case ExChangeMenu.Hunger_priority:
                    doSleepinessExchange = CanSleepinessExchange();
                    doHungerExchange = CanHungerExchange();
                    break;
            }
            */
            long tempFatigue = fatigue + _fatigue;
            if (doSleepinessExchange || doHungerExchange)
            {
                //exchange = FatigueSetOrConvert();
                checkThings.Add("fatigue:" + _fatigue + "->" + tempFatigue);
                
                if (sleepPriority)
                {
                    if (TrySleepinessExchange()) { return false; }
                    if (TryHungerExchange()) { return false; }
                } 
                else 
                {
                    if (TryHungerExchange()) { return false; }
                    if (TrySleepinessExchange()) { return false; }
                }
            }
            /*
            if (sleepPriority)
            {
                if (doSleepinessExchange) {
                    //fatigue加算
                    exchange = FatigueSetOrConvert();
                    //exchange実行
                    c_trainer.sleepiness.Mod(exchange);
                    checkThings.Add("sleepiness + " + exchange);
                    myLogger.LogInfo(checkThings);
                    return false;
                }
                if (doHungerExchange) {
                    //fatigue加算
                    exchange = FatigueSetOrConvert();
                    //exchange実行
                    c_trainer.hunger.Mod(exchange);
                    checkThings.Add("hunger + " + exchange);
                    myLogger.LogInfo(checkThings);
                    return false;
                }
            }
            else
            {
                if (doHungerExchange)
                {
                    //fatigue加算
                    exchange = FatigueSetOrConvert();
                    //exchange実行
                    c_trainer.hunger.Mod(exchange);
                    checkThings.Add("hunger + " + exchange);
                    myLogger.LogInfo(checkThings);
                    return false;
                }
                if (doSleepinessExchange)
                {
                    //fatigue加算
                    exchange = FatigueSetOrConvert();
                    //exchange実行
                    c_trainer.sleepiness.Mod(exchange);
                    checkThings.Add("sleepiness + " + exchange);
                    myLogger.LogInfo(checkThings);
                    return false;
                }
            }
            */
                /*
                if (sleepiness < maxSleepiness)
                {
                    if (hunger < maxHunger / 2)
                    {
                        if (hngPhase == 0 && EClass.rnd(2) == 0)//v0.2.1
                        {
                            c_trainer.hunger.Mod(1);
                            checkThings.Add("hunger:Plus");
                            checktext = myLogger.ArrayToString("/", checkThings);
                            myLogger.LogInfo(checktext);
                            return false;
                        }
                        //int seed = (hunger < maxHunger / 4) ? 5 : 10;
                        if (hngPhase <= 1 && EClass.rnd(4) == 0)
                        {
                            c_trainer.hunger.Mod(1);
                            checkThings.Add("hunger:Plus");
                            checktext = myLogger.ArrayToString("/", checkThings);
                            myLogger.LogInfo(checktext);
                            return false;
                        }
                        if (EClass.rnd(10) == 0)
                        {
                            c_trainer.hunger.Mod(1);
                            checkThings.Add("hunger:Plus");
                            checktext = myLogger.ArrayToString("/", checkThings);
                            myLogger.LogInfo(checktext);
                            return false;
                        }
                        //if (eval) { return false; }

                    }
                    if (EClass.rnd(Lower(c_trainer.LV + 1000, 5000)) >= 1000)//LVが高ければ眠気増加回避※MAX 80%
                    {
                        //eval = true;
                        checkThings.Add("Sleepiness:Eval"); //dt += "/Sleepiness:Eval";
                        checktext = myLogger.ArrayToString("/", checkThings);
                        myLogger.LogInfo(checktext);//Main.Lg(dt);
                        return false;
                    }
                    int seed2 = (sleepiness > maxSleepiness / 2) ? 2 : 4;
                    if (EClass.rnd(seed2) != 0)
                    {
                        c_trainer.sleepiness.Mod(1);
                        checkThings.Add("Sleepiness:Add"); //dt += "/Sleepiness:Add";
                        checktext = myLogger.ArrayToString("/", checkThings);
                        myLogger.LogInfo(checktext);//Main.Lg(dt);
                        return false;
                    }
                }
                */
                
            checkThings.Add("Exchange:failed"); //dt += "/Vanilla:StaminaDown";
            //checktext = myLogger.ArrayToString("/", checkThings);
            myLogger.LogInfo(checkThings);//Main.Lg(dt);
            return true;
        }

        //////////Prefix fin/////////////////////////////////////////////////////////////
        /*
        ///<summary>
        ///100 - border の確率[%]で成功。
        /// </summary>
        private bool Gatya(int border)
        {
            if (EClass.rnd(100) >= border) { return true; } else { return true; }
        }
        ///<summary>
        ///value(0->100)の値に応じてrateを乗算する。200%->50%の変動
        /// </summary>
        private int ScaleValue(int value, int rate)
        {
            //return ((((value / 25) - 2) * 25) + 100) * rate / 100;
            return (200 - value / 2 * 3) * rate / 100;
        }
        */
        private static bool TrySleepinessExchange()
        {
            if (doSleepinessExchange && EClass.rnd(sleepiness / (BepInConfig.sleeinessExchangeRate)) == 0)
            {
                exchange = FatigueSetOrConvert(0);
                c_trainer.sleepiness.Mod(exchange);
                checkThings.Add("sleepiness + " + exchange);
                myLogger.LogInfo(checkThings);
                return true;
            }
            return false;
        }
        private static bool TryHungerExchange()
        {
            if (doHungerExchange && EClass.rnd(hunger / (BepInConfig.hungerExchangeRate)) == 0)
            {
                exchange = FatigueSetOrConvert(1);
                c_trainer.hunger.Mod(exchange);
                checkThings.Add("hunger + " + exchange);
                myLogger.LogInfo(checkThings);
                return true;
            }
            return false;
        }
        /*
        private static bool IsEnableHungerExchange()
        {
            if (hunger > BepInConfig.hungerExchangeUpperLimit) return false;
            if (hunger < BepInConfig.hungerExchangeLowerLimit) return false;
            return true;
        }
        
        /*
        private static bool TryHungerExchange()
        {
            //int max = maxStamina;
            ////////int seed2 = (sleepiness > maxSleepiness / 2) ? 2 : 4;
            //sleepinessが代替可能な値かどうか
            
            if (!IsEnableHungerExchange()) { return false; }
            c_trainer.hunger.Mod(value);
            return true;
            /*
            int rate = BepInConfig.hungerExchangeBaseRate;
            int border = (BepInConfig.hungerExchangeScale) ? 100 - ScaleValue(hunger, rate) : 100 - rate;
            //判定する
            bool success = Gatya(border);

            if (success)
            {
                int num = a * 100 / max;
                c_trainer.hunger.Mod((num < -1) ? Mathf.Abs(num) : 1);
                //checkThings.Add("Sleepiness:Add"); //dt += "/Sleepiness:Add";
                //checktext = myLogger.ArrayToString("/", checkThings);
                //myLogger.LogInfo(checktext);//Main.Lg(dt);
                return true;
            }
            return false;
            */
        ////////////////////////////

        //}
        //exchange
        internal static bool CanHungerExchange()
        {
            //int sleepiness = chara.sleepiness.GetValue();
            if (!c_trainer.IsPC) return false;
            if (hunger >= BepInConfig.hungerExchangeUpperLimit) return false;
            if (hunger <= BepInConfig.hungerExchangeLowerLimit) return false;
            return true;
        }
        internal static bool CanSleepinessExchange()
        {
            //int sleepiness = chara.sleepiness.GetValue();
            if (!c_trainer.IsPC) return false;
            if (sleepiness >= BepInConfig.sleeinessExchangeUpperLimit) return false;
            if (sleepiness <= BepInConfig.sleeinessExchangeLowerLimit) return false;
            return true;
        }
        private static int FatigueSetOrConvert(int mode = 0)
        {
            int num = 0;
            int next = (mode == 0) ? fatigueNext : fatigueNext * 2;
            fatigue += _fatigue;
            if (fatigue >= next)
            {
                num = (int)(fatigue / next);
                fatigue -= num * next;
            }
            c_trainer.SetFatigue((int)fatigue);
            return num;
        }
        /*
        private static int Lower(int a, int b)
        {
            return a < b ? a : b;
        }
        */
    }
}
