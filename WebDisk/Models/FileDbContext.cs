using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDisk.Models
{
    public class FileDbContext:DbContext
    {
        public FileDbContext(DbContextOptions<FileDbContext> options)
            :base(options)
        {

        }

        //文件目录
        public DbSet<FileDirectory> FileDirectories { get; set; }

        //文件
        public DbSet<File> Files  { get; set; }

        /// <summary>
        /// 文件共享列表
        /// </summary>
        public DbSet<FileShared> FileShareds  { get; set; }
    }
}
