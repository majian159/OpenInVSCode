using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace OpenInVsCode
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Path to code.exe")]
        [Description("Specify the path to code.exe.")]
        public string PathToExe { get; set; }

        [Category("General")]
        [DisplayName("Open solution/project as regular file")]
        [Description("When true, opens solutions/projects as regular files and does not load folder path into VS Code.")]
        public bool OpenSolutionProjectAsRegularFile { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PathToExe) && !File.Exists(PathToExe))
            {
                e.ApplyBehavior = ApplyKind.Cancel;
                MessageBox.Show($"The file \"{PathToExe}\" doesn't exist.", "OpenInVSCode", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            base.OnApply(e);
        }
    }
}
