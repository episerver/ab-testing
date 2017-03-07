using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MergeTranslations
{
    class TMU
    {
        static void usage()
        {
            Console.WriteLine("Translations Merging Utility ( TMU.EXE )");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("");
            Console.WriteLine("TMU [,-h]                                    : shows this message");
            Console.WriteLine("Most common");
            Console.WriteLine("TMU [-all][srcFile][srcFolder][diff_folder]  : Save as -a && -r giving a src file and a src folder" );
            Console.WriteLine("TMU [-mall][diff_folder][srcFolder]          : Merges all language files with available translations");
            Console.WriteLine("");
            Console.WriteLine("Advanced single file manipulation (good for debugging tool)");
            Console.WriteLine("TMU [-n][srcFile]                        : Normalizes the file (tolower all keys and alphabeticalize all elements)");
            Console.WriteLine("TMU [-a][srcFile][destFile][diff_folder] : Normalizes and adds new keys from srcFile to the destFile");
            Console.WriteLine("TMU [-r][srcFile][destFile]              : Normalizes and removes keys from the destFile that are not in the srcFile");
            Console.WriteLine("TMU [-ar][srcFile][destFile][diff_folder]: Same as -a && -r");
            Console.WriteLine("TMU [-m][srcFile][destFile]              : Normalizes and merges srcFile key values to destFile");
            Console.WriteLine("");
            Console.WriteLine("TMU [-clean][srcFile][langFolder][diff_folder] : Emptys our diff files and removes any non localized keys from the localized files.");
            Console.WriteLine("Notes:");
            Console.WriteLine("");

            Console.WriteLine("Hit any key...");
            Console.ReadLine();
        }

        /// <summary>
        /// See usage -h 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            TranslationProcessor p = new TranslationProcessor();
            if ( args.Length != 0 )
            {
                if( args[0] == "-h" ) // usage / help
                {
                    TMU.usage();
                }
                else if (args[0] == "-all")
                {
                    TMU.ProcessAllFiles(args);
                }
                else if (args[0] == "-mall")
                {
                    TMU.MergeAllFiles(args);
                }
                else if (args[0] == "-clean")
                {
                    TMU.CleanAllFiles(args);
                }
                else if (args[0] == "-n") // normalize only
                {
                    Console.WriteLine("Adding And Removing keys  :");
                }
                else if (args[0] == "-a") // add only
                {
                    Console.WriteLine("Adding keys  :");
                    if (args.Length == 4)
                    {
                        Console.WriteLine(" Src     : " + args[1]);
                        Console.WriteLine(" Dest    : " + args[2]);
                        Console.WriteLine(" Diffs   : " + args[3]);
                        p.AddKeys(args[1], args[2], args[3]);
                    }
                    else if(args.Length == 3)
                    {
                        Console.WriteLine(" Src     : " + args[1]);
                        Console.WriteLine(" Dest    : " + args[2]);
                        Console.WriteLine(" Warning: No diff folder specified. Changes will not be stored in a diff folder.");
                        p.AddKeys(args[1], args[2]);
                    }
                }
                else if (args[0] == "-r")
                {
                    Console.WriteLine("Removing keys  :");
                    Console.WriteLine(" Src     : " + args[1]);
                    Console.WriteLine(" Dest    : " + args[2]);
                    p.RemoveKeys(args[1], args[2]);
                }
                else if (args[0] == "-ar") // add and remove
                {
                    Console.WriteLine("Adding And Removing keys  :");
                    if (args.Length == 4)
                    {
                        Console.WriteLine(" Src     : " + args[1]);
                        Console.WriteLine(" Dest    : " + args[2]);
                        Console.WriteLine(" Diffs   : " + args[3]);
                        p.AddKeys(args[1], args[2], args[3]);
                    }
                    else
                    {
                        Console.WriteLine(" Src     : " + args[1]);
                        Console.WriteLine(" Dest    : " + args[2]);
                        Console.WriteLine(" Warning: No diff folder specified. Changes will not be stored in a diff folder.");
                        p.AddKeys(args[1], args[2]);
                    }

                    p.RemoveKeys(args[1], args[2]);
                }
                else if (args[0] == "-m") // merge
                {
                    Console.WriteLine("Merging :");
                    Console.WriteLine(" Src     : " + args[1]);
                    Console.WriteLine(" Dest    : " + args[2]);

                    p.MergeKeys(args[1], args[2]);
                }
                else // show help
                {
                    Console.WriteLine("");
                    Console.WriteLine("Unknown Arguments!");
                    TMU.usage();
                }
            }
            else
            {
                TMU.usage();
            }
        }

        static void ProcessAllFiles(string[] args)
        {
            // Reuse the same logic in main, just do it with a file list. 
            Console.WriteLine("Adding / Removing / Creating diffs for  :");
            if (args.Length == 4)
            {
                Console.WriteLine(" Src         : " + args[1]);
                Console.WriteLine(" Src Folder  : " + args[2]);
                Console.WriteLine(" Diffs Folder: " + args[3]);
            }

            List<string> someargs = new List<string>();
            var Files = Directory.GetFiles(args[2]);
            foreach( var file in Files )
            {
                someargs.Clear();
                someargs.Add("-ar");
                someargs.Add(args[1]);
                someargs.Add(file);
                someargs.Add(args[3]);

                TMU.Main(someargs.ToArray());
            }
        }

        static void CleanAllFiles(string[] args)
        {
            Console.WriteLine("Adding / Removing / Creating diffs for  :");
            if (args.Length == 4)
            {
                Console.WriteLine(" Src         : " + args[1]);
                Console.WriteLine(" LangFolder  : " + args[2]);
                Console.WriteLine(" DiffFolder  : " + args[3]);
            }

            // for every file in the source folder, remove any keys that have the same values
            // in the src language file.
            XmlFileManager source = new XmlFileManager(args[1]);
            var LanguageFiles = Directory.GetFiles(args[2], "*.xml");
            List<MyElement> ListToRemove = new List<MyElement>();
            foreach (var LanguageFile in LanguageFiles)
            {
                if (args[1].ToLower() != LanguageFile.ToLower())
                {
                    ListToRemove.Clear();
                    XmlFileManager f = new XmlFileManager(LanguageFile);
                    foreach (var element in f.ElementList)
                    {
                        if (!element.e.HasElements)
                        {
                            var sourceElement = source.ElementList.FirstOrDefault(l => l.p == element.p);
                            if (sourceElement != null && sourceElement.e.Value == element.e.Value)
                            {
                                ListToRemove.Add(element);
                            }
                        }
                    }
                    foreach (var element in ListToRemove)
                    {
                        f.Remove(element);
                    }
                    f.Save();
                }
            }

            // Clean all the diff files.
            var DiffFiles = Directory.GetFiles(args[3], "*.DIFF");
            foreach (var DiffFile in DiffFiles)
            {
                XmlFileManager f = new XmlFileManager(DiffFile);
                f.Clean();
                f.Save();
            }
        }

        static void MergeAllFiles(string[] args)
        {
            // Reuse the same logic in main, just do it with a file list. 
            Console.WriteLine("Merging All");
            Console.WriteLine(" Src Folder  : " + args[1]);
            Console.WriteLine(" Diffs Folder: " + args[2]);

            List<string> someargs = new List<string>();
            var SourceFiles = Directory.GetFiles(args[1], "*.xml");
            var DiffFiles = Directory.GetFiles(args[2], "*.DIFF");
            foreach(var DiffFile in DiffFiles)
            {
                var f = Path.GetFileName(DiffFile).Split('.')[0]; // get the file name without the path or extension
                var SourceFile = SourceFiles.FirstOrDefault(l => l.Contains(f));
                if(SourceFile != null)
                {
                    someargs.Clear();
                    someargs.Add("-m");
                    someargs.Add(DiffFile);
                    someargs.Add(SourceFile);

                    TMU.Main(someargs.ToArray());
                }
            }
        }
    }
}
