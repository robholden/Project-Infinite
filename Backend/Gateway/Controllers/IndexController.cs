using System.Net;

using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[Route("/")]
[ApiController]
public class IndexController : ControllerBase
{
    public IndexController()
    {
    }

    [HttpGet]
    public ContentResult Get() => new()
    {
        Content = @"
            <!DOCTYPE html>
            <html lang='en'>
                <head>
                    <meta charset='UTF-8' />
                    <title>Snow Capture</title>

                    <style>
                        body {
                            padding-top: 40vh;
                            background-color: #252525;
                            text-align: center;
                            font-family: 'Courier New', Courier, monospace;
                            letter-spacing: 2px;
                        }

                        h1 {
                            font-size: 36px;
                            text-transform: uppercase;
                            margin: 0;
                            color: #fff;
                        }

                        p {
                            font-size: 14px;
                            color: #ccc;
                        }
                    </style>
                </head>
                <body>
                    <h1>Snow Capture</h1>
                    <p>What you were expecting to find?</p>
                </body>
            </html>
        ",
        StatusCode = (int)HttpStatusCode.OK,
        ContentType = "text/html"
    };
}