using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using MudBlazor;
using MPesa2Csv.Web;
using MPesa2Csv.Web.Shared;
using MPesa2Csv.Web.Dialogs;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;

namespace Mpesa2Csv
{
    public partial class Index
    {
        [Inject]
        public HttpClient _httpClient { get; set; }
        
        FormModel model;
        bool converting = false;
        protected override async void OnInitialized()
        {
            model = new FormModel();
        }

        private async Task OnValidSubmit()
        {
            converting = true;
            await InvokeAsync(StateHasChanged);
            var formDataContent = new MultipartFormDataContent();
            var password = model.Password;
            var fileContent = new StreamContent(model.BrowserFile.OpenReadStream());
            formDataContent.Add(fileContent, "file", model.BrowserFile.Name);
            formDataContent.Add(new StringContent(password), "password");
            var response = await _httpClient.PostAsync("api/Convert", formDataContent);
            // if error code shows invalid password, show a dialog with invalid password
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                if (error == "Invalid password")
                {
                    var parameters = new DialogParameters();
                    parameters.Add("Message", "Invalid password");
                    var dialog = DialogService.Show<ErrorDialog>("Error", parameters);
                    var result = await dialog.Result;
                }
            }
            else
            {
                var csvBytes = await response.Content.ReadAsByteArrayAsync();
                var csvName = Regex.Replace(model.BrowserFile.Name, ".pdf$", ".csv");
                // show a dialog with a download link
                var parameters = new DialogParameters();
                parameters.Add("CsvBytes", csvBytes);
                parameters.Add("CsvName", csvName);
                var dialog = DialogService.Show<DownloadDialog>("Download", parameters);
            }
            
            converting = false;
        }

        private async Task UploadFile(InputFileChangeEventArgs e)
        {
            model.BrowserFile = e.File;
        }
    }
}