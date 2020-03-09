using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Study.Website.Controllers
{
    public class MetaData
    {
        public MetaData(string text)
        {
            var arr = text.Split(',');
            RelativePath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(arr[0].Split(' ')[1]));
            Name = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(arr[1].Split(' ')[1]));
            Type = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(arr[2].Split(' ')[1]));
            FileType = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(arr[3].Split(' ')[1]));
            FileName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(arr[4].Split(' ')[1]));
        }

        public string RelativePath { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string FileType { get; set; }

        public string FileName { get; set; }
    }

    [Route("[controller]")]
    [ApiController]
    public class TusFilesController : ControllerBase
    {
        private readonly string dir = null;

        public TusFilesController(IConfiguration configuration)
        {
            dir = configuration["TusFileSavePhysicPath"];
        }


        // GET: api/Files/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            //先判断文件是否是完整的
            var chunkcompletePath = System.IO.Path.Combine(dir, $"{id}.chunkcomplete");
            if (System.IO.File.Exists(chunkcompletePath))
            {
                //解析元数据
                var metadataPath = System.IO.Path.Combine(dir, $"{id}.metadata");
                var metadataText = await System.IO.File.ReadAllTextAsync(metadataPath).ConfigureAwait(false);
                MetaData metaData = new MetaData(metadataText);
                return PhysicalFile(System.IO.Path.Combine(dir, id), metaData.Type, metaData.Name, true);
            }
            return NotFound("没有发现文件或者文件长度为0");
        }
    }
}
