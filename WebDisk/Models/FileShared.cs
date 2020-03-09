using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Models
{
    /// <summary>
    /// 文件共享表
    /// </summary>
    public class FileShared
    {
        [Key]
        public long Id { get; set; }


        /// <summary>
        /// 文件id
        /// </summary>
        [Required]
        public long  FileId { get; set; }

        /// <summary>
        /// 提取码（为空则无提取码）
        /// </summary>
        public string Code  { get; set; }


        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTimeOffset EndDateTime { get; set; }
    }
}
