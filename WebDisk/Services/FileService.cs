using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;
using WebDisk.Models;

namespace WebDisk.Services
{
    public class FileService
    {
        private readonly FileDbContext fileDbContext;
        private readonly string userId;

        public FileService(FileDbContext fileDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.fileDbContext = fileDbContext;
            userId = httpContextAccessor.HttpContext.User?.Claims.FirstOrDefault(e => e.Type == "sub")?.Value;
        }

      
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> AddFileAsync(AddFileDto dto)
        {
            await fileDbContext.Files.AddAsync(new Models.File
            {
                Id = GuidEx.NewGuid(),
                DirectoryId = long.Parse(dto.DirectoryId),
                UserId = userId,
                OriginFileName = dto.FileName,
                UploadFilePath = dto.Link,
                Size = dto.Size,
                ModifyDatetime = DateTimeOffset.Now,
                CreateTime = DateTimeOffset.Now,
                IsDeleted = false
            });

            int rows = await fileDbContext.SaveChangesAsync();
            return rows > 0;
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFileAsync(long fileId)
        {
            var file = await fileDbContext.Files.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == fileId);
            if (file != null)
            {
                //如果在回收站中被删除，则真正删除
                if (file.IsDeleted)
                {
                    fileDbContext.Files.Remove(file);
                }
                else
                {
                    //第一次删除
                    file.IsDeleted = true;
                }
              
                int rows = await fileDbContext.SaveChangesAsync();
                return rows > 0;
            }
            return false;
        }

        /// <summary>
        /// 修改文件名
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<bool> RenameFileAsync(long fileId, string newName)
        {
            var file = await fileDbContext.Files.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == fileId);
            if (file != null)
            {
                file.ModifyDatetime = DateTimeOffset.Now;
                file.OriginFileName = newName.Trim();
                int rows = await fileDbContext.SaveChangesAsync();
                return rows > 0;
            }
            return false;
        }


        /// <summary>
        /// 通过id获取文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<File> GetFileByIdAsync(long fileId)
        {
            var file = await fileDbContext.Files.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == fileId);
            return file;
        }


        /// <summary>
        /// 恢复文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task RecoveryFileAsync(long fileId)
        {
            var file = await fileDbContext.Files.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == fileId && e.IsDeleted);
            if (file != null)
            {
                file.IsDeleted = false;
                await fileDbContext.SaveChangesAsync();
            }
        }

    
    }
}
