using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LineNotify_NBP
{
    /// <summary>
    /// 定時執行
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoTaskAttribute : Attribute
    {
        /// <summary>
        /// 入口程序
        /// </summary>
        public string EnterMethod { get; set; }
        /// <summary>
        /// 執行間隔秒數（未設置或0則只執行一次）
        /// </summary>
        public int IntervalSeconds { get; set; }
        /// <summary>
        /// 開始執行日期
        /// </summary>
        public string StartTime { get; set; }

        //保留對Timer 的引用，避免回收
        private static Dictionary<AutoTaskAttribute, Timer> timers = new Dictionary<AutoTaskAttribute, Timer>();
        /// <summary>
        /// Global.asax.cs 中调用
        /// </summary>
        public static void RegisterTask()
        {
            new Task(() => StartAutoTask()).Start();
        }
        /// <summary>
        /// 啟動定時任務
        /// </summary>
        private static void StartAutoTask()
        {
            var types = Assembly.GetExecutingAssembly().ExportedTypes.Where(t => Attribute.IsDefined(t, typeof(AutoTaskAttribute))).ToList();
            foreach (var t in types)
            {
                try
                {
                    var att = (AutoTaskAttribute)Attribute.GetCustomAttribute(t, typeof(AutoTaskAttribute));
                    if (att != null)
                    {
                        if (string.IsNullOrWhiteSpace(att.EnterMethod))
                        {
                            throw new Exception("未指定任務入口！EnterMethod");
                        }
                        var ins = Activator.CreateInstance(t);
                        var method = t.GetMethod(att.EnterMethod);

                        if (att.IntervalSeconds > 0)
                        {
                            int duetime = 0; //計算延遲時間

                            if (string.IsNullOrWhiteSpace(att.StartTime))
                            {
                                duetime = 1000;
                            }
                            else
                            {
                                var datetime = DateTime.Parse(att.StartTime);
                                if (DateTime.Now <= datetime)
                                {
                                    duetime = (int)(datetime - DateTime.Now).TotalSeconds * 1000;
                                }
                                else
                                {
                                    duetime = att.IntervalSeconds * 1000 - ((int)(DateTime.Now - datetime).TotalMilliseconds) % (att.IntervalSeconds * 1000);
                                }
                            }

                            timers.Add(att, new Timer((o) =>
                            {
                                method.Invoke(ins, null);
                            }, ins, duetime, att.IntervalSeconds * 1000));
                        }
                        else
                        {
                            method.Invoke(ins, null);
                        }
                    }

                }
                catch (Exception ex)
                {
                    //LogHelper.Error(t.FullName + " 任務啟動失敗", ex);
                }
            }
        }
    }
}