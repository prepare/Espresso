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

        }

    }
}