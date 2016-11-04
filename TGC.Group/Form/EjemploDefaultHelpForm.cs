using System.Windows.Forms;

namespace TGC.Group.Form
{
    public partial class EjemploDefaultHelpForm : System.Windows.Forms.Form
    {
        public EjemploDefaultHelpForm(string helpRtf)
        {
            InitializeComponent();

            richTextBoxHelp.Rtf = helpRtf;
        }
    }
}