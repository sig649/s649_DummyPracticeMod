//using s649EnergyMod;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
#nullable enable
namespace s649.Logger
{
    public class MyLogger
    {
        
        public BepInEx.Logging.ManualLogSource? myLogSource;
        public enum LogLevel
        {
            Tweet,
            Deep,
            Info,
            Warning,
            Error,
            Fatal
        }
        //private static List<string> stackHeader = new List<string> { };
        //private static List<string> _stackHeader;
        internal string callerPackage = "";
        internal string topMethod = "";
        //private static string lastMethod = "";
        public LogLevel MyLogLevel = LogLevel.Info;
        /*
        /// <summary>
        /// ＊このメソッドはCallerMemberNameを採用したため不要になりました＊
        /// ＊依然としてInitLogStackを呼んでおく必要がありますが......＊
        /// ※重要：親メソッドの頭ではInitLogStackを呼び出すこと※
        /// 子メソッドの先頭で呼び出し、ログ用のヘッダーを追加する。
        /// 子メソッドの終点でLogStackDumpを呼び出す必要がある。
        /// ※混乱を招くため処理の途中に追加してはいけない※
        /// </summary>
        /// <returns>setter用にstackHeaderを出力</returns>
        internal static List<string> LogStack(string argString)
        {
            _stackHeader = stackHeader;
            stackHeader.Add(argString);
            SetHeader();
            return stackHeader;
        }
        internal static void LogStackDump()
        {
            stackHeader = _stackHeader;
            SetHeader();
            //if (stackHeader.Count > 1) { stackHeader.RemoveAt(stackHeader.Count - 1); }
        }
        */

        public void SetPackageName(string s)
        {   //初期化。HarmonyPatchのpreかpostで、メソッドの頭で必ず呼び出す。
            //preとpostで重複呼び出しはしない事。
            //stackHeader = new List<string> { s };
            //SetHeader(s);
            callerPackage = s;
        }
        public void SetFookedMethod(string s)
        {   //初期化。HarmonyPatchのpreかpostで、メソッドの頭で必ず呼び出す。
            //preとpostで重複呼び出しはしない事。
            //stackHeader = new List<string> { s };
            //SetHeader(s);
            topMethod = s;
        }
        /*
        public static void InitLogStack(List<string> sList)
        {   //初期化。HarmonyPatchのpreかpostで、メソッドの頭で必ず呼び出す。
            //preとpostで重複呼び出しはしない事。
            stackHeader = sList;
            SetHeader();
        }
        */
        /*
        private static void SetHeader()
        {
            topMethod = string.Join(".", stackHeader);
            //logHeader = "[" + logHeader + "]";
        }
        private static void SetHeader(string s)
        {
            topMethod = s;
            //logHeader = "[" + logHeader + "]";
        }
        */
        /*
        public void SetLevel(LogLevel level)
        {
            MyLogLevel = level;
        }
        */
        public string GetHeader(string caller)
        {
            return (topMethod == caller)? 
                "[" + callerPackage + "/" + topMethod + "]" :
                "[" + callerPackage + "/" + topMethod + "->" + caller + "]";
        }
        
        public string ArrayToString(string bind, object[] array)
        {
            //return string.Join(bind, array.Select(x => x?.ToString()));
            return string.Join(bind, array.Select(x =>
            {
                if (x == null) return "-";

                return x switch
                {
                    //int i => $"(int){i}",
                    string s => s,
                    //double d => $"{d:F2}", // 小数点2桁
                    Chara chara => chara.NameSimple,
                    _ => x.ToString()
                };
            }));
        }
        public string ArrayToString(string bind, List<object> array)
        {
            //return string.Join(bind, array.Select(x => x?.ToString()));
            return string.Join(bind, array.Select(x =>
            {
                if (x == null) return "-";

                return x switch
                {
                    //int i => $"(int){i}",
                    string s => s,
                    //double d => $"{d:F2}", // 小数点2桁
                    Chara chara => chara.NameSimple,
                    _ => x.ToString()
                };
            }));
        }

        public void LogTweet(string text)
        {
            if (MyLogLevel <= LogLevel.Tweet) 
            {
                var caller = GetCallerMemberName();
                //var header = "[" + topMethod + "->" + caller + "]";
                myLogSource?.LogInfo("[Tweet]" + GetHeader(caller) + text);
            }
                
            //LogTweet(MergeCaller(caller, method) + text);
            //if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo("[Tweet]" + text);
        }
        /*
        private static void LogTweet(string text)
        {
            if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo(logHeader + "[Tweet]" + text);
        }
        */
        /*
        public static void LogDeep(string method, string text)
        {
            var caller = GetCallerMemberName();
            LogDeep(MergeCaller(caller, method) + text);
            //if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo("[Tweet]" + text);
        }
        */
        public void LogDeep(string text)
        {
            if (MyLogLevel <= LogLevel.Deep)
            {
                var caller = GetCallerMemberName();
                //var header = "[" + topMethod + "->" + caller + "]";
                myLogSource?.LogInfo("[Deep]" + GetHeader(caller) + text);
            }
            //myLogSource?.LogInfo(logHeader + "[Deep]" + text);
        }
        /*
        public static void LogInfo(string[] caller, string text)
        {
            string returnText = string.Join(".", caller);
            returnText = "[" + returnText + "]" + text;
            LogInfo(returnText);
            //if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo("[Tweet]" + text);
        }
        */
        /*
        public static void LogInfo(string method, string text)
        {
            var caller = GetCallerMemberName();
            LogInfo(MergeCaller(caller, method) + text);
            //if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo("[Tweet]" + text);
        }
        */
        public void LogInfo(string text)
        {
            if (MyLogLevel <= LogLevel.Info)
            {
                var caller = GetCallerMemberName();
                //var header = "[" + topMethod + "->" + caller + "]";
                myLogSource?.LogInfo(GetHeader(caller) + text);
            }
            //myLogSource?.LogInfo(logHeader + text);
        }
        public void LogInfo(List<object> objs)
        {
            var caller = GetCallerMemberName();
            string text = ArrayToString("/", objs);
            LogInfo(text, caller);
        }
        private void LogInfo(string text, string caller)
        {
            if (MyLogLevel <= LogLevel.Info)
                myLogSource?.LogInfo(GetHeader(caller) + text);
        }
        /*
        public static void LogWarning(string method, string text)
        {
            var caller = GetCallerMemberName();
            LogWarning(MergeCaller(caller, method) + text);
        }
        */
        public void LogWarning(string text)
        {
            if (MyLogLevel <= LogLevel.Warning)
            {
                var caller = GetCallerMemberName();
                //var header = "[" + topMethod + "->" + caller + "]";
                myLogSource?.LogWarning(GetHeader(caller) + text);
            }

            //myLogSource?.LogWarning(logHeader + text);
        }
        /*
        public static void LogError(string method, string text)
        {
            var caller = GetCallerMemberName();
            LogError(MergeCaller(caller, method) + text);
        }
        */
        public void LogError(string text)
        {
            var caller = GetCallerMemberName();
            //var header = "[" + topMethod + "->" + caller + "]";
            myLogSource?.LogError(GetHeader(caller) + text);
            //myLogSource?.LogError(logHeader + text);
        }
        public void LogError(List<object> objs)
        {
            var caller = GetCallerMemberName();
            string text = ArrayToString("/", objs);
            LogError(text, caller);
        }
        private void LogError(string text, string caller)
        {
            //var caller = GetCallerMemberName();
            //var header = "[" + topMethod + "->" + caller + "]";
            myLogSource?.LogError(GetHeader(caller) + text);
            //myLogSource?.LogError(logHeader + text);
        }
        /*
        private static string MergeCaller(string caller, string method)
        {
            string callerText = string.Join("->", new[] { caller, method });
            callerText = "[" + callerText + "]";
            return callerText;
        }
        */
        private string GetCallerMemberName([CallerMemberName] string memberName = "")
        {
            return memberName;
        }
    }
}