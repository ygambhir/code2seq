using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extractor
{
    class Program
    {
        static List<String> ExtractSingleFile(string filename, Options opts)
        {
            string data = File.ReadAllText(filename);
            string sep = "\t";
            string[] splitContent = data.Split(sep.ToCharArray());
            string code = splitContent[3];
            string label = splitContent[2];
            var extractor = new Extractor(code, opts);
            List<String> result = extractor.Extract();
            string sep1 = " ";
            string[] label_words = label.Split(sep1.ToCharArray());
            string code_summary = String.Join("|", label_words);
            for (int i = 0; i < result.Count(); i++){
                result[i] = (code_summary + " " + result[i]);
            }

            return result;
        }

        static void Main(string[] args)
        {
            Options options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opt => options = opt)
                .WithNotParsed(errors =>
                {
                    Console.WriteLine(errors);
                    return;
                });

            string path = options.Path;
            string[] files;
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            }
            else
            {
                files = new string[] { path };
            }

            IEnumerable<string> results = null;

            results = files.AsParallel().WithDegreeOfParallelism(options.Threads).SelectMany(filename => ExtractSingleFile(filename, options));

            using (StreamWriter sw = new StreamWriter(options.OFileName, append: true))
            {
                foreach (var res in results)
                {
                    sw.WriteLine(res);
                }
            }
        }
    }
}
