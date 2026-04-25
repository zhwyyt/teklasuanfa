using System;
using System.Diagnostics;
using System.IO;

namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            var exporterPath = @"D:\Program Files\Tekla Structures\2017\nt\bin\plugins\TeklaBodyBracketRecognition.TeklaExporter.Gui.exe";

            if (!File.Exists(exporterPath))
            {
                System.Windows.Forms.MessageBox.Show(
                    "没有找到导出器程序:\n" + exporterPath,
                    "Tekla Body/Bracket Exporter",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exporterPath,
                WorkingDirectory = Path.GetDirectoryName(exporterPath) ?? AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }
}
