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
using iText.Kernel.Pdf;
using iText.IO.Source;
using iText.Kernel.Exceptions;

namespace Mpesa2Csv
{
    public partial class Index
    {
        [Inject]
        public IJSRuntime _jsRuntime { get; set; }
        
        [Inject]
        public HttpClient _httpClient { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        FormModel model;
        bool converting = false;
        protected override async void OnInitialized()
        {
            model = new FormModel();
        }

        private async Task OnValidSubmit()
        {
            converting = true;
           // await InvokeAsync(StateHasChanged);
            var password = model.Password;
            var fileName = Regex.Replace(model.BrowserFile.Name, @"\.(pdf|PDF)$", ".csv");
            // PdfReader class throw NotSupportedException given stream from Browserfile.
            using (var readStream = model.BrowserFile.OpenReadStream())
            {
                byte[] pdfBytesReadAsync = new byte[readStream.Length];
                await readStream.ReadAsync(pdfBytesReadAsync, 0, (int)readStream.Length);
                var byteSource = new RandomAccessSourceFactory().CreateSource(pdfBytesReadAsync);
                var readerProperties = new ReaderProperties().SetPassword(Encoding.UTF8.GetBytes(model.Password ?? ""));
                var pdfReader = await Task.Factory.StartNew(() => new PdfReader(byteSource, readerProperties));
                pdfReader.SetUnethicalReading(true);
                try
                {
                    var pdfDocument = new PdfDocument(pdfReader);
                    using (var convertedToCsvResult = await Task.Factory.StartNew(() => Converter.ConvertToCsv(pdfDocument)))
                    {
                        byte[] csvBytesReadAsync = new byte[convertedToCsvResult.Length];
                        await convertedToCsvResult.ReadAsync(csvBytesReadAsync, 0, (int)convertedToCsvResult.Length);
                        var dialogParameters = new DialogParameters();
                        dialogParameters.Add("FileName", fileName);
                        dialogParameters.Add("CsvBytes", csvBytesReadAsync);
                        var downloadDialogResult = DialogService.Show<DownloadDialog>("Download", dialogParameters);
                        
                    }
                }
                catch (BadPasswordException ex)
                {
                    //show the bad password dialog
                    var options = new DialogOptions { CloseOnEscapeKey = true,  };
                    var dialogParameters = new DialogParameters();
                    dialogParameters.Add("Message", "Try national ID");
                    var badPasswordDialogResult = DialogService.Show<ErrorDialog>("Bad Password", dialogParameters,  options);
                    
                }
                catch (PdfException ex)
                {
                    //show the bad password dialog
                    var options = new DialogOptions { CloseOnEscapeKey = true, };
                    var dialogParameters = new DialogParameters();
                    dialogParameters.Add("Message", "Invalid PDF");
                    var badPasswordDialogResult = DialogService.Show<ErrorDialog>("Can't read this PDF.",dialogParameters, options);
                }
                catch (PdfParsingException ex)
                {
                    var options = new DialogOptions { CloseOnEscapeKey = true, };
                    var dialogParameters = new DialogParameters();
                    dialogParameters.Add("Message", "Bad luck.  Has the format of the PDF account statement changed?");
                    var badPasswordDialogResult = DialogService.Show<ErrorDialog>("No Transactions Extracted", dialogParameters, options);
                }
                catch (Exception ex)
                {
                    //show the bad password dialog
                    var options = new DialogOptions { CloseOnEscapeKey = true, };
                    var dialogParameters = new DialogParameters();
                    dialogParameters.Add("Message", "Bad luck!");
                    var badPasswordDialogResult = DialogService.Show<ErrorDialog>("Something Unexpected Happened", dialogParameters, options);
                }

                converting = false;
            }
        }

        private async Task UploadFile(InputFileChangeEventArgs e)
        {
            model.BrowserFile = e.File;
        }
    }
}