using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Services
{
    public class AddDirectoryDto
    {
        /// <summary>
        /// 目录名称
        /// </summary>
        [Required]
        public string Name  { get; set; }


        /// <summary>
        /// 目录名称
        /// </summary>
        [Required]
        public string PId { get; set; }
    }
}
