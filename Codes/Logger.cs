//using s649EnergyMod;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using InfoElement = s649.Logger.InfoElement;
#nullable enable
namespace s649.Logger
{
    public static class ListExtension
    {
        public static void AddAsInfo(this List<InfoElement> list, string text)
        {
            InfoElement info = Components.MakeInfoElement(text);
            list.Add(info);
        }
    }
    public class InfoElement
    {
        MyLogger.LogLevel level;
        object elm;
        public InfoElement(object o, MyLogger.LogLevel lv)
        {
            level = lv;
            elm = o;
        }
        public override string ToString()
        {
            return (level <= Components.MyLogLevel) ? 
                ((elm is Chara)? ((Chara)elm).NameSimple : elm.ToString()):
                "";
        }
    }
    public static class Components
    {
        public static MyLogger.LogLevel MyLogLevel;
        public static InfoElement MakeInfoElement(object obj, MyLogger.LogLevel lv = MyLogger.LogLevel.Info)
        {
            return new InfoElement(obj, lv);
        }
    }
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
        internal string callerClass = "";
        internal string topMethod = "";
        //private static string lastMethod = "";
        
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
        
        public void SetCallClass(string s)
        {   //初期化。HarmonyPatchのpreかpostで、メソッドの頭で必ず呼び出す。
            //preとpostで重複呼び出しはしない事。
            //stackHeader = new List<string> { s };
            //SetHeader(s);
            callerClass = s;
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
            string className = (callerClass != "") ? "[" + callerClass + "]" : "";
            return "[" + callerClass + "]" + ((topMethod == caller)? 
                "[" + topMethod + "]" :
                "[" + topMethod + "->" + caller + "]");
        }
        /*
        private string ArrayToString(string bind, object[] array)
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
        */
        private string ArrayToString(string bind, List<InfoElement> array)
        {
            //return string.Join(bind, array.Select(x => x?.ToString()));
            if (array == null || array.Count == 0) return "";
            
            return string.Join(bind, array.Select(x =>
            {
                if (x == null) return "";
                return string.Join(bind, array.Select(x => x?.ToString()));
                /*
                return x switch
                {
                    //int i => $"(int){i}",
                    string s => s,
                    //double d => $"{d:F2}", // 小数点2桁
                    Chara chara => chara.NameSimple,
                    _ => x.ToString()
                };
                */
            }));
            
        }

        public void LogTweet(string text, [CallerMemberName] string memberName = "")
        {
            Log(GetHeader(memberName) + text, LogLevel.Tweet);
        }

        public void LogTweet(List<InfoElement> objs, [CallerMemberName] string memberName = "")
        {
            string text = ArrayToString("/", objs);
            Log(GetHeader(memberName) + text, LogLevel.Tweet);
        }

        /*
        public static void LogDeep(string method, string text)
        {
            var caller = GetCallerMemberName();
            LogDeep(MergeCaller(caller, method) + text);
            //if (MyLogLevel <= LogLevel.Tweet) myLogSource?.LogInfo("[Tweet]" + text);
        }
        */
        public void LogDeep(string text, [CallerMemberName] string memberName = "")
        {
            Log(GetHeader(memberName) + text, LogLevel.Deep);
        }
        public void LogDeep(List<InfoElement> objs, [CallerMemberName] string memberName = "")
        {
            string text = ArrayToString("/", objs);
            Log(GetHeader(memberName) + text, LogLevel.Deep);
        }

        public void LogInfo(string text, [CallerMemberName] string memberName = "")
        {
            Log(GetHeader(memberName) + text, LogLevel.Info);
        }
        public void LogInfo(List<InfoElement> objs, [CallerMemberName] string memberName = "")
        {
            string text = ArrayToString("/", objs);
            Log(GetHeader(memberName) + text, LogLevel.Info);
        }
        
        public void LogWarning(string text, [CallerMemberName] string memberName = "")
        {
            Log(GetHeader(memberName) + text, LogLevel.Warning);
        }
        public void LogWarning(List<InfoElement> objs, [CallerMemberName] string memberName = "")
        {
            //var caller = GetCallerMemberName();
            string text = ArrayToString("/", objs);
            Log(GetHeader(memberName) + text, LogLevel.Warning);
        }
       
        public void LogError(string text, [CallerMemberName] string memberName = "")
        {
            //var caller = GetCallerMemberName();
            Log(GetHeader(memberName) + text, LogLevel.Error);
        }
        public void LogError(List<InfoElement> objs, [CallerMemberName] string memberName = "")
        {
            //var caller = GetCallerMemberName();
            string text = ArrayToString("/", objs);
            Log(GetHeader(memberName) + text, LogLevel.Error);
        }
        private void Log(string text, LogLevel lv)
        {
            if (Components.MyLogLevel <= lv)
            {
                switch (lv)
                {
                    case LogLevel.Tweet:
                        myLogSource?.LogInfo("[T]" + text);
                        break;
                    case LogLevel.Deep:
                        myLogSource?.LogInfo("[D]" + text);
                        break;
                    case LogLevel.Info:
                        myLogSource?.LogInfo(text);
                        break;
                    case LogLevel.Warning:
                        myLogSource?.LogWarning(text);
                        break;
                    case LogLevel.Error:
                        myLogSource?.LogError(text);
                        break;
                    case LogLevel.Fatal:
                        myLogSource?.LogError(text);
                        break;
                    default: break;
                }
            }
        }
        
        //private string GetCallerMemberName([CallerMemberName] string memberName = "")
        //{
        //    return memberName;
        //}
    }
}