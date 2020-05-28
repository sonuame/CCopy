using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CCopy
{
    class Program
    {

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

            var fileTypes = app.Option(
                "-t|--types <types>",
                "Source file types e.g *.jpg *.png", CommandOptionType.MultipleValue);

            app.OnExecute(() =>
            {
                if (!pathFrom.HasValue())
                {
                    Console.WriteLine("--source/-s is required");
                    return 0;
                }


                var from = pathFrom.Value();
                var to = pathTo.HasValue() ? pathTo.Value() : AppDomain.CurrentDomain.BaseDirectory;
                var replace = overWrite.HasValue();
                var types = fileTypes.HasValue() ? fileTypes.Values.ToArray() : new string[] { "*.*" };

                Copy(from, to, "", replace, types);
                return 1;
            });

            app.Execute(args);
        }

        static void Copy(string from, string to, string rootDir = "", bool overwrite = true, params string[] types)
        {
            var isdirFrom = from.GetDIRInfo().Attributes == FileAttributes.Directory;
            var isdirTo = to.GetDIRInfo().Attributes == FileAttributes.Directory || to.GetDIRInfo().Attributes == (FileAttributes)(-1);

            if (isdirFrom && isdirTo)
            {
                var options = new ProgressBarOptions
                {
                    ProgressCharacter = '─',
                    ProgressBarOnBottom = true
                };

                List<FileInfo> files = new List<FileInfo>();
                if (types?.Length > 0)
                    types.ToList().ForEach(type => files.AddRange(from.GetDIRInfo().GetFiles(type, SearchOption.AllDirectories)));
                else
                    files.AddRange(from.GetDIRInfo().GetFiles("*.*", SearchOption.AllDirectories));

                using (var pbar = new ProgressBar(files.Count, "progress bar is on the bottom now", options))
                {
                    var count = 0;
                    files.ForEach(file =>
                    {
                        Copy(file.FullName, to, from, overwrite);
                        ++count;
                        pbar.Tick($"File {count} of {files.Count} copied");
                    });
                }
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
    }
}
