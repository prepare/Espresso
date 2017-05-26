//MIT, 2017, EngineKit
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace EasePatcher
{
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

        public event EventHandler FinishInitBuild;

        protected BuildState _buildState;
        protected void InvokeFinishInitBuild()
        {
            if (FinishInitBuild != null)
            {
                FinishInitBuild(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// path to patch folder relative to main espresso src folder (eg. node_patches\node7.10_modified)
        /// </summary>
        public string PatchSubFolder { get; set; }


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

            string targetDir = _original_node_src_dir + "/src/libespresso";
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            //create targetdir
            Directory.CreateDirectory(targetDir);
            //copy the following file to target folder
            string[] libEsprCoreFiles = Directory.GetFiles(_espresso_src + @"\libespresso");
            int j = libEsprCoreFiles.Length;
            for (int i = 0; i < j; ++i)
            {
                string esprCodeFilename = libEsprCoreFiles[i];
                switch (Path.GetExtension(esprCodeFilename).ToLower())
                {
                    default:
                        continue;
                    case ".cpp":
                    case ".h":
                        File.Copy(esprCodeFilename, targetDir + "/" + Path.GetFileName(esprCodeFilename));
                        break;
                }
            }
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
        public void Setup(string original_node_src,
            string espresso_src,
            string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            _initBuildParameters = initBuildParameters;
            this._original_node_src_dir = original_node_src;
            this._espresso_src = espresso_src;
        }

        public void Build()
        {
            //-------------------------------------------------------
            //FOR WINDOWS:....
            //-------------------------------------------------------
            //1. specific location of original node src
            //2. run first FRESH build of the node src 
            string vc_build_script = _original_node_src_dir + @"\vcbuild.bat";
            if (!File.Exists(vc_build_script))
            {
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
                InvokeFinishInitBuild();

            });
        }
    }
    class LinuxAndMacPatcher : PatcherBase
    {
        public void Setup(string original_node_src, string espresso_src, string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            this._original_node_src_dir = original_node_src;
            this._espresso_src = espresso_src;

            Build();
        }

        public void Build()
        {
            //1. configure
            //2. make
            _buildState = BuildState.InitBuild;
            ThreadPool.QueueUserWorkItem(delegate
            {
                UnixConfigure();
                UnixMake();
                InvokeFinishInitBuild();
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