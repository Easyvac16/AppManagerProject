using System.IO.Abstractions;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AppManagerProject
{
    public partial class Form1 : Form
    {
        private string selectedPath;

        public Form1()
        {
            InitializeComponent();
            InitializeTreeView();
            InitializeListView();
        }
        private void InitializeTreeView()
        {
            treeView1.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Комп'ютер");
            treeView1.Nodes.Add(rootNode);

            string[] drives = Directory.GetLogicalDrives();

            foreach (string drive in drives)
            {

                TreeNode driveNode = new TreeNode(drive);
                driveNode.Tag = drive;
                rootNode.Nodes.Add(driveNode);

                PopulateTreeView(driveNode, drive);
            }
        }
        private void InitializeListView()
        {

            listView1.View = View.Details;
            listView1.Columns.Add("Ім'я", 200);
            listView1.Columns.Add("Тип", 100);
            listView1.Columns.Add("Розмір", 100);
            listView1.Columns.Add("Час створення", 150);

            listView1.MultiSelect = false;
        }
        private void PopulateListView(string path)
        {
            listView1.Items.Clear();
            try
            {
                string[] directories = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);

                foreach (string directory in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    ListViewItem item = new ListViewItem(new string[] { Path.GetFileName(directory), "Папка", string.Empty, dirInfo.CreationTime.ToString() });
                    item.Tag = directory;
                    listView1.Items.Add(item);
                }

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    ListViewItem item = new ListViewItem(new string[] { Path.GetFileName(file), "Файл", fileInfo.Length.ToString(), fileInfo.CreationTime.ToString() });
                    item.Tag = file;
                    listView1.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedPath = e.Node.Tag as string;
            if (selectedPath != null)
            {
                PopulateListView(selectedPath);
            }
        }
        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string selectedPathInListView = listView1.SelectedItems[0].Tag as string;

                if (selectedPathInListView != null)
                {
                    if (Directory.Exists(selectedPathInListView))
                    {
                        TreeNode selectedNode = FindNodeByFullPath(treeView1.Nodes, selectedPathInListView);
                        if (selectedNode != null)
                        {
                            treeView1.SelectedNode = selectedNode;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(selectedPathInListView);
                    }
                }
            }
        }
        private TreeNode FindNodeByFullPath(TreeNodeCollection nodes, string fullPath)
        {
            foreach (TreeNode node in nodes)
            {
                if ((string)node.Tag == fullPath)
                {
                    return node;
                }
                if (node.Nodes.Count > 0)
                {
                    TreeNode foundNode = FindNodeByFullPath(node.Nodes, fullPath);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }
            return null;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string selectedPath = treeView1.SelectedNode.Tag as string;

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    using (folderBrowserDialog1 = new FolderBrowserDialog())
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string destinationDirectory = folderBrowserDialog1.SelectedPath;
                            string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(selectedPath));

                            if (File.Exists(selectedPath))
                            {
                                await CopyFileAsync(selectedPath, destinationPath, true);
                            }
                            else if (Directory.Exists(selectedPath))
                            {
                                await CopyDirectoryAsync(selectedPath, destinationPath);
                            }
                        }
                    }
                }
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string selectedPath = treeView1.SelectedNode.Tag as string;

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    DialogResult result = MessageBox.Show("Ви впевнені, що хочете видалити цей файл?", "Підтвердження видалення", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        if (File.Exists(selectedPath))
                        {
                            await DeleteFileAsync(selectedPath);
                        }
                        else if (Directory.Exists(selectedPath))
                        {
                            await DeleteDirectoryAsync(selectedPath);
                        }

                        treeView1.SelectedNode.Remove();
                    }
                }
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string selectedPath = treeView1.SelectedNode.Tag as string;

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    using (folderBrowserDialog1 = new FolderBrowserDialog())
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string destinationDirectory = folderBrowserDialog1.SelectedPath;
                            string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(selectedPath));

                            if (File.Exists(selectedPath))
                            {
                                await MoveFileAsync(selectedPath, destinationPath);
                            }
                            else if (Directory.Exists(selectedPath))
                            {
                                await MoveDirectoryAsync(selectedPath, destinationPath);
                            }

                            InitializeTreeView();
                            PopulateListView(selectedPath);
                        }
                    }
                }
            }
        }
        private void PopulateTreeView(TreeNode parentNode, string path)
        {
            parentNode.Nodes.Clear();

            try
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    TreeNode node = new TreeNode(Path.GetFileName(directory));
                    node.Tag = directory;
                    parentNode.Nodes.Add(node);

                    PopulateTreeView(node, directory);
                }

                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    TreeNode node = new TreeNode(Path.GetFileName(file));
                    node.Tag = file;
                    parentNode.Nodes.Add(node);
                }
            }
            catch
            {

            }
        }

        //Async Методи
        private async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory)
        {
            await Task.Run(() =>
            {
                CopyDirectory(sourceDirectory, destinationDirectory);
            });
        }

        private async Task DeleteDirectoryAsync(string directory)
        {
            await Task.Run(() =>
            {
                DeleteDirectory(directory);
            });
        }

        private async Task MoveDirectoryAsync(string sourceDirectory, string destinationDirectory)
        {
            await Task.Run(() =>
            {
                MoveDirectory(sourceDirectory, destinationDirectory);
            });
        }
        private async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite)
        {
            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }
        private void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirectory);
            DirectoryInfo destinationDir = new DirectoryInfo(destinationDirectory);

            if (!destinationDir.Exists)
            {
                destinationDir.Create();
            }

            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string destinationFile = Path.Combine(destinationDirectory, file.Name);
                file.CopyTo(destinationFile, true);
            }

            foreach (DirectoryInfo subDirectory in sourceDir.GetDirectories())
            {
                string subDestination = Path.Combine(destinationDirectory, subDirectory.Name);
                CopyDirectory(subDirectory.FullName, subDestination);
            }
        }
        private void DeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка видалення: " + ex.Message);
            }
        }
        public static async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при видаленні файлу {filePath}: {ex.Message}");
            }
        }
        public static async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    await Task.Run(() => File.Move(sourcePath, destinationPath));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при переміщенні файлу з {sourcePath} в {destinationPath}: {ex.Message}");
            }
        }
        private void MoveDirectory(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                Directory.Move(sourceDirectory, destinationDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка переміщення: " + ex.Message);
            }
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedPath))
            {
                FileSystemRights currentRights;
                AccessControlType currentControlType;
                try
                {
                    if (File.Exists(selectedPath))
                    {
                        currentRights = GetFileRights(selectedPath);
                        currentControlType = GetAccessControlType(selectedPath);
                    }
                    else if (Directory.Exists(selectedPath))
                    {
                        currentRights = GetDirectoryRights(selectedPath);
                        currentControlType = GetDirectoryAccessControlType(selectedPath);
                    }
                    else
                    {
                        return;
                    }
                    PermissionForm permissionForm = new PermissionForm(currentRights, currentControlType);

                    if (permissionForm.ShowDialog() == DialogResult.OK)
                    {
                        string username = WindowsIdentity.GetCurrent().Name;
                        FileSystemRights newRights = permissionForm.SelectedRights;
                        AccessControlType newControlType = permissionForm.GetSelectedControlType();

                        await ChangePermissionsAsync(selectedPath, username, newRights, newControlType);

                        if (File.Exists(selectedPath))
                        {
                            FileSecurity fileSecurity = new FileSecurity(selectedPath, AccessControlSections.Access);
                            FileSystemAccessRule currentRule = new FileSystemAccessRule(username, currentRights, currentControlType);
                            FileSystemAccessRule newRule = new FileSystemAccessRule(username, newRights, newControlType);

                            fileSecurity.RemoveAccessRule(currentRule);

                            fileSecurity.AddAccessRule(newRule);

                            FileInfo file = new FileInfo(selectedPath);
                            file.SetAccessControl(fileSecurity);
                        }
                        else if (Directory.Exists(selectedPath))
                        {
                            DirectorySecurity directorySecurity = new DirectorySecurity(selectedPath, AccessControlSections.Access);
                            FileSystemAccessRule currentRule = new FileSystemAccessRule(username, currentRights, currentControlType);
                            FileSystemAccessRule newRule = new FileSystemAccessRule(username, newRights, newControlType);

                            directorySecurity.RemoveAccessRule(currentRule);

                            directorySecurity.AddAccessRule(newRule);

                            DirectoryInfo directory = new DirectoryInfo(selectedPath);
                            directory.SetAccessControl(directorySecurity);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }
        private FileSystemRights GetFileRights(string path)
        {
            FileSystemRights noRights = 0;
            FileSecurity fileSecurity = new FileSecurity(path, AccessControlSections.Access);
            AuthorizationRuleCollection rules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == WindowsIdentity.GetCurrent().Name)
                {
                    return rule.FileSystemRights;
                }
            }

            return noRights;
        }
        private AccessControlType GetAccessControlType(string path)
        {
            FileSecurity fileSecurity = new FileSecurity(path, AccessControlSections.Access);
            AuthorizationRuleCollection rules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == WindowsIdentity.GetCurrent().Name)
                {
                    return rule.AccessControlType;
                }
            }

            return AccessControlType.Allow;
        }
        private FileSystemRights GetDirectoryRights(string path)
        {
            FileSystemRights noRights = 0;
            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);
            AuthorizationRuleCollection rules = directorySecurity.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == WindowsIdentity.GetCurrent().Name)
                {
                    return rule.FileSystemRights;
                }
            }


            return noRights;
        }

        private AccessControlType GetDirectoryAccessControlType(string path)
        {
            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);
            AuthorizationRuleCollection rules = directorySecurity.GetAccessRules(true, true, typeof(NTAccount));

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.Value == WindowsIdentity.GetCurrent().Name)
                {
                    return rule.AccessControlType;
                }
            }

            return AccessControlType.Allow;
        }


        public static async Task ChangePermissionsAsync(string path, string username, FileSystemRights rights, AccessControlType controlType)
        {
            IFileSystem fileSystem = new FileSystem();

            if (fileSystem.File.Exists(path))
            {
                FileSecurity fileSecurity = fileSystem.File.GetAccessControl(path);
                FileSystemAccessRule accessRule = new FileSystemAccessRule(username, rights, controlType);
                fileSecurity.AddAccessRule(accessRule);
                fileSystem.File.SetAccessControl(path, fileSecurity);
            }
            else if (fileSystem.Directory.Exists(path))
            {
                DirectorySecurity directorySecurity = fileSystem.Directory.GetAccessControl(path);
                FileSystemAccessRule accessRule = new FileSystemAccessRule(username, rights, controlType);
                directorySecurity.AddAccessRule(accessRule);
                fileSystem.Directory.SetAccessControl(path, directorySecurity);
            }
            else
            {
                MessageBox.Show("Файл або папка за вказаним шляхом не існує.");
            }

            MessageBox.Show("Права доступу успішно змінено.");
        }
    }
}