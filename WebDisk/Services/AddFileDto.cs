using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Services
{
    public class AddFileDto
    {
        public string Link { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// 大小（字节）
        /// </summary>
        public long Size { get; set; }


        /// <summary>
        /// 目录id
        /// </summary>
        public string DirectoryId { get; set; }
    }
}
