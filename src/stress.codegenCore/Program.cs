using System;
using stress.codegen;

public class Hukares
{
    public static void Main(string [] args)
    {
        if(args[8].ToLower() == "true")
        {
            Console.ReadLine();
        }
        LoadSuiteGenerator suiteGen = new LoadSuiteGenerator();
        suiteGen.GenerateSuite(new Random().Next(), args[0], args[1], new string [] {args[2]}
            , new string [] {args[3]}, new string [] {args[4]},
            LoadSuiteConfig.Deserialize("args[5]"),
            args[6], Boolean.Parse(args[7]), null);
    /*suiteGen.GenerateSuite(new Random().Next(), "TestRun", "C:\\git\\dotnet-reliability\\test\\generated/TestRun/", new string [] {"C:\\git\\dotnet-reliability\\bin/obj/utextracted/"}
	, new string [] {"*.Tests.dll"}, new string [] {"C:\\git\\dotnet-reliability\\bin/obj/utdepends/"},
	LoadSuiteConfig.Deserialize("C:\\git\\dotnet-reliability\\test\\suiteconfig\\smoketest.suite.json"),
	"C:\\git\\dotnet-reliability\\bin/obj/testdiscoverycache.json", false, null);*/
    }
}