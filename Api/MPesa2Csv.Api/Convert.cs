using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MPesa2Csv.Api
{
    public static class Convert
    {
        // upload file, get file back
        // browser can save
        // return validation message with error code if password does not work
        [FunctionName("Convert")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";



            //private Task HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
            //{
            //    // todo: handle file too large issue
            //    validationMessageStore.Clear();
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


            //return new FileContentResult()

            throw new NotImplementedException();
        }
    }
}
