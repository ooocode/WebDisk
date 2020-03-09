using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Services.Req
{
    public class CreateSharedDto
    { 
        /// <summary>
      /// 文件id
      /// </summary>
        [Required]
        public string FileId { get; set; }

        /// <summary>
        /// 是否有提取码（为空则无提取码）
        /// </summary>
        public bool HadCode { get; set; }

        /// <summary>
        /// 共享天数 如果小于等于0 则无限期
        /// </summary>
        public int Days { get; set; }
    }
}
