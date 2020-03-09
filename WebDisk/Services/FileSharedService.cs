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
    public class FileSharedService
    {
        private readonly FileDbContext fileDbContext;
        private readonly string userId;

        public FileSharedService(FileDbContext fileDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.fileDbContext = fileDbContext;
            userId = httpContextAccessor.HttpContext.User?.Claims.FirstOrDefault(e => e.Type == "sub")?.Value;
        }

        /// <summary>
        /// 获取共享文件
        /// </summary>
        /// <param name="sharedId"></param>
        /// <returns></returns>
        public async Task<FileShared> GetFileSharedAsync(GetFileSharedDto dto)
        {
            var shared = await fileDbContext.FileShareds.FirstOrDefaultAsync(e => e.Id == dto.SharedId);
            if(shared == null)
            {
                return null;
            }

            //如果需要提取码 
            if (!string.IsNullOrEmpty(shared.Code))
            {
                if(dto.Code != shared.Code)
                {
                    return null;
                }
            }
            return shared;
        }


        /// <summary>
        /// 共享文件是否需要授权码
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsSharedFileNeedCodeAysnc(long sharedId)
        {
            var shared = await fileDbContext.FileShareds.FirstOrDefaultAsync(e => e.Id == sharedId);
            return !string.IsNullOrEmpty(shared?.Code);
        }

        /// <summary>
        /// 创建文件共享
        /// </summary>
        /// <returns></returns>
        public async Task<FileShared> CreateSharedFileAsync(CreateSharedDto dto)
        {
            long fileId = long.Parse(dto.FileId);

            //先判断文件存不存在
            var exist = await fileDbContext.Files.AnyAsync(e => e.Id == fileId && e.UserId == userId);
            if (exist)
            {
                var sharedId = GuidEx.NewGuid();
                string code = string.Empty;
                if (dto.HadCode)
                {
                    code = (new Random()).Next(1000, 9999).ToString();
                }

                
                if (dto.Days <= 0)
                {
                    dto.Days = 3650;
                }

                fileDbContext.FileShareds.Add(new FileShared
                {
                    Id = sharedId,
                    Code = code,
                    EndDateTime = DateTimeOffset.Now.AddDays(dto.Days),
                    FileId = fileId
                });

                int row = await fileDbContext.SaveChangesAsync();

                return await GetFileSharedAsync(new GetFileSharedDto { Code = code, SharedId = sharedId });
            }
            return null;
        }


        /// <summary>
        /// 删除共享
        /// </summary>
        /// <param name="sharedId"></param>
        /// <returns></returns>
        public async Task<bool> DeletSharedAsync(long sharedId)
        {
            //共享文件
            var shared = await fileDbContext.FileShareds.FirstOrDefaultAsync(e => e.Id == sharedId);
            if (shared != null)
            {
                //是否是我的文件
                var isMyFile = await fileDbContext.Files.AnyAsync(e => e.Id == shared.FileId && e.UserId == userId);
                if (isMyFile)
                {
                    fileDbContext.FileShareds.Remove(shared);
                    int row = await fileDbContext.SaveChangesAsync();
                    return row > 0;
                }
            }
            return false;
        }
    }
}
