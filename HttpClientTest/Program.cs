using Hamakaze;
using System;
using System.Threading;
using static System.Console;

namespace HttpClientTest {
    public static class Program {
        public static void Main(string[] args) {
            ResetColor();

            HttpClient.Instance.DefaultUserAgent = @"SharpChat/1.0";

            /*string[] commonMediaTypes = new[] {
                @"application/x-executable",
                @"application/graphql",
                @"application/javascript",
                @"application/x.fwif",
                @"application/json",
                @"application/ld+json",
                @"application/msword",
                @"application/pdf",
                @"application/sql",
                @"application/vnd.api+json",
                @"application/vnd.ms-excel",
                @"application/vnd.ms-powerpoint",
                @"application/vnd.oasis.opendocument.text",
                @"application/vnd.openxmlformats-officedocument.presentationml.presentation",
                @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"application/x-www-form-urlencoded",
                @"application/xml",
                @"application/zip",
                @"application/zstd",
                @"audio/mpeg",
                @"audio/ogg",
                @"image/gif",
                @"image/apng",
                @"image/flif",
                @"image/webp",
                @"image/x-mng",
                @"image/jpeg",
                @"image/png",
                @"multipart/form-data",
                @"text/css",
                @"text/csv",
                @"text/html",
                @"text/php",
                @"text/plain",
                @"text/xml",
                @"text/html; charset=utf-8",
            };

            Logger.Write(@"Testing Media Type parsing...");
            foreach(string mts in commonMediaTypes) {
                HttpMediaType hmt = HttpMediaType.Parse(mts);
                Logger.Write($@"O {mts}");
                Logger.Write($@"P {hmt}");
            }

            return;*/

            static void setForeground(ConsoleColor color) {
                ResetColor();
                ForegroundColor = color;
            }

            using ManualResetEvent mre = new(false);
            bool kill = false;
            string[] urls = {
                @"https://flashii.net/",
                @"https://flashii.net/changelog",
                @"https://abyss.flash.moe/",
                @"https://flashii.net/info/contact",
                @"https://flashii.net/news/",
                @"https://flash.moe/",
                @"https://flashii.net/forum/",
            };

            foreach(string url in urls) {
                // routine lifted out of satori
                string paramUrl = Uri.EscapeDataString(url);
                HttpClient.Send(
                    new HttpRequestMessage(HttpRequestMessage.GET, $@"https://mii.flashii.net/metadata?url={paramUrl}"),
                    onComplete: (task, res) => {
                        WriteLine($@"Connection: {task.Request.Connection}");
                        WriteLine($@"AcceptEncodings: {string.Join(@", ", task.Request.AcceptedEncodings)}");
                        WriteLine($@"IsSecure: {task.Request.IsSecure}");
                        WriteLine($@"RequestTarget: {task.Request.RequestTarget}");
                        WriteLine($@"UserAgent: {task.Request.UserAgent}");
                        WriteLine($@"ContentType: {task.Request.ContentType}");
                        WriteLine();

                        setForeground(ConsoleColor.Green);

                        WriteLine($@"Connection: {res.StatusCode}");
                        WriteLine($@"Connection: {res.StatusMessage}");
                        WriteLine($@"Connection: {res.Connection}");
                        WriteLine($@"ContentEncodings: {string.Join(@", ", res.ContentEncodings)}");
                        WriteLine($@"TransferEncodings: {string.Join(@", ", res.TransferEncodings)}");
                        WriteLine($@"Date: {res.Date}");
                        WriteLine($@"Server: {res.Server}");
                        WriteLine($@"ContentType: {res.ContentType}");
                        WriteLine();

                        /*if(res.HasBody) {
                            string line;
                            using StreamWriter sw = new StreamWriter(@"out.html", false, new UTF8Encoding(false));
                            using StreamReader sr = new StreamReader(res.Body, new UTF8Encoding(false), false, leaveOpen: true);
                            while((line = sr.ReadLine()) != null) {
                                //Logger.Debug(line);
                                sw.WriteLine(line);
                            }
                        }*/
                    },
                    onError: (task, ex) => {
                        setForeground(ConsoleColor.Red);
                        WriteLine(ex);
                    },
                    onCancel: task => {
                        setForeground(ConsoleColor.Yellow);
                        WriteLine(@"Cancelled.");
                    },
                    onDownloadProgress: (task, p, t) => {
                        setForeground(ConsoleColor.Blue);
                        WriteLine($@"Downloaded {p} bytes of {t} bytes.");
                    },
                    onUploadProgress: (task, p, t) => {
                        setForeground(ConsoleColor.Magenta);
                        WriteLine($@"Uploaded {p} bytes of {t} bytes.");
                    },
                    onStateChange: (task, s) => {
                        setForeground(ConsoleColor.White);
                        WriteLine($@"State changed: {s}");

                        if(!kill && (task.IsFinished || task.IsCancelled)) {
                            kill = true;
                            mre?.Set();
                        }
                    }
                );
            }

            mre.WaitOne();
            ResetColor();
        }
    }
}
