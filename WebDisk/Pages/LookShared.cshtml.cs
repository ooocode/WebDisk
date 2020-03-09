using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebDisk.Services;

namespace WebDisk
{
    public class GetSharedFileViewModel
    {
        [Required]
      
        public long SharedId  { get; set; }

        [Required(ErrorMessage = "请输入提取码")]
        [DisplayName("提取码")]
        public string Code  { get; set; }
    }


    public class LookSharedModel : PageModel
    {
        private readonly FileSharedService fileSharedService;

        public LookSharedModel(FileSharedService fileSharedService)
        {
            this.fileSharedService = fileSharedService;
        }

        /// <summary>
        /// 共享id
        /// </summary>
        [FromRoute(Name = "Id")]
        public long Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var isNeedCode = await fileSharedService.IsSharedFileNeedCodeAysnc(Id);
            if (!isNeedCode)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }
    }
}