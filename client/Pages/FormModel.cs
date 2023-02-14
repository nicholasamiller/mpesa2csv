using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Mpesa2Csv
{
    public partial class Index
    {
        public class FormModel
        {
            public string Password { get; set; }

            [Required(ErrorMessage = "File required.")]
            public IBrowserFile BrowserFile { get; set; }
        }
    }
}