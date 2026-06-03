using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
namespace s649_Config
{
    public class ConfigItem<T>
    {
        public string Section;
        public string Key;
        public T Value;
        public string Description;
        public string DescriptionJP = "";
    }
    public static class ConfigFileExtension
    {
        public static ConfigEntry<T> BindItem<T>(this ConfigFile configfile, ConfigItem<T> item)
        {
            var descript = Lang.isJP ? item.DescriptionJP : item.Description;
            return configfile.Bind(item.Section, item.Key, item.Value, descript);
        }
    }
}
