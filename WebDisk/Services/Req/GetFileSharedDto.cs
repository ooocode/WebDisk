using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Services.Req
{
    public class GetFileSharedDto
    {
        /// <summary>
        /// 共享id
        /// </summary>
        [Required]
        public long SharedId  { get; set; }


        /// <summary>
        /// 提取码
        /// </summary>
        public string Code { get; set; }
    }
}
