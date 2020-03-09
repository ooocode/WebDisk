using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebDisk.Services;
using WebDisk.Services.Req;

namespace WebDisk
{
    [Authorize]
    [IgnoreAntiforgeryToken]
    public class RecyclebinModel : PageModel
    {
        private readonly DirectoryService directoryService;
        private readonly FileService fileService;

        public RecyclebinModel(DirectoryService directoryService, FileService fileService)
        {
            this.directoryService = directoryService;
            this.fileService = fileService;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// 恢复文件夹
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRecoveryDirectoryAsync([FromQuery]string directoryId)
        {
            var result = await directoryService.RecoveryDirectoryAsync(long.Parse(directoryId));
            if (result)
            {
                return new OkResult();
            }
            return BadRequest();
        }


        /// <summary>
        /// 恢复文件
        /// </summary>
        /// <param name="directoryId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRecoveryFileAsync([FromQuery]string fileId)
        {
            await fileService.RecoveryFileAsync(long.Parse(fileId));
            return new OkResult();
        }
    }
}