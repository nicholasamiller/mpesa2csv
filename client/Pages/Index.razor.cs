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
using iText.IO.Source;
using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;

namespace Mpesa2Csv
{
    public partial class Index
    {
        EditContext editContext;
        FormModel model;
        ValidationMessageStore validationMessageStore;
        bool converting = false;
        PdfDocument cachedPdfDoc;
        protected override void OnInitialized()
        {
            model = new FormModel();
            editContext = new EditContext(model);
            validationMessageStore = new ValidationMessageStore(editContext);
            editContext.OnValidationRequested += async (s, e) =>
            {
                {
                    validationMessageStore.Clear();
                    if (model.BrowserFile == null)
                    {
                        validationMessageStore.Add(() => model.BrowserFile,"Please select a file");
                    }
                    else
                    {
                        if (model.BrowserFile.ContentType != "application/pdf")
                        {
                            validationMessageStore.Add(() => model.BrowserFile, "Please select a PDF file");
                        }
                        else
                        {
                            if (model.BrowserFile.Size > 10000000)
                            {
                                validationMessageStore.Add(() => model.BrowserFile, "Please select a file less than 10MB");
                            }
                        }
                    }
                    await InvokeAsync(StateHasChanged);
                };
            };
        }

        private async Task OnValidSubmit()
        {
            converting = true;
            await Run(model.BrowserFile);
            converting = false;
        }

        private async Task UploadFile(InputFileChangeEventArgs e)
        {
            model.BrowserFile = e.File;
        }

        //private Task HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
        //{
        //    // todo: handle file too large issue
        //    validationMessageStore.Clear();
        //    var password = model.Password;
        //    var readerProperties = new ReaderProperties().SetPassword(Encoding.UTF8.GetBytes(password));
        //    try
        //    {
        //        // PdfReader class throw NotSupportedException given stream from Browserfile.
        //        var readStream = model.BrowserFile.OpenReadStream();
        //        var bytes = new byte[readStream.Length];
        //        readStream.ReadAsync(bytes, 0, (int)readStream.Length);
        //        var byteSource = new RandomAccessSourceFactory().CreateSource(bytes);
        //        var pdfReader = new PdfReader(byteSource, readerProperties);
        //        pdfReader.SetUnethicalReading(true);
        //        cachedPdfDoc = new PdfDocument(pdfReader);
        //    }
        //    catch (BadPasswordException ex)
        //    {
        //        validationMessageStore.Add(() => model.Password, "Invalid password");
        //    }
        //    catch (Exception ex)
        //    {
        //        validationMessageStore.Add(() => model.BrowserFile, "Invalid PDF file.");
        //    }
        //    finally
        //    {
        //        model.BrowserFile.OpenReadStream().Dispose();
        //    }
        //}

        private async Task Run(IBrowserFile file)
        {
            var csvName = Regex.Replace(file.Name, ".pdf$", ".csv");
            var convertedCsv = await ConvertToCsvAsync(cachedPdfDoc);
            await DownloadCsvAsync(convertedCsv, csvName);
        }

        private async Task<Stream> ConvertToCsvAsync(PdfDocument pdfDoc)
        {
            var resultStream = await Task.Run(() => Converter.ConvertToCsv(pdfDoc));
            return resultStream;
        }

        private async Task DownloadCsvAsync(Stream stream, string csvName)
        {
            using (var outputStream = new MemoryStream())
            {
                await stream.CopyToAsync(outputStream);
                await JSRuntime.InvokeVoidAsync("downloadFromByteArray", new
                {
                ByteArray = outputStream.ToArray(), FileName = csvName, ContentType = "application/pdf"
                }

                );
            }

            stream.Dispose();
        }

        public class FormModel
        {
            public string Password { get; set; }

            public IBrowserFile BrowserFile { get; set; }
        }
    }
}