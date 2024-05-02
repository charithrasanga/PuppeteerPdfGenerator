using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Drawing;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Xml.Linq;

public class PdfService : IPdfService
{
    public async Task<byte[]> GeneratePdfAsync(GeneratePdfRequest request)
    {
        var pdfData = await ConvertHtmlToPdf(request.HtmlData);
        return pdfData;
    }

    private async Task<byte[]> ConvertHtmlToPdf(string htmlData)
    {
        var installedBrowser = await new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = Path.GetTempPath()
        }).DownloadAsync(BrowserTag.Stable);

        var browserOptions = new LaunchOptions
        {
            
            Headless = true,
            // disable unwanted chrome features to speed up the process
            Args = new[] {
                "--no-sandbox",
                "--disable-features=IsolateOrigins",
                "--disable-site-isolation-trials",
                "--autoplay-policy=user-gesture-required",
                "--disable-background-networking",
                "--disable-background-timer-throttling",
                "--disable-backgrounding-occluded-windows",
                "--disable-breakpad",
                "--disable-client-side-phishing-detection",
                "--disable-component-update",
                "--disable-default-apps",
                "--disable-dev-shm-usage",
                "--disable-domain-reliability",
                "--disable-extensions",
                "--disable-features=AudioServiceOutOfProcess",
                "--disable-hang-monitor",
                "--disable-ipc-flooding-protection",
                "--disable-notifications",
                "--disable-offer-store-unmasked-wallet-cards",
                "--disable-popup-blocking",
                "--disable-print-preview",
                "--disable-prompt-on-repost",
                "--disable-renderer-backgrounding",
                "--disable-setuid-sandbox",
                "--disable-speech-api",
                "--disable-sync",
                "--hide-scrollbars",
                "--ignore-gpu-blacklist",
                "--metrics-recording-only",
                "--mute-audio",
                "--no-default-browser-check",
                "--no-first-run",
                "--no-pings",
                "--no-zygote",
                "--password-store=basic",
                "--use-gl=swiftshader",
                "--use-mock-keychain"
                        },
            ExecutablePath = installedBrowser.GetExecutablePath(),
        };

        using var browser = await Puppeteer.LaunchAsync(browserOptions);
        using var page = await browser.NewPageAsync();
        //await page.SetViewportAsync(new ViewPortOptions
        //{
        //    Width = 1920,
        //    Height = 1080
        //});

        await page.SetContentAsync(htmlData);

        var headerTemplate = GetHeaderTemplate();
        var footerTemplate = GetFooterTemplate();

        var pdfOptions = new PdfOptions
        {
          //  Format = PaperFormat.A4, // If you're using a standard format but want to adjust width, you might need to switch to custom dimensions
            // For custom dimensions, use the Width and Height properties
           //  Width = "10in", // Example custom width. Adjust according to your needs.
            // Height = "11.7in", // Example custom height. Adjust if needed.
            PreferCSSPageSize = true,
            DisplayHeaderFooter = true,
            PrintBackground = true,
            HeaderTemplate = headerTemplate,
            FooterTemplate = footerTemplate,
            MarginOptions = new MarginOptions { Top = "1cm", Right = "0cm", Bottom = "2cm", Left = "0cm" }
        };

        var pdfData = await page.PdfDataAsync(pdfOptions);

        return pdfData;
    }

    private string GetHeaderTemplate()
    {
        return $@"
            <div style='color:#9298a2;font-size:10px;text-align:right;width:100%;padding-right:1cm;'>
                <span class='pageNumber'></span> of <span class='totalPages'></span>
            </div>";
    }


    private string GetFooterTemplate()
    {
        return @"
           <style>
            html{
                   -webkit-print -color-adjust: exact;
                }
        </style>
            <div style='display: flex; justify-content: space-between; width: 100%; background-color: green; border-top: 1px solid #eaeaed; padding: 10px 10px 10px 10px;'>
                <div style='flex: 1; text-align: center; color: #475061; font-size: 6px;'>
                    Copyright © 2013 - 2022 Huntington Mark, LLC. This Huntington Learning Center is franchised by AJ Squared Education LLC under a franchise agreement with Huntington Learning Centers, Inc.
                </div>
                <div style='flex: 1; text-align: center; color: #475061; font-size: 8px; font-weight: bold;'>
                    OUR MISSION IS TO GIVE EVERY STUDENT THE BEST EDUCATION POSSIBLE
                </div>
                <div style='flex: 1; text-align: center;'>
                <image style='width:100px; height:auto;' src='data:image/jpeg;base64,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' />
                </div>
            </div>
        ";
    }


}

