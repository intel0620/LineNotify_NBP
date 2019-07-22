using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace LineNotify_NBP.Models
{

    public class UploadFile
    {
        public UploadFile()
        {
            ContentType = "multipart/form-data";
        }
        public string Name { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public FileStream Stream { get; set; }
    }

}