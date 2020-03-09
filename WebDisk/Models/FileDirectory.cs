using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Models
{
    /// <summary>
    /// 目录
    /// </summary>
    public class FileDirectory
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 用户id  每个用户都有自己的目录
        /// </summary>
        [Required]
        public string UserId { get; set; }


        /// <summary>
        /// 目录名称
        /// </summary>
        [Required] 
        public string Name { get; set; }


        /// <summary>
        /// 父节点id
        /// </summary>
        [Required]
        public long PId  { get; set; }


        /// <summary>
        /// 全路径
        /// </summary>
        [Required]
        public string Path  { get; set; }

        /// <summary>
        /// 目录创建时间
        /// </summary>
        [Required]
        public DateTimeOffset CreateTime { get; set; }


        /// <summary>
        /// 修改时间
        /// </summary>
        [Required]
        public DateTimeOffset ModifyDatetime { get; set; }

        /// <summary>
        /// 是否被删除了（如果是，则表明在回收站）
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
