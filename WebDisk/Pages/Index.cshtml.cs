using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Study.Website.Data;
using Utility;
using WebDisk.Models;
using WebDisk.Services;
using WebDisk.Services.Req;
using Z.EntityFramework;
using Z.EntityFramework.Plus;

namespace WebDisk.Pages
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FileDbContext fileDbContext;
        private readonly DirectoryService directoryService;
        private readonly FileService fileService;
        private readonly FileSharedService fileSharedService;

        public IndexModel(ILogger<IndexModel> logger, FileDbContext fileDbContext,
            DirectoryService directoryService,
            FileService fileService,
            FileSharedService fileSharedService)
        {
            _logger = logger;
            this.fileDbContext = fileDbContext;
            this.directoryService = directoryService;
            this.fileService = fileService;
            this.fileSharedService = fileSharedService;
        }



        public class LongToStringConverter : JsonConverter<long>
        {
            public override long Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    // try to parse number directly from bytes
                    ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                    if (Utf8Parser.TryParse(span, out long number, out int bytesConsumed) && span.Length == bytesConsumed)
                        return number;

                    // try to parse from a string if the above failed, this covers cases with other escaped/UTF characters
                    if (Int64.TryParse(reader.GetString(), out number))
                        return number;
                }

                // fallback to default handling
                return reader.GetInt64();
            }

            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        public class FileDirectoryViewModel
        {
            [JsonConverter(typeof(LongToStringConverter))]
            public long Id { get; set; }

            /// <summary>
            /// 用户id  每个用户都有自己的目录
            /// </summary>
            public string UserId { get; set; }


            /// <summary>
            /// 目录名称
            /// </summary>
            public string Name { get; set; }


            /// <summary>
            /// 父目录id
            /// </summary>
            [JsonConverter(typeof(LongToStringConverter))]
            public long PId { get; set; }


            public string Path { get; set; }


            /// <summary>
            /// 目录创建时间
            /// </summary>
            public DateTimeOffset CreateTime { get; set; }

            public List<FileDirectoryViewModel> Children { get; set; }
        }

        /// <summary>
        /// 数据库查出的数组形式转树结构
        /// </summary>
        /// <param name="fileDirectories"></param>
        /// <returns></returns>
        public List<FileDirectoryViewModel> BuildTree(List<FileDirectory> fileDirectories)
        {
            List<FileDirectoryViewModel> source = fileDirectories.Select(e => new FileDirectoryViewModel
            {
                UserId = e.UserId,
                CreateTime = e.CreateTime,
                Id = e.Id,
                Name = e.Name,
                PId = e.PId,
                Path = e.Path
            }).ToList();

            List<FileDirectoryViewModel> Tree = new List<FileDirectoryViewModel>();
            Dictionary<long, FileDirectoryViewModel> keyValuePairs = new Dictionary<long, FileDirectoryViewModel>();

            //list --> map
            source.ForEach(e => keyValuePairs[e.Id] = e);

            //转成tree
            source.ForEach(e =>
            {
                //e是当前节点

                //没有父目录 直接添加到树根
                if (e.PId == 0)
                {
                    Tree.Add(e);
                }
                else
                {
                    //找出父节点
                    if (keyValuePairs.ContainsKey(e.PId))
                    {
                        var parent = keyValuePairs[e.PId];
                        if (parent.Children == null)
                        {
                            parent.Children = new List<FileDirectoryViewModel>();
                        }
                        parent.Children.Add(e);
                    }
                }
            });
            return Tree;
        }


        /// <summary>
        /// 深度优先遍历树（栈 换成队列变成广度优先）
        /// </summary>
        /// <param name="">Action<FileDirectoryViewModel, FileDirectoryViewModel> 当前目录 上级目录</param>
        private void TravelTree(List<FileDirectoryViewModel> tree, Action<FileDirectoryViewModel> action)
        {
            foreach (var e in tree.Where(e => e.PId == 0))
            {
                Queue<FileDirectoryViewModel> stack = new Queue<FileDirectoryViewModel>();
                stack.Enqueue(e);
                while (stack.Count > 0)
                {
                    var front = stack.Dequeue();

                    if (front.PId == 0)
                    {
                        action(front);
                    }


                    if (front.Children != null)
                    {
                        foreach (var item in front.Children)
                        {
                            stack.Enqueue(item);
                            action(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 深度优先遍历树（栈 换成队列变成广度优先）
        /// </summary>
        /// <param name="">Action<FileDirectoryViewModel, FileDirectoryViewModel> 当前目录 上级目录</param>
        private IEnumerable<FileDirectory> TreeNodeToList(FileDirectoryViewModel node)
        {

            Queue<FileDirectoryViewModel> stack = new Queue<FileDirectoryViewModel>();
            stack.Enqueue(node);
            while (stack.Count > 0)
            {
                var front = stack.Dequeue();

                if (front.PId == 0)
                {
                    yield return new FileDirectory
                    {
                        Id = front.Id,
                        CreateTime = front.CreateTime,
                        Name = front.Name,
                        // PId = front.PId,
                        UserId = front.UserId
                    };
                }


                if (front.Children != null)
                {
                    foreach (var item in front.Children)
                    {
                        stack.Enqueue(item);
                        yield return new FileDirectory
                        {
                            Id = item.Id,
                            CreateTime = item.CreateTime,
                            Name = item.Name,
                            // PId = item.PId,
                            UserId = item.UserId
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 当前目录id
        /// </summary>
        [FromRoute(Name = "Id")]
        public long? Id { get; set; }
        public async Task OnGetAsync()
        {
            //Id = dir?.Id ?? 0;
            //string userId = User.Claims.FirstOrDefault(e => e.Type == "sub")?.Value;
            //if (!Id.HasValue)
            //{
            //    var dir = await fileDbContext.FileDirectories.FirstOrDefaultAsync(e => e.UserId == userId && e.PId == 0);
            //    Id = dir?.Id ?? 0;
            //}
        }


        //加载目录
        public async Task<IActionResult> OnGetDirectoriesAsync()
        {
            string userId = User.Claims.FirstOrDefault(e => e.Type == "sub")?.Value;

            var source = await fileDbContext.FileDirectories
                .Where(e => userId == e.UserId && e.IsDeleted == false).ToListAsync();

            var tree = BuildTree(source);

            return new JsonResult(tree);
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCreateDirectoryAsync([FromBody]AddDirectoryDto dto)
        {
            await directoryService.AddDirectoryAsync(dto);
            return new OkResult();
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDeleteDirectoryAsync([FromQuery]string directoryId)
        {
            int row = await directoryService.DeleteDirectoryAsync(long.Parse(directoryId));
            return Content(row.ToString());
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="upLoadFileInfo"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUploadFileAsync([FromBody]List<UpLoadFileInfo> upLoadFileInfos)
        {
            foreach (var upLoadFileInfo in upLoadFileInfos)
            {
                await fileService.AddFileAsync(new AddFileDto
                {
                    DirectoryId = (Id ?? 0).ToString(),
                    FileName = upLoadFileInfo.FileName,
                    Link = upLoadFileInfo.Link,
                    Size = upLoadFileInfo.Size
                });
            }
            return new OkResult();
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="upLoadFileInfo"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostDeleteFileAsync([FromQuery]string fileId)
        {
            await fileService.DeleteFileAsync(long.Parse(fileId));
            return new OkResult();
        }

        /// <summary>
        ///文件重命名
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRenameAsync([FromQuery]string fileId, [FromQuery] string newName)
        {
            await fileService.RenameFileAsync(long.Parse(fileId), newName);
            return new OkResult();
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetFileInfoAsync(string fileId)
        {
            var file = await fileService.GetFileByIdAsync(long.Parse(fileId));
            if (file == null)
            {
                return NotFound();
            }
            return new JsonResult(file);
        }


        /// <summary>
        /// 文件夹重命名
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRenameDirectoryAsync([FromBody]RenameDirectoryDto dto)
        {
            if (TryValidateModel(dto))
            {
                await directoryService.RenameDirectoryAsync(dto);
                return new OkResult();
            }
            return BadRequest();
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetDownloadFileAsync([FromQuery]string fileId)
        {
            var file = await fileService.GetFileByIdAsync(long.Parse(fileId));
            if (file != null)
            {
                return Redirect(file.UploadFilePath);
            }
            return BadRequest();
        }

        /// <summary>
        /// 创建文件共享
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCreateSharedAsync([FromBody]CreateSharedDto dto)
        {
            if (TryValidateModel(dto))
            {
                var shared = await fileSharedService.CreateSharedFileAsync(dto);
                if (shared != null)
                {
                    return new JsonResult(new
                    {
                        Code = shared.Code,
                        EndDateTime = shared.EndDateTime.ToString(),
                        FileId = shared.FileId.ToString(),
                        Id = shared.Id.ToString()
                    });
                }
            }

            return BadRequest();
        }
    }
}
