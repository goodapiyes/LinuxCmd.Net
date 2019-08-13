using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace LinuxCmd.Net.NetWork
{
    public class CommandHandler
    {
        public static string HeartBeatCmd()
        {
            //if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //    return "not support for current os";
            LinuxServerInfo info = new LinuxServerInfo();
            StringBuilder builder = new StringBuilder();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return $"{CommandTag.HeartBeat.GetValue()}|0|End";
            //测试
            //builder.AppendFormat("|{0}", "CentOS Linux release 7.6.1810");
            //builder.AppendFormat("|{0}", "9天 7小时 09分钟");
            //builder.AppendFormat("|{0}", "1.13,1.33,1.55");
            //builder.AppendFormat("|{0}", "10");
            //builder.AppendFormat("|{0}", "3.1");
            //builder.AppendFormat("|{0}", "1");
            //builder.AppendFormat("|{0}", "7686788,12555555");
            //builder.AppendFormat("|End");
            builder.AppendFormat("{0}|{1}", CommandTag.HeartBeat.GetValue(),1);
            builder.AppendFormat("|{0}", info.OSName);
            builder.AppendFormat("|{0}", info.RunTime);
            builder.AppendFormat("|{0}", info.LoadAverages);
            builder.AppendFormat("|{0}", info.Cpu.CpuUsage);
            builder.AppendFormat("|{0}", info.Mem?.MemUsage);
            builder.AppendFormat("|{0}", info.Disk?.UseUsage);
            builder.AppendFormat("|{0},{1}", info.IO?.ReadBytes, info.IO?.WriteBytes);
            builder.AppendFormat("|End");
            return builder.ToString();
        }

        public static string GetCmdName(string data)
        {
            if (!data.Contains('|'))
                return null;
            return data.Split('|')[0];
        }
        public static bool GetCmdValid(string data)
        {
            if (!data.Contains('|'))
                return false;
            return data.Split('|')[1].Contains("1");
        }
    }

    public enum CommandTag
    {
        HeartBeat = 1001
    }

    public static class Extensions
    {
        public static int GetValue(this Enum instance)
        {
            return (int)System.Enum.Parse(instance.GetType(), instance.ToString(), true);
        }

        public static T ExToObject<T>(this string json)
        {
            return ToObject<T>(json);
        }

        public static string ExToJson(this object obj)
        {
            return ToJson(obj);
        }

        /// <summary>
        ///     将Json字符串转换为对象
        /// </summary>
        /// <param name="json">Json字符串</param>
        public static T ToObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///     将对象转换为Json字符串
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="isConvertToSingleQuotes">是否将双引号转成单引号</param>
        public static string ToJson(object target, bool isConvertToSingleQuotes = false)
        {
            if (target == null)
                return "{}";
            var result = JsonConvert.SerializeObject(target);
            if (isConvertToSingleQuotes)
                result = result.Replace("\"", "'");
            return result;
        }
    }

}