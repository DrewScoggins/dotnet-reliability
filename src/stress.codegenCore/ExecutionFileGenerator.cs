// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using stress.execution;
using stress.codegen.resources;

namespace stress.codegen
{
    public class ExecutionFileGeneratorLinux : ISourceFileGenerator
    {
        private const string STRESS_SCRIPT_NAME = "stress.sh";
        /*
         * scriptName - name of the script to be generated, this should include the path
         * testName - the name of the test executable
         * arguments - arguments to be passed to the test executable
         * envVars - dictionary of environment variables and their values
         * host (optional) - needed for hosted runtimes
         */
        public void GenerateSourceFile(LoadTestInfo loadTestInfo)
        {
            string shellScriptPath = Path.Combine(loadTestInfo.SourceDirectory, STRESS_SCRIPT_NAME);

            using (TextWriter stressScript = new StreamWriter(shellScriptPath, false))
            {
                // set the line ending for shell scripts
                stressScript.NewLine = "\n";

                stressScript.WriteLine("#!/bin/sh");
                stressScript.WriteLine();
                stressScript.WriteLine();

                stressScript.WriteLine("# stress script for {0}", loadTestInfo.TestName);
                stressScript.WriteLine();
                stressScript.WriteLine();

                stressScript.WriteLine("# environment section");
                // first take care of the environment variables

                //if the environment variable "DUMPLING_PROPERTIES" already exists remove it
                //this is needed because multiple LoadTestInfo objects use the same environment dictionary instance
                if(loadTestInfo.EnvironmentVariables.ContainsKey("DUMPLING_PROPERTIES"))
                {
                    loadTestInfo.EnvironmentVariables.Remove("DUMPLING_PROPERTIES");
                }

                // add environment variables STRESS_TESTID, STRESS_BUILDID
                loadTestInfo.EnvironmentVariables["STRESS_TESTID"] = loadTestInfo.TestName;
                loadTestInfo.EnvironmentVariables["STRESS_BUILDID"] = loadTestInfo.TestName.Split('_')[0];

                // build a string of all the env var pairs to be passed as properties to dumpling 
                StringBuilder envVarPropertiesBuilder = new StringBuilder();

                foreach (var kvp in loadTestInfo.EnvironmentVariables)
                {
                    envVarPropertiesBuilder.Append(kvp.Key + "=" + kvp.Value + " ");
                }

                // add this string to the env vars as DUMPLING_PROPERTIES trimming the last space
                loadTestInfo.EnvironmentVariables["DUMPLING_PROPERTIES"] = "\"" + envVarPropertiesBuilder.ToString().TrimEnd(' ') + "\"";

                //write commands to setup environment environment 
                foreach (KeyValuePair<string, string> kvp in loadTestInfo.EnvironmentVariables)
                {
                    stressScript.WriteLine("export {0}={1}", kvp.Key, kvp.Value);
                }
                stressScript.WriteLine();
                stressScript.WriteLine();

                //Prepare the executing directory
                stressScript.WriteLine("mkdir tmpRun");
                stressScript.WriteLine("cd tmpRun");

                stressScript.WriteLine();

                stressScript.WriteLine("cp -f -v ../bin/Debug/netcoreapp2.0/* .");
                stressScript.WriteLine("cp -f -v $HELIX_CORRELATION_PAYLOAD/* .");
                stressScript.WriteLine("cp ../runstress.sh .");

                // Prepare the test execution line
                string testCommandLine = loadTestInfo.TestName + ".dll";

                stressScript.WriteLine("chmod +x runstress.sh");
                
                // If there is a host then prepend it to the test command line
                if (!String.IsNullOrEmpty(loadTestInfo.SuiteConfig.Host))
                {
                    stressScript.WriteLine($"chmod +x {loadTestInfo.SuiteConfig.Host}");
                    testCommandLine = loadTestInfo.SuiteConfig.Host + " " + testCommandLine;
                    // If the command line isn't a full path or ./ for current directory then add it to ensure we're using the host in the current directory
                    if ((!loadTestInfo.SuiteConfig.Host.StartsWith("/")) && (!loadTestInfo.SuiteConfig.Host.StartsWith("./")))
                    {
                        testCommandLine = "./" + testCommandLine;
                    }
                }

                testCommandLine = "./runstress.sh " + testCommandLine;

                stressScript.WriteLine("#test execution");

                stressScript.WriteLine("echo executing [{0}]", testCommandLine);
                stressScript.WriteLine(testCommandLine);
                // Save off the exit code
                stressScript.WriteLine("export _EXITCODE=$?");

                //Leave the tmp dir
                stressScript.WriteLine("cd ..");

                // exit the script with the return code
                stressScript.WriteLine("exit $_EXITCODE");
            }

            // Add the shell script to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo(STRESS_SCRIPT_NAME, SourceFileAction.Binplace));

            //add runstress.sh to the source directory as well
            var runstressPath = Path.Combine(loadTestInfo.SourceDirectory, "runstress.sh");

            var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("stress.codegenCore.resources.runstress.sh"));
            string unixContent = sr.ReadToEnd().Replace("\r\n", "\n");

            File.WriteAllText(runstressPath, unixContent);

            //add runstress.sh to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo("runstress.sh", SourceFileAction.Binplace));
        }
        
    }

    public class ExecutionFileGeneratorWindows : ISourceFileGenerator
    {
        private const string STRESS_SCRIPT_NAME = "stress.bat";
        public void GenerateSourceFile(LoadTestInfo loadTestInfo)// (string scriptName, string testName, Dictionary<string, string> envVars, string host = null)
        {
            string batchScriptPath = Path.Combine(loadTestInfo.SourceDirectory, STRESS_SCRIPT_NAME);

            using (TextWriter stressScript = new StreamWriter(batchScriptPath, false))
            {
                stressScript.WriteLine("@echo off");
                stressScript.WriteLine("REM stress script for " + loadTestInfo.TestName);
                stressScript.WriteLine();
                stressScript.WriteLine();

                //Setup dumpling
                stressScript.WriteLine("powershell wget https://dumpling.azurewebsites.net/api/client/dumpling.py -OutFile dumpling.py");
                stressScript.WriteLine("%HELIX_PYTHONPATH% dumpling.py install --update --installpath %USERPROFILE%\\.dumpling");
                stressScript.WriteLine("%HELIX_PYTHONPATH% %USERPROFILE%\\.dumpling\\dumpling.py install --full");

                // add environment variables STRESS_TESTID, STRESS_BUILDID
                loadTestInfo.EnvironmentVariables["STRESS_TESTID"] = loadTestInfo.TestName;
                loadTestInfo.EnvironmentVariables["STRESS_BUILDID"] = loadTestInfo.TestName.Split('_')[0];

                StringBuilder envVarPropertiesBuilder = new StringBuilder();
                foreach (var kvp in loadTestInfo.EnvironmentVariables)
                {
                    envVarPropertiesBuilder.Append(kvp.Key + "=" + kvp.Value + " ");
                }
                loadTestInfo.EnvironmentVariables["DUMPLING_PROPERTIES"] = "\"" + envVarPropertiesBuilder.ToString().TrimEnd(' ') + "\"";

                stressScript.WriteLine("REM environment section");
                // first take care of the environment variables
                foreach (KeyValuePair<string, string> kvp in loadTestInfo.EnvironmentVariables)
                {
                    stressScript.WriteLine("set {0}={1}", kvp.Key, kvp.Value);
                }
                stressScript.WriteLine();
                stressScript.WriteLine();

                //Setup the run directory
                stressScript.WriteLine("REM Build the run directory");
                stressScript.WriteLine("mkdir tmpRun");
                stressScript.WriteLine("pushd tmpRun");
                stressScript.WriteLine(@"xcopy ..\bin\Debug\netcoreapp2.0\* . /ey");
                stressScript.WriteLine(@"xcopy %HELIX_CORRELATION_PAYLOAD%\* . /ey");
                
                stressScript.WriteLine();
                stressScript.WriteLine();
                
                // Prepare the test execution line
                string testCommandLine = loadTestInfo.TestName + ".dll";

                // If there is a host then prepend it to the test command line
                if (!String.IsNullOrEmpty(loadTestInfo.SuiteConfig.Host))
                {
                    testCommandLine = loadTestInfo.SuiteConfig.Host + " " + testCommandLine;
                }
                stressScript.WriteLine("REM test execution");
                stressScript.WriteLine("echo calling [{0}]", testCommandLine);
                stressScript.WriteLine(testCommandLine);
                // Save off the exit code
                stressScript.WriteLine("set _EXITCODE=%ERRORLEVEL%");
                stressScript.WriteLine("echo test exited with ExitCode: %_EXITCODE%");
                stressScript.WriteLine();
                // Check the return code
                stressScript.WriteLine("if %_EXITCODE% EQU 0 goto :REPORT_PASS");
                stressScript.WriteLine("REM error processing");
                stressScript.WriteLine("powershell [string[]](Get-ChildItem C:\\cores\\*.dmp) > dumpPath.txt");
                stressScript.WriteLine("for /F \"tokens=*\" %%i IN ('type dumpPath.txt') do set _corefile=%%i");
                stressScript.WriteLine("echo 'executing %HELIX_PYTHONPATH% %USERPROFILE%\\.dumpling\\dumpling.py upload --dumppath %_corefile% --noprompt --triage full --displayname %STRESS_TESTID% --incpaths \"%cd%\" --properties %DUMPLING_PROPERTIES%'");
                stressScript.WriteLine("%HELIX_PYTHONPATH% %USERPROFILE%\\.dumpling\\dumpling.py upload --dumppath %_corefile% --noprompt --triage full --displayname %STRESS_TESTID% --incpaths \"%cd%\" --properties %DUMPLING_PROPERTIES%");
                stressScript.WriteLine("goto :REMOVETMPDIR");
                stressScript.WriteLine();
                stressScript.WriteLine();
                stressScript.WriteLine(":REPORT_PASS");
                stressScript.WriteLine("echo JRS - Test Passed. Report the pass.");
                stressScript.WriteLine();
                stressScript.WriteLine();

                //Remove dir
                stressScript.WriteLine(":REMOVETMPDIR");
                stressScript.WriteLine("popd");
                stressScript.WriteLine("rmdir tmpRun /S /Q");
                stressScript.WriteLine();

                
                //
                //                
                //                
                //                
                //                

                // exit the script with the exit code from the process
                stressScript.WriteLine(":END");
                stressScript.WriteLine("exit /b %_EXITCODE%");
            }

            // Add the batch script to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo(STRESS_SCRIPT_NAME, SourceFileAction.Binplace));
        }
    }
}