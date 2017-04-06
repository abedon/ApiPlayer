using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using AutoTest.Common;

namespace AutoTestClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestManager.Initialize();

            //DisplayHeader();

            bool interactive = false;
            bool exit = false;

            if (args.Length == 0)
            {
                Console.WriteLine(" No command line arguments specified, exiting program.");

                return;
            }
            else if (args.Length == 1 && args[0].ToLower().Trim() == "-interactive")
            {
                interactive = true;
            }
            else
            {
                exit = true;
            }

            if (interactive || args.Length != 0)
            {
                do
                {
                    try
                    {
                        string[] input;

                        if (interactive)
                        {
                            Console.Write("\n=> : ");

                            input = Console.ReadLine().ToLower().Trim().Split(' ');
                        }
                        else
                        {
                            input = args;
                        }

                        if (input.Length > 0)
                        {
                            string command = input[0].ToLower().Trim();

                            if (command[0] == '-')
                            {
                                command = command.Substring(1);
                            }

                            switch (command)
                            {
                                case "exit":
                                    if (input.Length == 1)
                                    {
                                        exit = true;
                                    }
                                    else
                                    {
                                        throw new System.Exception(" Improper use of the \"exit\" command, type \"exit\"");
                                    }

                                    break;
                                case "help":
                                    if (input.Length == 1)
                                    {
                                        Console.WriteLine(" You have been helped");
                                    }
                                    else if (input.Length == 2)
                                    {
                                        Console.WriteLine(" You have been helped /commanded");
                                    }
                                    else
                                    {
                                        throw new System.Exception(" Improper use of the \"help\" command, type \"help\" or \"help /command\"");
                                    }

                                    break;
                                case "list":
                                    if (input.Length != 2)
                                    {
                                        throw new System.Exception();
                                    }

                                    string type = input[1];

                                    switch (type)
                                    {
                                        case "all":
                                            TestManager.ListAll();

                                            break;
                                        case "testcase":
                                            TestManager.ListTestCases();

                                            break;
                                        case "testsuite":
                                            TestManager.ListTestSuites();

                                            break;
                                        default:
                                            throw new System.Exception(" Improper use of the \"list\" command, type \"help /list\" to see proper usage");
                                    }

                                    break;
                                case "clean":
                                    if (input.Length > 3 || input.Length == 1)
                                    {
                                        throw new System.Exception(" Improper use of the \"clean\" command, type \"help /clean\" to see proper usage");
                                    }

                                    switch (input[1])
                                    {
                                        case "all":
                                            if (input.Length > 2)
                                            {
                                                throw new System.Exception(" Improper use of the \"clean\" command, type \"help /clean\" to see proper usage");
                                            }

                                            TestManager.CleanAll();

                                            break;
                                        case "testsuite":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                if (parameters.Length == 1 && parameters[0] == "all")
                                                {
                                                    TestManager.CleanAllTestSuites();
                                                }
                                                else
                                                {
                                                    int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                    TestManager.CleanSelectTestSuites(testIDs);
                                                }
                                            }
                                            catch
                                            {
                                                throw new System.Exception(" Improper use of the \"run\" command, type \"help /run\" to see proper usage");
                                            }

                                            break;
                                        case "testcase":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                if (parameters.Length == 1 && parameters[0] == "all")
                                                {
                                                    TestManager.CleanAllTestCases();
                                                }
                                                else
                                                {
                                                    int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                    TestManager.CleanSelectTestCases(testIDs);
                                                }
                                            }
                                            catch
                                            {
                                                throw new System.Exception(" Improper use of the \"run\" command, type \"help /run\" to see proper usage");
                                            }

                                            break;
                                        default:
                                            throw new System.Exception(" Improper use of the \"" + command + "\" command, type \"help /" + command + "\" to see proper usage");
                                    }

                                    break;
                                case "run":
                                    if (input.Length > 3 || input.Length == 1)
                                    {
                                        throw new System.Exception(" Improper use of the \"run\" command, type \"help /run\" to see proper usage");
                                    }

                                    switch (input[1])
                                    {
                                        case "all":
                                            if (input.Length > 2)
                                            {
                                                throw new System.Exception();
                                            }

                                            TestManager.RunAll();

                                            break;
                                        case "testsuite":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                if (parameters.Length == 1 && parameters[0] == "all")
                                                {
                                                        TestManager.RunAllTestSuites();
                                                }
                                                else
                                                {
                                                    int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                        TestManager.RunSelectTestSuites(testIDs);
                                                }
                                            }
                                            catch
                                            {
                                                throw new System.Exception(" Improper use of the \"run\" command, type \"help /run\" to see proper usage");
                                            }

                                            break;
                                        case "testcase":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                if (parameters.Length == 1 && parameters[0] == "all")
                                                {
                                                        TestManager.RunAllTestCases();
                                                }
                                                else
                                                {
                                                    int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                        TestManager.RunSelectTestCases(testIDs);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new System.Exception(" Improper use of the \"run\" command, type \"help /run\" to see proper usage");
                                            }

                                            break;
                                        default:
                                            throw new System.Exception(" Improper use of the \"" + command + "\" command, type \"help /" + command + "\" to see proper usage");
                                    }

                                    break;

                                //TMS calls this autotest client to do functional testing
                                //We don't upload result files to test management server in this case
                                case "tmsrun":
                                    if (input.Length > 3 || input.Length == 1)
                                    {
                                        throw new System.Exception(" Improper use of the \"tmsrun\" command, type \"help /tmsrun\" to see proper usage");
                                    }

                                    switch (input[1])
                                    {
                                        case "testsuite":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                TestManager.RunSelectTestSuitesForTMS(testIDs);
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new System.Exception(" Improper use of the \"testsuite\" command, type \"help /testsuite\" to see proper usage. Error: " + ex.Message);
                                            }

                                            break;
                                        case "testcase":
                                            try
                                            {
                                                string[] parameters = input[2].Split(',').Distinct().ToArray();

                                                if (parameters.Length == 1 && parameters[0] == "all")
                                                {
                                                    TestManager.RunAllTestCases();
                                                }
                                                else
                                                {
                                                    int[] testIDs = Utils.StringArrayToIntegerArray(parameters);

                                                    TestManager.RunSelectTestCasesForTMS(testIDs);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new System.Exception(" Improper use of the \"testcase\" command, type \"help /testcase\" to see proper usage. Error: " + ex.Message + "\r\n" + ex.StackTrace);
                                            }

                                            break;
                                        default:
                                            throw new System.Exception(" Improper use of the \"" + command + "\" command, type \"help /" + command + "\" to see proper usage");
                                    }

                                    break;
                                default:
                                    throw new System.Exception(" Invalid command, \"" + command + "\", type \"help\" for list of valid commands");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                } while (!exit);
            }
        }

        private static void DisplayHeader()
        {
            AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

            string name = assemblyName.Name.ToString();
            string version = assemblyName.Version.ToString();

            Console.WriteLine(name + " [Version " + version + "]");
        }
    }
}
