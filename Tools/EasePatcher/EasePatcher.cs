﻿//MIT, 2017, EngineKit
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
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
            string[] libEsprCoreFiles = Directory.GetFiles(_espresso_src + @"\libespresso");
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
                ReplaceFile(patchSrcFile, targetDir + "/" + Path.GetFileName(patchSrcFile));
            }
            //sub-folders
            string[] subFolders = Directory.GetDirectories(patchSrcDir);
            j = subFolders.Length;
            for (int i = 0; i < j; ++i)
            {
                ReplaceFileInDirectory(subFolders[i], targetDir + "/" + Path.GetFileName(subFolders[i]));
            }
        }
        static void ReplaceFile(string patchFileName, string targetToBeReplaceFileName)
        {
            if (!File.Exists(targetToBeReplaceFileName))
            {
                //not found -> stop
                throw new NotSupportedException();
            }
            //----------------------------
            if (Path.GetFileName(targetToBeReplaceFileName) != Path.GetFileName(patchFileName))
            {
                //filename must match
                throw new NotSupportedException();
            }
            //----------------------------
            //replace src to dest
            File.Copy(patchFileName, targetToBeReplaceFileName, true);
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
            this._original_node_src_dir = original_node_src_dir;
            this._espresso_src = espresso_src;
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
            string vc_build_script = _original_node_src_dir + @"\vcbuild.bat";
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
                if (line == @"<ModuleDefinitionFile>$(OutDir)obj\global_intermediate\openssl.def</ModuleDefinitionFile>")
                {
                    //remove this line
                    allLines.RemoveAt(i);
                }
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

                        int impl_count = this._newCppImplFiles.Count;
                        for (int m = 0; m < impl_count; ++m)
                        {
                            allLines.Insert(i + 2 + m, "<ClCompile Include=\"" + _newCppImplFiles[m] + "\" />");
                        }

                    }
                    else if (!add_header_files && line.StartsWith("<ClInclude"))
                    {
                        add_header_files = true;
                        int header_count = this._newHeaderFiles.Count;
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
        public void Setup(string original_node_src, string espresso_src, string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            this._original_node_src_dir = original_node_src;
            this._espresso_src = espresso_src;
        }
        public void Build(SimpleAction nextAction)
        {
            //1. configure
            //2. make
            _buildState = BuildState.InitBuild;
            ThreadPool.QueueUserWorkItem(delegate
            {
                UnixConfigure();
                UnixMake();
                if (nextAction != null)
                {
                    nextAction();
                }
            });

        }
        void UnixConfigure()
        {
            ProcessStartInfo stInfo = new ProcessStartInfo(_espresso_src + "/configure");
            stInfo.WorkingDirectory = _original_node_src_dir;
            //
            Process proc = Process.Start(stInfo);
            proc.WaitForExit();
        }
        void UnixMake()
        {
            ProcessStartInfo stInfo = new ProcessStartInfo("make");
            stInfo.WorkingDirectory = _original_node_src_dir;
            //
            Process proc = Process.Start(stInfo);
            proc.WaitForExit();
        }
    }

}