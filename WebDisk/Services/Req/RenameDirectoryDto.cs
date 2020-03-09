using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Services.Req
{
    public class RenameDirectoryDto
    {
        [Required]
        public string DirectoryId  { get; set; }

        [Required]
        public string NewName  { get; set; }
    }
}
