//MIT, 2017, EngineKit
using System;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace EasePatcher
{
    public partial class Form1 : Form
    {
        delegate void SimpleAction();
        bool _buildState = false;
        string original_node_src = @"C:\projects\node-v7.10.0";

        public Form1()
        {
            InitializeComponent();
        }

        private void cmdPatchEspresso_Click(object sender, EventArgs e)
        {
            //-------------------------------------------------------
            //FOR WINDOWS:....
            //-------------------------------------------------------
            //1. specific location of original node src
            //2. run first FRESH build of the node src 
            string vc_build_script = original_node_src + @"\vcbuild.bat";
            if (!File.Exists(vc_build_script))
            {
                return;
            }

            _buildState = true;

            ThreadPool.QueueUserWorkItem(delegate
            {
                //choose x86 or x64;
                //choose release or debug
                ProcessStartInfo procStartInfo = new ProcessStartInfo(
                    vc_build_script,
                    "x86 release");
                //procStartInfo.UseShellExecute = false;
                //procStartInfo.RedirectStandardOutput = true;
                //procStartInfo.CreateNoWindow = true;
                procStartInfo.WorkingDirectory = original_node_src;

                Process proc = System.Diagnostics.Process.Start(procStartInfo);
                proc.WaitForExit();
                //finish
                this.Invoke(new SimpleAction(() =>
                {
                    this.Text = "Finish!";
                }));
            });

        }

        private void cmdPatch_Click(object sender, EventArgs e)
        {
            //1. copy file from espresso's patch folder 
            //and place into target dir 
            //the version must be match between original source and the patch
            string esprsso_src = @"D:\projects\CompilerKit\Espresso";
            string patch_folder = esprsso_src + @"\node_patches\node7.10_modified";

            ReplaceFileInDirectory(patch_folder, original_node_src);
            //2. copy core of libespresso bridge code( nodejs and .NET)
            //to src/espresso_ext folder

            string targetDir = original_node_src + "/src/libespresso";
            if (Directory.Exists(targetDir))
            {
                //should be 1 level
                Directory.Delete(targetDir);
            }
            //create targetdir
            Directory.CreateDirectory(targetDir);
            //copy the following file to target folder
            string[] libEsprCoreFiles = Directory.GetFiles(esprsso_src + @"\libespresso");
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
