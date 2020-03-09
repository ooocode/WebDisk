using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Models
{
    public class File
    {
        [Key]
        public long Id  { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public string UserId { get; set; }


        /// <summary>
        /// 原始文件名
        /// </summary>
        [Required] 
        public string OriginFileName  { get; set; }

        /// <summary>
        /// 上传路径
        /// </summary>
        [Required]
        public string UploadFilePath  { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        [Required]
        public long Size  { get; set; }

        /// <summary>
        /// 目录id
        /// </summary>
        [Required] 
        public long DirectoryId  { get; set; }

        /// <summary>
        /// 目录创建时间
        /// </summary>
        [Required]
        public DateTimeOffset CreateTime { get; set; }


        /// <summary>
        /// 修改时间
        /// </summary>
        [Required]
        public DateTimeOffset ModifyDatetime  { get; set; }

        /// <summary>
        /// 是否被删除了（如果是，则表明在回收站）
        /// </summary>
        public bool IsDeleted  { get; set; }
    }
}
