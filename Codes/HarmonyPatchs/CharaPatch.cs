using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using s649.Logger;
using s649_DummyPracticeMod;
using s649_DummyPracticeMod.Codes;
namespace s649_DummyTrainingMod.Codes.HarmonyPatchs
{
    
    [HarmonyPatch]
    internal class TickConditionsPatch
    {
        static int UpperLimitStaminaExchange => MainPlugin.CE_UpperLimitStaminaExchange.Value;
        private static MyLogger myLogger => MainPlugin.myLogger;
        internal static int BaseRecoverFrequency => MainPlugin.PluginConfig.ConversionFrequency;
        internal static int AdditionalSleepFreq => MainPlugin.PluginConfig.SleepinessAdditionFreq;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara), nameof(Chara.TickConditions))]
        public static void TickConditionsPostfix(Chara __instance)
        {
            Chara chara;
            
            bool isSleep = false;
            bool factioncheck = false;
            try
            {
                chara = __instance;
                isSleep = chara.HasCondition<ConSleep>();
                factioncheck = chara.IsPCFaction || chara.IsPCParty || chara.IsPC;
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("Error during faction or sleep check");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            //faction check
            if (!factioncheck) return;
            
            //status check
            if (isSleep) return;

            if (EClass.rnd(BaseRecoverFrequency) != 0) return;
            //Debug.Log($"TR_Chara:{chara.NameSimple}/SLP:{isSleep}/FAC[{factioncheck}]");


            //ai check
            AIAct aiAct;
            bool isRestrained;// = chara.isRestrained;
            bool residentAttacker = false;
            bool isTraining;
            try
            {
                aiAct = chara.ai;
                isTraining = aiAct is AI_PracticeDummy;
                isRestrained = aiAct is AI_Torture || chara.isRestrained;
                residentAttacker = chara.IsInCombat && HasActiveRestrainer(chara);
                
                if (chara.IsPC)
                {
                    if (isTraining || isRestrained) goto AIChecked;
                    return;
                }
                else
                {
                    if (isTraining || isRestrained) goto AIChecked;
                    if (residentAttacker && EClass.rnd(2) == 0)
                    {
                        goto AIChecked;
                    }
                    return;
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("error during ai check");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            
            
        AIChecked:
            //Debug.Log($"TR_Chara:{chara.NameSimple}_AI:{aiAct}_Tr:{isTraining}_bind:{chara.isRestrained}_RA:{residentAttacker}");
            //Debug.Log($"Chara check: {chara.NameSimple}/{aiAct}/{chara.isRestrained}/{chara.IsRestrainedResident}");
            /*
            if (MainPlugin.CE_FatigueDelete.Value && chara.GetFatigueValue() != 0)
            {
                chara.SetFatigue(0);
                Debug.Log($"Fatigue Reset / Chara : {chara.NameSimple}");
                Msg.SetColor(Color.yellow);
                Msg.SayRaw($"{chara.NameSimple}の内部疲労を消去しました。");
            }
            */

            //stamina check
            int maxStamina;
            int currentStamina;
            
            //int fatigueCheck;
            bool fatigueCheckResult;
            try
            {
                //stamina check
                maxStamina = chara.stamina.max;
                currentStamina = chara.stamina.value;
                if (currentStamina * 100 / maxStamina >= UpperLimitStaminaExchange) return;
                //int fatigueRate = (maxStamina - currentStamina) * 100 / maxStamina;//バニラの疲労度とは異なる。疲労するほどダウン
                //fatigueCheck = currentStamina * 10 / maxStamina;// 10 - fatigueRate / 10;
                fatigueCheckResult = currentStamina < 0 || EClass.rnd(currentStamina * 10 / maxStamina) == 0;
                //
                if (!fatigueCheckResult) return;
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("error during stamina check");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            //Debug.Log($"TR:Chara:{chara.NameSimple}_STChecked_[{currentStamina}/{maxStamina}_Limit:{UpperLimitStaminaExchange}");

            //hunger check
            bool charaIsAwaken;// = chara.HasCondition<ConAwakening>();
            bool isEuphoric;// = chara.HasCondition<ConEuphoric>();//高揚
            int hunger;
            float boostBase, boostMin, boostMax, consume;
            int convRest;
            bool hungerCheckResult;
            try 
            {
                charaIsAwaken = chara.HasCondition<ConAwakening>();
                isEuphoric = chara.HasCondition<ConEuphoric>();//高揚
                hunger = chara.hunger.value;
                if (hunger >= BepInProps.hungerExchangeUpperLimit) return;
                //if (hunger < BepInProps.hungerExchangeLowerLimit) return;

                
                // 各種ブースト
                boostBase = 1;
                boostMin = 0.5f;
                boostMax = 2;
                if (isRestrained) boostBase *= 2;
                if (isEuphoric) boostMax *= 3;
                if (charaIsAwaken) { boostMin = boostMax; }
                
                consume = boostBase * UnityEngine.Random.Range(boostMin, boostMax);
                //if (consume < 1) consume = 1; 下の方で処理
                
                convRest = 100 - hunger;
                if (consume > convRest) consume = convRest;
                hungerCheckResult = EClass.rnd(100) <= convRest;
                //Debug.Log($"TR:Chara[{chara.NameSimple}]_ConvRestCheck_Result[{hungerCheckResult}]_[{hosei}/{consume}/{convRest}]");
                if (!hungerCheckResult) return;
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("error during hunger check");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            //Debug.Log($"TR:Chara[{chara.NameSimple}]_HunPassed[{hunger}/{consume}_Limit:{BepInProps.hungerExchangeUpperLimit}]");

            //sleep check
            int sleep;
            bool sleepRestCheck;
            try 
            {
                sleep = chara.sleepiness.value;
                if (sleep >= BepInProps.sleeinessExchangeUpperLimit) return;
                //if (sleep < BepInProps.sleeinessExchangeLowerLimit) return;
                sleepRestCheck = EClass.rnd(100 + (100 - sleep) * 4) > 100;
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("error during sleep check");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            
            //Debug.Log($"TR:Chara[{chara.NameSimple}]_SlpPassed[{sleep}/{sleepRestCheck}]_Limit:{BepInProps.sleeinessExchangeUpperLimit}]");

            //recover execute
            float hosei;
            try
            {
                hosei = MainPlugin.PluginConfig.ConversionRateHunger;
                // 修正内容: Math.Clamp を ClassExtension.Clamp に置き換え（int 型の拡張メソッドとして利用）
                int recoveredS = ((int)(consume * hosei * maxStamina / 100)).Clamp(1, maxStamina);
                //if (consume < 1) consume = 1;
                int consumed = (int)consume;
                if (consumed < 1) consumed = 1;
                bool addSleep = false;
                if (sleepRestCheck)
                {
                    chara.hunger.Mod(consumed);
                    addSleep = !charaIsAwaken && EClass.rnd(AdditionalSleepFreq) == 0;
                    if (addSleep) chara.sleepiness.Mod(1);
                    chara.stamina.Mod(recoveredS);
                    Msg.SetColor("positive");//Msg.SetColor(Color.yellow);
                    string text = Lang.isJP ?
                        $"{chara.NameSimple}は{consumed}の満腹度を糧にして{recoveredS}のスタミナを補った。"
                        :
                        $"{chara.NameSimple} used the satiety from {consumed} to replenish {recoveredS} points of stamina.";
                    Msg.SayRaw(text);
                    // Debug.Log($"TR:Chara[{chara.NameSimple}]_Recovered[{recoveredS}]_Current[Sta:{chara.stamina.value}(-{consumed})/{maxStamina}]_+Hunger[{chara.hunger.value}(+{consumed})]_Awake[{charaIsAwaken}]");
                    PlayRecoverEffect(chara);
                }
                else
                {
                    if (charaIsAwaken)
                    {
                        Msg.SetColor("negative");//Msg.SetColor(Color.cyan);
                        //Msg.SayRaw($"{chara.NameSimple}は{consume}の満腹度を糧にして迫りくる眠気を克服した。");

                        string text = Lang.isJP ?
                        $"{chara.NameSimple}は{consume}の満腹度を糧にして、{consume}の眠気を克服した。"
                        :
                        $"{chara.NameSimple} used {consume}'s fullness as fuel to overcome {consume}'s drowsiness.";
                        Msg.SayRaw(text);
                        chara.hunger.Mod(consumed);
                        chara.sleepiness.Mod(-consumed);
                        //Debug.Log($"TR:Chara[{chara.NameSimple}]_+Hunger[{chara.hunger.value}(+{consumed})]_-Sleepiness[{chara.sleepiness.value}(-{consumed})]_Awake[{charaIsAwaken}]");
                    }
                    else
                    {
                        Msg.SetColor("negative"); //Msg.SetColor(Color.cyan);
                        //Msg.SayRaw($"{chara.NameSimple}は眠気を克服するために{consume}の満腹度を余分に消費した。");
                        string text = Lang.isJP ?
                        $"{chara.NameSimple}は眠気のせいで{consumed}の満腹度を浪費した。"
                        :
                        $"{chara.NameSimple} wasted {consumed}'s fullness due to drowsiness.";
                        Msg.SayRaw(text);

                        chara.hunger.Mod(consumed);
                        //chara.sleepiness.Mod(consume);
                        //Debug.Log($"TR:Chara[{chara.NameSimple}]_+Hunger:{chara.hunger.value}(+{consumed})_Sleepiness[{chara.sleepiness.value}]_Awake[{charaIsAwaken}]");
                    }

                    string debtext = $"TR:Chara[{chara.NameSimple}]";
                    if (sleepRestCheck) debtext += $"_Recovered[{recoveredS}]_ST[{chara.stamina.value}(-{consumed})/{maxStamina}]";
                    debtext += $"_+HUN[{chara.hunger.value}(+{consumed})]";
                    debtext += "_SLP[";
                    if (!charaIsAwaken && !sleepRestCheck)
                    {
                        debtext += $"{chara.sleepiness.value}]";
                    }
                    else
                    {
                        debtext += $"{chara.sleepiness.value}";
                        debtext += charaIsAwaken ? "(-" : "(+";
                        debtext += $"{consumed})]";
                    }
                    debtext += $"_Awake[{charaIsAwaken}]";
                    debtext += $"_bind[{isRestrained}]";
                    debtext += $"_Euph[{isEuphoric}]";
                    debtext += $"_ReAt[{residentAttacker}]";
                    Debug.Log(debtext);
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError("error during recover execute");
                Debug.Log(ex.Message);
                Debug.Log(ex.StackTrace);
                return;
            }
            
        }

        internal static void PlayRecoverEffect(Card card)
        {
            card.PlaySound("heal_tick", 1f, true);
            card.PlayEffect("heal_tick", true, 0f, default(Vector3));
        }
        internal static bool HasActiveRestrainer(Chara chara) 
        {
            if (chara.enemy == null) return false;
            Chara enemy = chara.enemy;
            if (enemy.isDead) return false;
            if (!enemy.IsAliveInCurrentZone) return false;
            if (!enemy.IsPCFaction) return false;
            if (!enemy.isRestrained) return false;
            if (!chara.CanSee(enemy)) return false;
            return true;
        }

    }
}
