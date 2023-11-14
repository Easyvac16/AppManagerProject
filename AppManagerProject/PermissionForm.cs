using System.Security.AccessControl;

namespace AppManagerProject
{
    public partial class PermissionForm : Form
    {
        public FileSystemRights SelectedRights { get; private set; }
        public AccessControlType SelectedControlType { get; private set; }
        public PermissionForm(FileSystemRights rights, AccessControlType controlType)
        {
            InitializeComponent();

            SelectedControlType = controlType;

            checkBox1.Checked = (rights & FileSystemRights.Read) == FileSystemRights.Read;
            checkBox2.Checked = (rights & FileSystemRights.Write) == FileSystemRights.Write;
            checkBox3.Checked = (rights & FileSystemRights.FullControl) == FileSystemRights.FullControl;
        }
        public AccessControlType GetSelectedControlType()
        {
            if (checkBox1.Checked || checkBox2.Checked || checkBox3.Checked)
            {
                return AccessControlType.Allow;
            }
            else
            {
                return AccessControlType.Deny;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                SelectedRights |= FileSystemRights.Read;
            }
            else
            {
                SelectedRights &= ~FileSystemRights.Read;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                SelectedRights |= FileSystemRights.Write;
            }
            else
            {
                SelectedRights &= ~FileSystemRights.Write;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                SelectedRights |= FileSystemRights.FullControl;
            }
            else
            {
                SelectedRights &= ~FileSystemRights.FullControl;
            }
        }
    }
}
