using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;
using WebDisk.Models;
using WebDisk.Services.Req;

namespace WebDisk.Services
{
    public class DirectoryService
    {
        private readonly FileDbContext fileDbContext;
        private readonly string userId;

        public DirectoryService(FileDbContext fileDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.fileDbContext = fileDbContext;
            userId = httpContextAccessor.HttpContext.User?.Claims.FirstOrDefault(e => e.Type == "sub")?.Value;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddDirectoryAsync(AddDirectoryDto dto)
        {
            var id = GuidEx.NewGuid();

            if(!long.TryParse(dto.PId,out long pid))
            {
                pid = 0;
            }

            string path = null;
            var parent = await fileDbContext.FileDirectories.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == pid);
            if (parent == null)
            {
                //pid = 0
                path = $"0|{id}";
            }
            else
            {
                path = $"{parent.Path}|{id}";
            }

            FileDirectory fileDirectory = new FileDirectory
            {
                Id = id,
                CreateTime = DateTimeOffset.Now,
                Name = dto.Name,
                PId = pid,
                Path = path,
                UserId = userId,
                IsDeleted = false,
                ModifyDatetime = DateTimeOffset.Now
            };

            await fileDbContext.FileDirectories.AddAsync(fileDirectory);
            await fileDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 重命名目录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task RenameDirectoryAsync(RenameDirectoryDto dto)
        {
            var dirId = long.Parse(dto.DirectoryId);
            var dir = await fileDbContext.FileDirectories.FirstOrDefaultAsync(e => e.Id == dirId && e.UserId == userId);
            if (dir != null)
            {
                dir.ModifyDatetime = DateTimeOffset.Now;
                dir.Name = dto.NewName.Trim();
                await fileDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="directoryId"></param>
        public async Task<int> DeleteDirectoryAsync(long directoryId)
        {
            var dirs = fileDbContext.FileDirectories.Where(e => e.UserId == userId && e.Path.Contains(directoryId.ToString()));
            foreach (var item in dirs)
            {
                //在回收站了 则真正删除
                if (item.IsDeleted)
                {
                    fileDbContext.FileDirectories.Remove(item);
                }
                else
                {
                    item.IsDeleted = true;
                }
            }
           
            return await fileDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取当前目录的下一级目录
        /// </summary>
        public async Task<List<FileDirectory>> GetSubDirectoriesAsync(long curDirectoryId)
        {
            List<FileDirectory> dirs = await fileDbContext.FileDirectories
                .Where(e => e.UserId == userId && e.PId == curDirectoryId && e.IsDeleted == false)
                .ToListAsync();

            return dirs;
        }


        /// <summary>
        /// 获取当前目录的上一级目录
        /// </summary>
        public async Task<FileDirectory> GetParentDirectoryAsync(long curDirectoryId)
        {
            FileDirectory dir = await fileDbContext.FileDirectories
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == curDirectoryId);
            if (dir != null)
            {
                var perent = await fileDbContext.FileDirectories
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Id == dir.PId);
                return perent;
            }
            return null;
        }

        /// <summary>
        /// 从回收站恢复文件夹
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        public async Task<bool> RecoveryDirectoryAsync(long directoryId)
        {
            var dir = await fileDbContext.FileDirectories.FirstOrDefaultAsync(e => e.Id == directoryId && e.UserId == userId && e.IsDeleted);
            if (dir != null)
            {
                dir.IsDeleted = false;
                int row = await fileDbContext.SaveChangesAsync();
                return row > 0;
            }
            return false;
        }

        /// <summary>
        /// 获取目录下的所有文件
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        public async Task<List<File>> GetDirectoryFilesAsync(long directoryId)
        {
            var files = await fileDbContext.Files.Where(e => e.DirectoryId == directoryId && e.UserId == userId && e.IsDeleted == false)
                .ToListAsync();
            return files;
        }
    }
}
