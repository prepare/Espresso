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
    class Patcher
    {
        string _original_node_src_dir;
        string _espresso_src;
        string _initBuildParameters;
        bool _isWindowsBuild;
        BuildState _buildState;
        public event EventHandler FinishInitBuild;

        public void Setup(string original_node_src, string espresso_src, string initBuildParameters = "")
        {
            _buildState = BuildState.Zero;
            _initBuildParameters = initBuildParameters;
            this._original_node_src_dir = original_node_src;
            this._espresso_src = espresso_src;
            //check platform
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.OSX) ||
                System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Linux)
                )
            {
                //unix -style build
                _isWindowsBuild = false;
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
            {
                //Windows
                _isWindowsBuild = true;
            }
            else
            {
                _isWindowsBuild = false;
            }
        }
        public void InitBuild(EventHandler finishInitBuild)
        {
            if (_isWindowsBuild)
            {
                //Windows
                this.FinishInitBuild = finishInitBuild;
                InitWindowsBuild();
            }
            else
            {
                this.FinishInitBuild = finishInitBuild;
                InitUnixBuild();
            }
        }

        void InitWindowsBuild()
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
                if (FinishInitBuild != null)
                {
                    FinishInitBuild(this, EventArgs.Empty);
                }
            });
        }
        void InitUnixBuild()
        {
            //1. configure
            //2. make

            ThreadPool.QueueUserWorkItem(delegate
            {
                if (FinishInitBuild != null)
                {
                    FinishInitBuild(this, EventArgs.Empty);
                }
                UnixConfigure();
                UnixMake();
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
        public void DoPatch()
        {
            //1. copy file from espresso's patch folder 
            //and place into target dir 
            //the version must be match between original source and the patch

            string patch_folder = _espresso_src + @"\node_patches\node7.10_modified";

            ReplaceFileInDirectory(patch_folder, _original_node_src_dir);
            //2. copy core of libespresso bridge code( nodejs and .NET)
            //to src/espresso_ext folder

            string targetDir = _original_node_src_dir + "/src/libespresso";
            if (Directory.Exists(targetDir))
            {
                //should be 1 level
                Directory.Delete(targetDir);
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
    }
}