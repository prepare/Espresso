//MIT, 2019, winterdev
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace OpenSslPrivateKeyUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            //this is just a help util
            //ref https://nodejs.org/dist/latest-v10.x/docs/api/http2.html
            //ref https://stackoverflow.com/questions/7360602/openssl-and-error-in-reading-openssl-conf-file
            //ref https://stackoverflow.com/questions/22327160/openssl-enter-export-password-to-generate-a-p12-certificate

            //We use openssl tool from nodejs

            //1. To generate the certificate and key for this example

            string openssl = @"D:\projects\espr_dev\node-v11.12.0\Release\openssl-cli.exe";
            string openssl_cnf = "openssl.cnf";

            {
                string open_ssl_args = @" req -config " + openssl_cnf + " -x509 -newkey rsa:2048 -nodes -sha256 -subj /CN=localhost/C=US/ST=AA/O=company -keyout localhost-privkey.pem -out localhost-cert.pem";

                ProcessStartInfo p_start = new ProcessStartInfo(openssl, open_ssl_args);
                p_start.WorkingDirectory = Directory.GetCurrentDirectory(); //create output in current dir, read openssl.cnf from current dir
                Process proc = Process.Start(p_start);
                proc.WaitForExit();
                int exitcode = proc.ExitCode;

            }
            //2. create p12 
            {
                //
                //EXAMPLE ONLY**** 
                //
                string open_ssl_args = @" pkcs12 -inkey localhost-privkey.pem  -in localhost-cert.pem -export -out mycert.pfx -password pass:12345";

                ProcessStartInfo p_start = new ProcessStartInfo(openssl, open_ssl_args);
                p_start.WorkingDirectory = Directory.GetCurrentDirectory(); //create output in current dir, read openssl.cnf from current dir
                Process proc = Process.Start(p_start);
                proc.WaitForExit();
                int exitcode = proc.ExitCode;
            }
        }
    }
}
