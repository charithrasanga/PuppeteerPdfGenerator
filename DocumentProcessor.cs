using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace PuppeteerPdfGenerator
{
    public class DocumentProcessor
    {
        private readonly ILogger<DocumentProcessor> _logger;
        private readonly IPdfService _pdfService;

        public DocumentProcessor(ILogger<DocumentProcessor> log, IPdfService pdfService)
        {
            _logger = log;
            _pdfService = pdfService;
        }

        [FunctionName("GeneratePDF")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "generatepdf" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(GeneratePdfRequest), Required = true, Description = "The request body")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Generate PDF Request received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestData = JsonConvert.DeserializeObject<GeneratePdfRequest>(requestBody);

            // Call the PDF service to generate the PDF
            byte[] pdfBytes = await _pdfService.GeneratePdfAsync(requestData);

            // Return the PDF file
            return new FileContentResult(pdfBytes, "application/pdf");
        }

    }
}

