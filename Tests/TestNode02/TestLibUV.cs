//MIT, 2020, WinterDev
//MIT, 2016-2017, EngineKit  (from SharpConnect.IO)

//test libuv
//some code from  https://github.com/dotnet/corefxlab, and SharpConnect.IO

using System;
using System.Text;
using System.Net.Libuv;
namespace TestApp01
{
    static class TestLibUV
    {
        public static void Test1()
        {
            //--------------------------------------------------------
            Console.WriteLine("EspressoND test libuv inside nodejs");
            Console.WriteLine("SharpConnect.IO says : hello!");
            //--------------------------------------------------------
            UVLoop uvLoop = new UVLoop();
            TcpListener tcpListener = new TcpListener("127.0.0.1", 8080, uvLoop);
            tcpListener.ConnectionAccepted += (Tcp newIncommingConn) =>
            {
                newIncommingConn.ReadCompleted += (arg) =>
                {
                    //get data --temp
                    string req = null;
                    unsafe
                    {
                        byte* dataH = (byte*)arg.UnsafePointer;
                        int dataLen = arg.Length;
                        byte[] copyData = new byte[dataLen];
                        System.Runtime.InteropServices.Marshal.Copy((IntPtr)dataH,
                            copyData, 0, dataLen
                            );
                        req = System.Text.Encoding.UTF8.GetString(copyData);
                    }
                    //--------------------------------------------------------
                    StringBuilder stbuilder = new StringBuilder();
                    stbuilder.Append("OK-12345");
                    newIncommingConn.TryWrite(
                        CreateTestResponse(stbuilder));

                    //--------------------------------------------------------
                };
                newIncommingConn.ReadStart();
            };
            tcpListener.Listen(); //start listening
            uvLoop.Run();

            //server wait for new incomming connection
            //-----------
            //try test by call http://127.0.0.1:8080 from web browser
            uvLoop.Stop();
        }

        //------------------------------------------------------------------------------------
        //test only
        static void HeaderAppendStatusCode(StringBuilder stBuilder, int statusCode)
        {
            switch (statusCode)
            {
                case 200:
                    stBuilder.Append("200 OK\r\n");
                    return;
                case 500:
                    stBuilder.Append("500 InternalServerError\r\n");
                    return;
                default:
                    //from 'Nowin' project
                    stBuilder.Append((byte)('0' + statusCode / 100));
                    stBuilder.Append((byte)('0' + statusCode / 10 % 10));
                    stBuilder.Append((byte)('0' + statusCode % 10));
                    stBuilder.Append("\r\n");
                    return;
            }
        }
        static void HeaderAppendConnectionType(StringBuilder headerStBuilder, bool keepAlive)
        {
            if (keepAlive)
                headerStBuilder.Append("Connection: keep-alive\r\n");
            else
                headerStBuilder.Append("Connection: close\r\n");
        }
        static string GetContentType(WebResponseContentType contentType)
        {
            //TODO: review here again
            switch (contentType)
            {
                case WebResponseContentType.ImageJpeg:
                    return "image/jpeg";
                case WebResponseContentType.ImagePng:
                    return "image/png";
                case WebResponseContentType.ApplicationOctetStream:
                    return "application/octet-stream";
                case WebResponseContentType.ApplicationJson:
                    return "application/json";
                case WebResponseContentType.TextXml:
                    return "text/xml";
                case WebResponseContentType.TextHtml:
                    return "text/html";
                case WebResponseContentType.TextJavascript:
                    return "text/javascript";
                case WebResponseContentType.TextPlain:
                    return "text/plain";
                default:
                    throw new NotSupportedException();
            }
        }
        /// <summary>
        /// content type
        /// </summary>
        public enum WebResponseContentType : byte
        {
            TextHtml,
            TextPlain,
            TextXml,
            TextJavascript,

            ImagePng,
            ImageJpeg,

            ApplicationOctetStream,
            ApplicationJson,
        }

        static byte[] CreateTestResponse(StringBuilder bodyPart)
        {
            StringBuilder headerStBuilder = new StringBuilder();
            headerStBuilder.Append("HTTP/1.1 ");
            HeaderAppendStatusCode(headerStBuilder, 200);
            HeaderAppendConnectionType(headerStBuilder, false);
            //--------------------------------------------------------------------------------------------------------
            headerStBuilder.Append("Content-Type: " + GetContentType(WebResponseContentType.TextPlain));
            headerStBuilder.Append(" ; charset=utf-8\r\n");
            //--------------------------------------------------------------------------------------------------------


            char[] body = bodyPart.ToString().ToCharArray();
            byte[] bodyBuffer = System.Text.Encoding.UTF8.GetBytes(body);
            int contentByteCount = bodyBuffer.Length;
            //--------------------------------------------------------------------------------------------------------

            headerStBuilder.Append("Content-Length: ");
            headerStBuilder.Append(contentByteCount);
            headerStBuilder.Append("\r\n");
            //-----------------------------------------------------------------                                    
            headerStBuilder.Append("\r\n");//end header part       

            var headBuffer = Encoding.UTF8.GetBytes(headerStBuilder.ToString().ToCharArray());
            byte[] dataToSend = new byte[headBuffer.Length + contentByteCount];
            Buffer.BlockCopy(headBuffer, 0, dataToSend, 0, headBuffer.Length);
            Buffer.BlockCopy(bodyBuffer, 0, dataToSend, headBuffer.Length, contentByteCount);

            return dataToSend;
        }
    }
}
