using LineNotify_NBP.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace LineNotify_NBP.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CallBack()
        {
            
            return View();
        }


        /// <summary>
        /// 測試任務，每1小時(3600秒)執行一次
        /// IIS:
        /// 1.應用程式集區 -> 點選Web名稱(LineNotify_NBP)  ->進階設定(一般) 啟用模式選: AlwaysRuning
        /// 2.站台(LineNotify_NBP)->進階設定 (一般)-> 預先載入已啟用 : True
        /// </summary>
        [AutoTask(EnterMethod = "StartTask", IntervalSeconds = 3600, StartTime = "2019-07-16 12:00:00")]
        public class TestTask
        {
            public static void StartTask()
            {
                //要發送對象的Token(以下是亂打的Token,請自行更換),這裡是發送對象,多人時要額外用迴圈處理
                string Token = "ii2qe7rstArwSpbF6w6nrGDJW8kfqPGeeV8lvtOaeYqRs";

                //訊息內容 \r是換行 ,DateTime使用UtcNow可佈署於Azure抓取正確時間 
                string mag = "\r[" + DateTime.UtcNow.AddHours(08).ToString("yyyy-MM-dd HH:mm:ss") + "]";
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        wc.Encoding = Encoding.UTF8;
                        wc.Headers.Add("Authorization", "Bearer " + Token);
                       
                        NameValueCollection nc = new NameValueCollection();

                        //網址可從DB清單撈取, 這邊可以延伸其他應用再回覆通知....
                        if (UrlIsExist("https://tw.yahoo.com/"))
                        {
                            mag = mag + "\r[MY_Web]:連線正常";
                            nc["message"] += mag;
                        }
                        else
                        {
                            mag = mag + "\r[MY_Web]:斷線";
                            nc["message"] += mag;
                            nc["stickerPackageId"] = "1";  //可使用line貼圖 參考: https://devdocs.line.me/files/sticker_list.pdf
                            nc["stickerId"] = "135";
                        }

                        byte[] bResult = wc.UploadValues("https://notify-api.line.me/api/notify", nc);

                    }
                    catch
                    {
                    }
                }


                //Url 是否存在的方法(正式使用建議移到類別庫或Model_Servers呼叫使用, 不要放在Controller)
                bool UrlIsExist(string url)
                {
                    Uri u = null;
                    try
                    {
                        u = new Uri(url);
                    }
                    catch { return false; }
                    bool isExist = false;
                    HttpWebRequest r = WebRequest.Create(u) as HttpWebRequest;
                    r.Method = "HEAD";
                    try
                    {
                        HttpWebResponse s = r.GetResponse() as HttpWebResponse;
                        if (s.StatusCode == HttpStatusCode.OK)
                        {
                            isExist = true;
                        }
                    }
                    catch (WebException x)
                    {
                        try
                        {
                            isExist = ((x.Response as HttpWebResponse).StatusCode != HttpStatusCode.NotFound);
                        }
                        catch { isExist = (x.Status == WebExceptionStatus.Success); }
                    }
                    return isExist;

                }
            }


           
        }

      
    }


   



}