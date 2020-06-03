using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CCopy
{
    class Program
    {
        static bool Silent;

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            var pathFrom = app.Option(
                "-s|--source <path>",
                "Defines the path of the file(s) to read from", CommandOptionType.SingleValue);

            var pathTo = app.Option(
                "-d|--dest <path>",
                "Defines the path of the files to write to", CommandOptionType.SingleValue);

            var overWrite = app.Option(
                "-r|--replace",
                "Set the Over-Write mode if destination file exists", CommandOptionType.NoValue);

            var subDir = app.Option(
                "-a|--all",
                "Select Sub directories too", CommandOptionType.NoValue);

            var fileTypes = app.Option(
                "-t|--types <types>",
                "Source file types e.g *.jpg *.png", CommandOptionType.MultipleValue);

            var silent = app.Option(
                "-q|--quiet",
                "No Progress display", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                Console.WriteLine();
                Console.WriteLine("CCopy version 1.0");

                if (!pathFrom.HasValue())
                {
                    Console.WriteLine("--source/-s is required");
                    return 0;
                }


                var from = Path.Combine(Directory.GetCurrentDirectory(), pathFrom.Value());
                var to = pathTo.HasValue() ? pathTo.Value() : Directory.GetCurrentDirectory();
                var replace = overWrite.HasValue();
                var resursive = subDir.HasValue();
                Silent = silent.HasValue();

                var types = fileTypes.HasValue() ?
                    (fileTypes.Value().Contains(",") ? fileTypes.Value().Split(",") :
                    fileTypes.Value().Split(" ")) : new string[] { "*.*" };


                Copy(
                    from: from,
                    to: to,
                    rootDir: "",
                    recursive: resursive,
                    overwrite: replace,
                    types: types
                );
                return 1;
            });

            app.Execute(args);

        }

        static void Copy(string from, string to, string rootDir = "", bool recursive = false, bool overwrite = true, params string[] types)
        {
            var isdirFrom = from.IsDir();
            var isdirTo = to.IsDir();

            if (isdirFrom && isdirTo)
            {
                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true
                };

                List<FileInfo> files = new List<FileInfo>();
                if (types?.Length > 0)
                    types.ToList().ForEach(type => files.AddRange(from.GetDIRInfo().GetFiles(type, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));
                else
                    files.AddRange(from.GetDIRInfo().GetFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                Console.WriteLine($"{files.Count} total files");

                var count = 0;
                Parallel.ForEach(files, file =>
                {
                    Copy(file.FullName, to, from, overwrite);
                    if (!Silent)
                    {
                        ++count;
                        Console.Write($"..{count}");
                    }
                });
            }

            if (!isdirFrom && isdirTo)
            {
                if (File.Exists(from))
                {
                    var destpath = Path.Combine(to, from.Replace(rootDir + "\\", ""));
                    Directory.CreateDirectory(destpath.GetFileInfo().DirectoryName);
                    File.Copy(from, destpath, overwrite);
                }
            }

            if (!isdirFrom && !isdirTo)
            {
                if (File.Exists(from))
                {
                    Directory.CreateDirectory(to.GetFileInfo().DirectoryName);
                    File.Copy(from, to, overwrite);
                    Console.WriteLine("1 File(s) copied");
                }
            }
        }
    }


    static class extentions
    {
        public static DirectoryInfo GetDIRInfo(this string @this)
        {
            return new DirectoryInfo(@this);
        }

        public static FileInfo GetFileInfo(this string @this)
        {
            return new FileInfo(@this);
        }

        public static bool IsDir(this string @this)
        {
            return Directory.Exists(@this) || !File.Exists(@this);
        }
    }
}
