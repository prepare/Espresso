//MIT, 2017-present,WinterDev, EngineKit
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace EasePatcher
{
    delegate void SimpleAction();
    enum BuildState
    {
        Zero,
        InitBuild,
    }

    enum PatcherOS
    {
        Unknown,
        Windows,
        Mac,
        Linux
    }

    abstract class PatcherBase
    {
        protected string _original_node_src_dir;
        protected string _espresso_src;
        protected BuildState _buildState;
        protected List<string> _newHeaderFiles = new List<string>();
        protected List<string> _newCppImplFiles = new List<string>();

        /// <summary>
        /// path to patch folder relative to main espresso src folder (eg. node_patches\node7.10_modified)
        /// </summary>
        public string PatchSubFolder { get; set; }

        protected virtual void OnPatchFinishing()
        {

        }

        public void DoPatch()
        {
            //1. copy file from espresso's patch folder 
            //and place into target dir 
            //the version must be match between original source and the patch

            //string patch_folder = _espresso_src + "\"+ @"node_patches\node7.10_modified";
            string patch_folder = _espresso_src + "/" + PatchSubFolder;

            ReplaceFileInDirectory(patch_folder, _original_node_src_dir);
            //2. copy core of libespresso bridge code( nodejs and .NET)
            //to src/espresso_ext folder

            string libespr_dir = "src/libespresso";
            string targetDir = _original_node_src_dir + "/" + libespr_dir;
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            //create targetdir
            Directory.CreateDirectory(targetDir);
            //copy the following file to target folder
            string[] libEsprCoreFiles = Directory.GetFiles(_espresso_src + @"/libespresso");
            int j = libEsprCoreFiles.Length;
            _newHeaderFiles.Clear();
            _newCppImplFiles.Clear();

            for (int i = 0; i < j; ++i)
            {
                string esprCodeFilename = libEsprCoreFiles[i];
                switch (Path.GetExtension(esprCodeFilename).ToLower())
                {
                    default:
                        continue;
                    case ".cpp":
                        {
                            string onlyfilename = Path.GetFileName(esprCodeFilename);
                            _newCppImplFiles.Add(libespr_dir + "/" + onlyfilename);
                            File.Copy(esprCodeFilename, targetDir + "/" + onlyfilename);
                        }
                        break;
                    case ".h":
                        {
                            string onlyfilename = Path.GetFileName(esprCodeFilename);
                            _newHeaderFiles.Add(libespr_dir + "/" + onlyfilename);
                            File.Copy(esprCodeFilename, targetDir + "/" + onlyfilename);
                        }
                        break;
                }
            }
            //
            OnPatchFinishing();
        }
        static void ReplaceFileInDirectory(string patchSrcDir, string targetDir)
        {
            //recursive
            string[] allFiles = Directory.GetFiles(patchSrcDir);
            //copy these files to target folder
            int j = allFiles.Length;
            for (int i = 0; i < j; ++i)
            {
                string patchSrcFile = allFiles[i];
                string extension = Path.GetExtension(patchSrcFile);
                switch (extension)
                {
                    default:
                        break;
                    case ".js":
                    case ".h":
                    case ".cpp":
                    case ".cc":
                        ReplaceFile(patchSrcFile, targetDir + "/" + Path.GetFileName(patchSrcFile));
                        break;
                }
            }
            //sub-folders
            string[] subFolders = Directory.GetDirectories(patchSrcDir);
            j = subFolders.Length;
            for (int i = 0; i < j; ++i)
            {
                ReplaceFileInDirectory(subFolders[i], targetDir + "/" + Path.GetFileName(subFolders[i]));
            }
        }
        static void ReplaceFile(string patchFileName, string targetToBeReplacedFileName)
        {
            if (!File.Exists(targetToBeReplacedFileName))
            {
                //not found -> stop
                Console.WriteLine("NOT FOUND targetToBeReplacedFileName: " + targetToBeReplacedFileName);

                throw new NotSupportedException();
            }
            //----------------------------
            if (Path.GetFileName(targetToBeReplacedFileName) != Path.GetFileName(patchFileName))
            {
                //filename must match
                throw new NotSupportedException();
            }
            //----------------------------
            //replace src to dest
            File.Copy(patchFileName, targetToBeReplacedFileName, true);
        }

        public static PatcherOS GetPatcherOS()
        {
            //check platform
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX))
            {
                return PatcherOS.Mac;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux)
                )
            {
                return PatcherOS.Linux;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
              System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return PatcherOS.Windows;
            }
            else
            {
                return PatcherOS.Unknown;
            }
        }
    }

    class WindowsPatcher : PatcherBase
    {
        string _initBuildParameters;

        public void Setup(string original_node_src_dir,
            string espresso_src,
            string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            _initBuildParameters = initBuildParameters;
            _original_node_src_dir = original_node_src_dir;
            _espresso_src = espresso_src;
        }
        public bool ConfigHasSomeErrs
        {
            get;
            private set;
        }
        public void Configure(SimpleAction nextAction)
        {
            //-------------------------------------------------------
            //FOR WINDOWS:....
            //-------------------------------------------------------
            //1. specific location of original node src
            //2. run first FRESH build of the node src 


            string vc_build_script = Path.Combine(
               Directory.GetCurrentDirectory(), _original_node_src_dir + @"/vcbuild.bat").ToString();

            if (!File.Exists(vc_build_script))
            {
                Console.WriteLine("Err! not found batch files");
                ConfigHasSomeErrs = true;
                if (nextAction != null)
                {
                    nextAction();
                }
                return;
            }

            _buildState = BuildState.InitBuild;

            ThreadPool.QueueUserWorkItem(delegate
            {
                //choose x86 or x64;
                //choose release or debug
                ProcessStartInfo procStartInfo = new ProcessStartInfo(
                    vc_build_script,
                    _initBuildParameters);
                //procStartInfo.UseShellExecute = false;
                //procStartInfo.RedirectStandardOutput = true;
                //procStartInfo.CreateNoWindow = true;
                procStartInfo.WorkingDirectory = _original_node_src_dir;

                Process proc = System.Diagnostics.Process.Start(procStartInfo);
                proc.WaitForExit();
                //finish
                if (nextAction != null)
                {
                    nextAction();
                }
            });
        }

        protected override void OnPatchFinishing()
        {
            base.OnPatchFinishing();
            //-------
            //modify vcx project
            //insert
            string vcx = _original_node_src_dir + "/node.vcxproj";
            List<string> allLines = new List<string>();
            //-------
            //create backup
            if (!File.Exists(vcx + ".backup"))
            {
                File.Copy(vcx, vcx + ".backup");
            }
            //
            using (FileStream fs = new FileStream(vcx, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    allLines.Add(line.Trim());//trim here
                    line = reader.ReadLine();
                }
            }
            //-------
            //find proper insert location
            int j = allLines.Count;

            //remove some config line
            for (int i = j - 1; i >= 0; --i)
            {
                string line = allLines[i];
                //1.  remove 
                if (line == @"<ModuleDefinitionFile>$(OutDir)obj\global_intermediate\openssl.def</ModuleDefinitionFile>")
                {
                    //remove this line
                    allLines.RemoveAt(i);
                }
                else if (line == "<ClCompile Include=\"src\\node_main.cc\">")
                {
                    //check next line 
                    string nextline = allLines[i + 1];
                    if (nextline == "<ExcludedFromBuild>true</ExcludedFromBuild>")
                    {
                        //remove next line
                        allLines.RemoveAt(i + 1);
                    }
                }
                //2. if we config as dll , 
                //it exclude main.cc,
                //but in our case we need it
                //
            }
            //-------
            bool add_header_files = false;
            bool add_impl_files = false;
            j = allLines.Count;//reset
            for (int i = 0; i < j; ++i)
            {
                string line = allLines[i];
                if (line == "<ItemGroup>")
                {
                    //next line
                    line = allLines[i + 1];
                    if (!add_impl_files && line.StartsWith("<ClCompile"))
                    {
                        //found insertion point 
                        add_impl_files = true;

                        int impl_count = _newCppImplFiles.Count;
                        for (int m = 0; m < impl_count; ++m)
                        {
                            allLines.Insert(i + 2 + m, "<ClCompile Include=\"" + _newCppImplFiles[m] + "\" />");
                        }

                    }
                    else if (!add_header_files && line.StartsWith("<ClInclude"))
                    {
                        add_header_files = true;
                        int header_count = _newHeaderFiles.Count;
                        for (int m = 0; m < header_count; ++m)
                        {
                            allLines.Insert(i + 2 + m, "<ClInclude Include=\"" + _newHeaderFiles[m] + "\" />");
                        }
                    }

                }
                //----
                if (add_impl_files && add_header_files) { break; }
            }

            //save back
            using (FileStream fs = new FileStream(vcx, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                j = allLines.Count;//reset
                for (int i = 0; i < j; ++i)
                {
                    writer.WriteLine(allLines[i]);
                }
                writer.Flush();
            }
        }
    }

    class LinuxAndMacPatcher : PatcherBase
    {
        string _init_build_pars = "";
        public void Setup(string original_node_src, string espresso_src, string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            _init_build_pars = initBuildParameters;
            _original_node_src_dir = original_node_src;
            _espresso_src = espresso_src;
        }
        public void PatchGyp(SimpleAction nextAction)
        {

            _buildState = BuildState.InitBuild;
            ThreadPool.QueueUserWorkItem(delegate
            {
                InternalPatchGyp();
                //we patch the gyp *** 
                UnixConfigure();
                //UnixMake();
                if (nextAction != null)
                {
                    nextAction();
                }
            });
        }
        void InternalPatchGyp()
        {
            string src_dir = _original_node_src_dir;
            List<string> lines = new List<string>();
            using (FileStream fs = new FileStream(src_dir + "/" + "node.gyp", FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }
            //-------------------
            //find specific location and insert src
            string[] new_patches = new string[]
            {
                "src/libespresso/bridge.cpp",
                "src/libespresso/bridge2_impl.cpp",
                "src/libespresso/jscontext.cpp",
                "src/libespresso/jsengine.cpp",
                "src/libespresso/jsscript.cpp",
                "src/libespresso/managedref.cpp",
                "src/libespresso/mini_BinaryReaderWriter.cpp",
                "src/libespresso/libespr_nodemain.cpp",
                "src/libespresso/bridge2.h",
                "src/libespresso/espresso.h",
                "src/libespresso/jsscript.h",
            };

            int j = lines.Count;

            for (int i = j - 1; i >= 0; --i)
            {
                string line = lines[i].Trim();
                if (line == "'src/node.cc',")
                {
                    //insert 
                    //and break
                    foreach (string patchFileName in new_patches)
                    {
                        lines.Insert(i, "'" + patchFileName + "',");
                    }
                    break;
                }
            }
            //----------      
            //save gyp 
            j = lines.Count;
            using (FileStream fs = new FileStream(src_dir + "/" + "node.gyp", FileMode.Create))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                for (int i = 0; i < j; ++i)
                {
                    writer.WriteLine(lines[i]);
                }
                writer.Flush();
            }
        }


        void UnixConfigure()
        {
            ProcessStartInfo stInfo = new ProcessStartInfo(_original_node_src_dir + "/configure", _init_build_pars);
            stInfo.WorkingDirectory = _original_node_src_dir;
            //
            Process proc = Process.Start(stInfo);
            proc.WaitForExit();
        }
        void UnixMake()
        {
            ProcessStartInfo stInfo = new ProcessStartInfo("make");
            stInfo.WorkingDirectory = _original_node_src_dir;
            Process proc = Process.Start(stInfo);
            proc.WaitForExit();
        }
    }

}