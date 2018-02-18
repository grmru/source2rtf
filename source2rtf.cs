using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace source2rtf
{
    class source2rtf
    {
        private static string[] _args;
        private static List<string> _source_files;
        private static string _output_rtf_file_full_path;
        private static string _input_directory;

        //Key options
        private static bool _recursively;
        private static List<string> _ext_list;
        private static Encoding _output_encoding;

        static int Main(string[] args)
        {
            #region Initialization
            _args = args;
            _source_files = new List<string>();
            _output_rtf_file_full_path = "";
            _input_directory = "";

            _recursively = false;
            _ext_list = new List<string>(); _ext_list.Add("*.cs");
            _output_encoding = Encoding.Default;
            #endregion

            //Searching for arguments
            for (int a = 0; a < args.Length; a++)
            {
                //Trying to catch an output file path
                try
                {
                    FileInfo fl = new FileInfo(args[a]);
                    if (fl.Extension.ToLower() == ".rtf")
                        _output_rtf_file_full_path = fl.FullName;
                }
                catch (Exception ex) { }

                //Trying to determinate input directory
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(args[a]);
                    if (dir.Exists)
                        _input_directory = dir.FullName;
                }
                catch (Exception ex) { }
                
                //Key options
                if (args[a].ToLower() == "/?")
                    Console.Write(GetUsageMessage());

                if (args[a].ToLower() == "/r")
                    _recursively = true;

                if (args[a].ToLower().StartsWith("/e:"))
                {
                    string[] splited1 = args[a].ToLower().Split(':');
                    if (splited1.Length == 2)
                    {
                        string[] exts = splited1[1].Split(',');
                        _ext_list = new List<string>(exts);
                    }
                }
            }

            _source_files = ScanInputDirectory(_input_directory, _recursively, _ext_list);

            if (_source_files.Count == 0)
            {
                Console.WriteLine("[Error] No files found!");
                return -10;
            }

            if (_output_rtf_file_full_path.Length == 0)
            {
                Console.WriteLine("[Error] Please determinate output RTF file path.");
                return -20;
            }

            string rtf_content = GenerateRTFContent(_source_files);

            FileInfo out_fl = new FileInfo(_output_rtf_file_full_path);
            if (!out_fl.Directory.Exists)
            {
                Console.WriteLine("[Error] Output directory doesn't exist!");
                return -30;
            }

            Console.Write(String.Format("Compiling {0} file(s) " + System.Environment.NewLine +
                                        "in {1} directory" + System.Environment.NewLine +
                                        "into {2} file", _source_files.Count, _input_directory, _output_rtf_file_full_path) + System.Environment.NewLine);
            Console.WriteLine("......");
            System.IO.StreamWriter sw = new StreamWriter(_output_rtf_file_full_path, false, _output_encoding);
            sw.Write(rtf_content);
            sw.Write('\0');
            sw.Close();
            Console.WriteLine("[Done]");

            //Temporary statistic print
            //printStatistic_args();
            //printStatistic_level1();

            return 0;
        }

        #region UsageMessage
        private static string GetUsageMessage()
        {
            string output = "";
            output = output + "+######################################################+" + System.Environment.NewLine;
            output = output + "# source2rtf is using to compile all your source files #" + System.Environment.NewLine;
            output = output + "# in one file in Rich Text Format                      #" + System.Environment.NewLine;
            output = output + "+######################################################+" + System.Environment.NewLine;
            output = output + "(c) George A. Tsyrkov, 2013" + System.Environment.NewLine;
            output = output + System.Environment.NewLine;
            output = output + "# USAGE: source2rtf [keys] [input_directory] [output_rtf_file_full_path]" + System.Environment.NewLine;
            output = output + "#  keys: " + System.Environment.NewLine;
            output = output + "#        /?  - show this message" + System.Environment.NewLine;
            output = output + "#        /r  - scan input directory recursively" + System.Environment.NewLine;
            output = output + "#        /e:[.ext1,.ext2,...] - list of extentions (.cs is default)" + System.Environment.NewLine;
            output = output + "#" + System.Environment.NewLine;
            output = output + @"# EXAMPLE: source2rtf /r /e:.cs,.cpp C:\work\src\prj1 C:\docs\src_prj1.rtf" + System.Environment.NewLine;
            output = output + System.Environment.NewLine;
            return output;
        } 
        #endregion

        private static List<string> ScanInputDirectory(string dir_path, bool recur, List<string> extentions)
        {
            List<string> ret_list = new List<string>();

            if (dir_path.Length == 0)
                return ret_list;

            DirectoryInfo dir = new DirectoryInfo(dir_path);
            if (!dir.Exists)
                return ret_list;

            System.IO.SearchOption s_opt = SearchOption.TopDirectoryOnly;
            if (recur)
                s_opt = SearchOption.AllDirectories;

            FileInfo[] files = dir.GetFiles("*", s_opt);

            for (int i = 0; i < files.Length; i++)
                if (extentions.Contains(files[i].Extension))
                    ret_list.Add(files[i].FullName);

            return ret_list;
        }

        private static string GenerateRTFContent(List<string> src_files)
        {
            string output = "";

            //RTF Header
            output =          @"{\rtf1\ansi\ansicpg1251\deff0\nouicompat\deflang1049" + System.Environment.NewLine;
            output = output + @"{\fonttbl{\f0\fnil\fcharset204 Times New Roman;}{\f1\fnil\fcharset204 Courier New;}}" + System.Environment.NewLine;
            output = output + @"{\*\generator source2rtf 1.0}" + System.Environment.NewLine;
            output = output + @"{\stylesheet" + System.Environment.NewLine;
            output = output + @"{\widctlpar\adjustright \fs20\cgrid \snext0 Normal;}" + System.Environment.NewLine;
            output = output + @"{\s29\widctlpar\tqc\tx4320\tqr\tx8640\qr\adjustright \fs20\cgrid \sbasedon0 \snext29 footer;}" + System.Environment.NewLine;
            output = output + @"}" + System.Environment.NewLine;
            output = output + @"{\footer \s29\widctlpar\tqc\tx4320\tqr\tx8640\qr\adjustright \fs20\cgrid {\chpgn}}" + System.Environment.NewLine;
            output = output + @"\viewkind4\uc1" + System.Environment.NewLine;
            output = output + System.Environment.NewLine;

            for (int i = 0; i < src_files.Count; i++)
            {            
                FileInfo in_file = new FileInfo(src_files[i]);
                output = output + @"\b\f0\fs28\lang1033" + System.Environment.NewLine;
                output = output + in_file.Name + @"\par" + System.Environment.NewLine;
                output = output + @"\par" + System.Environment.NewLine;

                output = output + @"\b0\f1\fs20" + System.Environment.NewLine;

                Encoding encoding = GetEncoding(src_files[i]);
                StreamReader sr = new StreamReader(src_files[i], encoding);                
                while (!sr.EndOfStream)
                {                    
                    byte[] b_line = encoding.GetBytes(sr.ReadLine());
                    byte[] b_converted_line = Encoding.Convert(encoding, _output_encoding, b_line);
                    string converted_line = _output_encoding.GetString(b_converted_line);
                    converted_line = converted_line.Replace("    ", "  "); //TODO: Try to Replace only first whitespaces
                    converted_line = converted_line.Replace(new string(new char[] { '\\' }), new string(new char[] { '\\', '\\' }));
                    converted_line = converted_line.Replace("{", new string(new char[] { '\\', '{' }));
                    converted_line = converted_line.Replace("}", new string(new char[] { '\\', '}' }));
                    output = output + converted_line + @"\par" + System.Environment.NewLine;
                }
                sr.Close();

                output = output + @"\page" + System.Environment.NewLine;
                output = output + System.Environment.NewLine;
            }

            output = output + System.Environment.NewLine;
            output = output + @"}";

            return output;
        }

        private static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open)) file.Read(bom, 0, 4);

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode;                    //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode;           //UTF-16BE
            if (bom[0] == 0    && bom[1] == 0    && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }

        #region Statistic outputs
        private static void printStatistic_args()
        {
            Console.WriteLine("---");
            for (int a = 0; a < _args.Length; a++)
                Console.WriteLine("args[" + a + "]=" + _args[a] + ";");

            Console.WriteLine(System.Environment.NewLine);
        }

        private static void printStatistic_level1()
        {
            Console.WriteLine("---");
            Console.WriteLine("output_rtf_file_full_path=" + _output_rtf_file_full_path + ";");
            Console.WriteLine("input_directory=" + _input_directory + ";");
            Console.WriteLine("recursively=" + (_recursively ? "1" : "0") + ";");

            string ext_list_inline = "";
            for (int i = 0; i < _ext_list.Count; i++)
                ext_list_inline = ext_list_inline + _ext_list[i] + ((i < _ext_list.Count - 1) ? "," : "");
            Console.WriteLine("ext_list=" + ext_list_inline + ";");

            for (int i = 0; i < _source_files.Count; i++)
                Console.WriteLine("source_files[" + i + "]=" + _source_files[i] + ";");

            Console.WriteLine(System.Environment.NewLine);
        }
        #endregion

    }
}
