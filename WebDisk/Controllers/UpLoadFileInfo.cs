using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Study.Website.Data
{
    public class UpLoadFileInfo
    {
        public string Link { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// 大小（字节）
        /// </summary>
        public long Size { get; set; }
    }
}
