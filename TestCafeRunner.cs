using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Text;
using System.IO;

namespace TestCafeRunner
{
    public struct BrowserInfo {
        public BrowserInfo(string path, string cmd) {
            this.path = path;
            this.cmd = cmd;
            this.alias = String.Empty;
        }

        public BrowserInfo(string alias) {
            this.path = String.Empty;
            this.cmd = String.Empty;
            this.alias = alias;
        }

        public string path;
        public string cmd;
        public string alias;
    }

    public static class TestCafeRunner
    {
        static string Execute(string fileName, string[] args) {
            return Execute(fileName, args, String.Empty);
        }

        static string Execute(string fileName, string[] args, string data) {
            var arg = String.Join(" ", args);
            string stdout = String.Empty;
            string stderr = String.Empty;

            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                Arguments = arg,
                ErrorDialog = false
            };

            using(var p = Process.Start(processStartInfo)) {
                if(data != String.Empty) {
                    p.StandardInput.Write(data);
                }

                stdout = p.StandardOutput.ReadToEnd();
                
                p.WaitForExit();

                if(p.ExitCode != 0) {
                    Console.WriteLine(stdout);
                    Environment.Exit(p.ExitCode);
                }
            };

            return stdout;
        }

        static string JsonToNUnit (string jsonReport) {
            string execFilePath = "node";
            string jsonToNUnitPath = Path.Combine(Environment.CurrentDirectory, "js", "json-to-nunit.js");

            return Execute(execFilePath, new string[1]{jsonToNUnitPath}, jsonReport);
        }


        public static string RunTests(string testCafeFolder, string testsFolder, string[] browsers) {
            string execFilePath = "node";
            string testCafeBinPath = Path.Combine(testCafeFolder, "bin", "testcafe");
            string options = String.Join(" ", new string[3]{
                "--hostname 127.0.0.1",
                "--reporter json",
                "--ports 1337,1338"
            });
            string browserList = String.Join(",", browsers);
            string tests = Path.Combine(testsFolder, "**", "*.test.js");

            string jsonReport = Execute(execFilePath, new String[4]{testCafeBinPath, options, browserList, tests});

            return JsonToNUnit(jsonReport);
        }

        public static string RunTests(string testCafeFolder, string testsFolder, BrowserInfo[] browsers) {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(BrowserInfo[]));
            MemoryStream jsonStream = new MemoryStream();
            StreamReader jsonStreamReader = new StreamReader(jsonStream);

            jsonSerializer.WriteObject(jsonStream, browsers);
            jsonStream.Position = 0;

            string execFilePath = "node";
            string runTestCafeJsPath = Path.Combine(Environment.CurrentDirectory, "js", "run-tests.js");
            string browsersJSON = jsonStreamReader.ReadToEnd().Replace("\"", "\\\"");
            string jsonReport = Execute(execFilePath, new String[4]{runTestCafeJsPath, testCafeFolder, testsFolder, browsersJSON});

            return JsonToNUnit(jsonReport);
        }
    }
}
