using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Drawing;

namespace RegistrySearch
{
    internal static class Functions
    {
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static FormWindowState lastWindowState = FormWindowState.Normal;
        private static string sPathSeparator = "\\";
        private static int iSelectedIndex = 0;
        internal static List<string> listKey = new List<string>();
        internal enum eSearchType
        {
            Folder = 1,
            Key = 2,
            Value = 3,
        }
        internal static eSearchType eType = eSearchType.Folder;

        internal static void Init()
        {
            try
            {
                Program.mainForm.button6.Visible = false;

                Size sizeApp = new Size();
                if (ConfigurationManager.AppSettings["windowState"] == "Maximized")
                    Program.mainForm.WindowState = FormWindowState.Maximized;
                else
                {
                    Program.mainForm.WindowState = FormWindowState.Normal;
                    sizeApp.Width = Convert.ToInt32(ConfigurationManager.AppSettings["appWidth"]);
                    sizeApp.Height = Convert.ToInt32(ConfigurationManager.AppSettings["appHeight"]);
                    Program.mainForm.Size = sizeApp;
                }

                Program.mainForm.button2.Enabled = false;
                Program.mainForm.comboBox1.Items.Add("All");
                for (int i = 1; i <= 6; i++)
                    Program.mainForm.comboBox1.Items.Add(GetSelectedRegistryRootKey(i));
                Program.mainForm.comboBox1.SelectedIndex = 0;
                Clear();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Close()
        {
            try
            {
                ChangeAppSetting("windowState", Program.mainForm.WindowState.ToString());
                if (Program.mainForm.WindowState != FormWindowState.Maximized)
                {
                    ChangeAppSetting("appWidth", Program.mainForm.Size.Width.ToString());
                    ChangeAppSetting("appHeight", Program.mainForm.Size.Height.ToString());
                }
                config.Save(ConfigurationSaveMode.Modified);

                Application.Exit();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Resize()
        {
            try
            {
                //ToDo:
                //Testen, dass nach Maximized wieder vorhergehende Größe
                //nach Schließen von Minimized nach Öffnen wieder normal
                if (lastWindowState != FormWindowState.Normal)
                {
                    ChangeAppSetting("appWidth", Program.mainForm.Size.Width.ToString());
                    ChangeAppSetting("appHeight", Program.mainForm.Size.Height.ToString());
                    config.Save(ConfigurationSaveMode.Modified);
                }

                FormWindowState windowState = Program.mainForm.WindowState;
                if (windowState == FormWindowState.Maximized || windowState == FormWindowState.Minimized)
                {
                    lastWindowState = Program.mainForm.WindowState;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void ChangeAppSetting(string sSetting, string sValue)
        {
            try
            {
                config.AppSettings.Settings.Remove(sSetting);
                config.AppSettings.Settings.Add(sSetting, sValue);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Clear()
        {
            try
            {
                Program.mainForm.listBox1.Items.Clear();
                listKey.Clear();
                Program.mainForm.progressBar1.Value = 0;
                Program.mainForm.progressBar1.Visible = false;
                Program.mainForm.groupBox2.Enabled = false;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Search()
        {
            try
            {
                Clear();
                ProcessSearch(true);

                string sFolder = Program.mainForm.textBox1.Text.Trim();
                string sKey = Program.mainForm.textBox4.Text.Trim();
                string sValue = Program.mainForm.textBox2.Text.Trim();
                iSelectedIndex = Program.mainForm.comboBox1.SelectedIndex;

                if (iSelectedIndex != 0)
                {
                    RegistryKey regKey = GetRegistryKey(iSelectedIndex);
                    if (regKey == null)
                    {
                        ProcessSearch(false);
                        return;
                    }
                    Program.mainForm.progressBar1.Maximum = regKey.SubKeyCount;
                }
                else
                    Program.mainForm.progressBar1.Maximum = 6;

                if (sFolder == "" && sKey == "" && sValue == "")
                {
                    MessageBox.Show("Please enter a folder or key or value.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ProcessSearch(false);
                }
                else
                {
                    if (sFolder != "")
                        eType = Functions.eSearchType.Folder;
                    else if (sKey != "")
                        eType = Functions.eSearchType.Key;
                    else if (sValue != "")
                        eType = Functions.eSearchType.Value;

                    Program.mainForm.worker.RunWorkerAsync();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ProcessSearch(false);
            }
        }

        internal static void ProcessSearch(bool bRun)
        {
            try
            {
                if (bRun)
                {
                    Program.mainForm.button1.Enabled = false;
                    Program.mainForm.button2.Enabled = true;
                    Program.mainForm.button3.Enabled = false;
                    Program.mainForm.button4.Enabled = false;
                    Program.mainForm.button6.Enabled = false;
                    Program.mainForm.textBox1.Enabled = false;
                    Program.mainForm.textBox2.Enabled = false;
                    Program.mainForm.textBox3.Enabled = false;
                    Program.mainForm.textBox4.Enabled = false;
                    Program.mainForm.groupBox2.Enabled = false;
                    Program.mainForm.listBox1.Enabled = false;
                    Program.mainForm.comboBox1.Enabled = false;
                    Program.mainForm.progressBar1.Visible = true;
                }
                else
                {
                    Program.mainForm.button1.Enabled = true;
                    Program.mainForm.button2.Enabled = false;
                    Program.mainForm.button3.Enabled = true;
                    Program.mainForm.button4.Enabled = true;
                    Program.mainForm.button6.Enabled = true;
                    Program.mainForm.listBox1.Enabled = true;
                    Program.mainForm.comboBox1.Enabled = true;

                    if (Program.mainForm.textBox1.Text.Trim() != "")
                        Program.mainForm.textBox1.Enabled = true;
                    if (Program.mainForm.textBox2.Text.Trim() != "")
                        Program.mainForm.textBox2.Enabled = true;
                    if (Program.mainForm.textBox4.Text.Trim() != "")
                        Program.mainForm.textBox4.Enabled = true;

                    if (Program.mainForm.textBox1.Text.Trim() == "" && Program.mainForm.textBox2.Text.Trim() == "" && Program.mainForm.textBox4.Text.Trim() == "")
                    {
                        Program.mainForm.textBox1.Enabled = true;
                        Program.mainForm.textBox2.Enabled = true;
                        Program.mainForm.textBox4.Enabled = true;
                    }

                    if (Program.mainForm.comboBox1.SelectedIndex == 0)
                        Program.mainForm.textBox3.Enabled = false;
                    else
                        Program.mainForm.textBox3.Enabled = true;

                    Program.mainForm.progressBar1.Maximum = 1;
                    Program.mainForm.progressBar1.Value = Program.mainForm.progressBar1.Maximum;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Stop()
        {
            try
            {
                Program.mainForm.worker.CancelAsync();
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static RegistryKey GetSelectedRegistryRootKey(int intRegRoot)
        {
            RegistryKey regRoot = Registry.LocalMachine;

            try
            {
                switch (intRegRoot)
                {
                    case 1:
                        regRoot = Registry.ClassesRoot;
                        break;
                    case 2:
                        regRoot = Registry.CurrentUser;
                        break;
                    case 3:
                        regRoot = Registry.LocalMachine;
                        break;
                    case 4:
                        regRoot = Registry.Users;
                        break;
                    case 5:
                        regRoot = Registry.CurrentConfig;
                        break;
                    case 6:
                        regRoot = Registry.PerformanceData;
                        break;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            return regRoot;
        }

        private static RegistryKey GetRootKey(string sRoot)
        {
            RegistryKey sRet = Registry.LocalMachine;

            try
            {
                for (int i = 1; i <= 6; i++)
                {
                    sRet = GetSelectedRegistryRootKey(i);
                    if (sRoot == sRet.Name) return sRet;
                }

                throw new Exception("Can not find root key.");

                //if (sRoot == Registry.ClassesRoot.Name)
                //    return Registry.ClassesRoot;

                //else if (sRoot == Registry.CurrentUser.Name)
                //    return Registry.CurrentUser;

                //else if (sRoot == Registry.LocalMachine.Name)
                //    return Registry.LocalMachine;

                //else if (sRoot == Registry.Users.Name)
                //    return Registry.Users;

                //else if (sRoot == Registry.CurrentConfig.Name)
                //    return Registry.CurrentConfig;

                //else if (sRoot == Registry.PerformanceData.Name)
                //    return Registry.PerformanceData;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }

            return sRet;
        }

        internal static void SearchFolder()
        {
            try
            {
                string sFolder = Program.mainForm.textBox1.Text.Trim();

                if (iSelectedIndex > 0)
                {
                    RegistryKey regFolder = GetRegistryKey(iSelectedIndex);
                    if (regFolder == null) return;

                    int iPercentFinished = 0;
                    //Parallel.ForEach(regFolder.GetSubKeyNames(), keyname => SearchKey(keyname, regFolder, sFolder));
                    foreach (string keyname in regFolder.GetSubKeyNames())
                    {
                        if (Program.mainForm.worker.CancellationPending) return;
                        SearchFolder(keyname, regFolder, sFolder);
                        Program.mainForm.worker.ReportProgress(++iPercentFinished);
                    }
                }
                else
                {
                    List<RegistryKey> listRegFolder = new List<RegistryKey>();
                    for (int i = 1; i <= 6; i++)
                        listRegFolder.Add(GetSelectedRegistryRootKey(i));
                    for (int i = 0; i < listRegFolder.Count; i++)
                    {
                        //Parallel.ForEach(sFolder.GetSubKeyNames(), keyname => SearchKey(keyname, listRegFolder[i], sFolder));
                        foreach (string keyname in listRegFolder[i].GetSubKeyNames())
                        {
                            if (Program.mainForm.worker.CancellationPending) return;
                            SearchFolder(keyname, listRegFolder[i], sFolder);
                        }
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void SearchKey()
        {
            try
            {
                string sKey = Program.mainForm.textBox4.Text.Trim();

                if (iSelectedIndex > 0)
                {
                    RegistryKey regFolder = GetRegistryKey(iSelectedIndex);
                    if (regFolder == null) return;

                    int iPercentFinished = 0;

                    SearchKeyNames(regFolder, sKey);

                    foreach (string keyname in regFolder.GetSubKeyNames())
                    {
                        try
                        {
                            if (Program.mainForm.worker.CancellationPending) return;
                            RegistryKey sSubFolder = regFolder.OpenSubKey(keyname);
                            if (sSubFolder != null) SearchKey(sSubFolder, sKey);
                        }
                        catch
                        { }
                        Program.mainForm.worker.ReportProgress(++iPercentFinished);
                    }
                }
                else
                {
                    List<RegistryKey> listRegFolder = new List<RegistryKey>();
                    for (int i = 1; i <= 6; i++)
                        listRegFolder.Add(GetSelectedRegistryRootKey(i));
                    int iPercentFinished = 0;
                    for (int i = 0; i < listRegFolder.Count; i++)
                    {
                        SearchKeyNames(listRegFolder[i], sKey);
                        foreach (string keyname in listRegFolder[i].GetSubKeyNames())
                        {
                            try
                            {
                                if (Program.mainForm.worker.CancellationPending) return;
                                RegistryKey sSubFolder = listRegFolder[i].OpenSubKey(keyname);
                                if (sSubFolder != null) SearchKey(sSubFolder, sKey);
                            }
                            catch
                            { }
                        }
                        Program.mainForm.worker.ReportProgress(++iPercentFinished);
                    }
                }
            }
            catch (System.IO.IOException)
            { }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void SearchKey(RegistryKey root, string sKey)
        {
            try
            {
                if (Program.mainForm.worker.CancellationPending) return;

                SearchKeyNames(root, sKey);

                foreach (string keyname in root.GetSubKeyNames())
                {
                    if (Program.mainForm.worker.CancellationPending) return;
                    try
                    {
                        RegistryKey sSubFolder = root.OpenSubKey(keyname);
                        if (sSubFolder != null) SearchKey(sSubFolder, sKey);
                    }
                    catch
                    { }
                }
            }
            catch (System.IO.IOException)
            { }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void SearchKeyNames(RegistryKey root, string sKey)
        {
            try
            {
                foreach (string sValueName in root.GetValueNames())
                {
                    if (Program.mainForm.worker.CancellationPending) return;
                    if (sValueName == sKey)
                    {
                        listKey.Add(root.Name);
                        Program.mainForm.worker.ReportProgress(0, 1);
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void SearchValue()
        {
            try
            {
                string sKey = Program.mainForm.textBox2.Text.Trim();

                if (iSelectedIndex > 0)
                {
                    RegistryKey regFolder = GetRegistryKey(iSelectedIndex);
                    if (regFolder == null) return;

                    int iPercentFinished = 0;

                    SearchValueNames(regFolder, sKey);

                    foreach (string keyname in regFolder.GetSubKeyNames())
                    {
                        try
                        {
                            if (Program.mainForm.worker.CancellationPending) return;
                            RegistryKey sSubFolder = regFolder.OpenSubKey(keyname);
                            if (sSubFolder != null) SearchValue(sSubFolder, sKey);
                        }
                        catch
                        { }
                        Program.mainForm.worker.ReportProgress(++iPercentFinished);
                    }
                }
                else
                {
                    List<RegistryKey> listRegFolder = new List<RegistryKey>();
                    for (int i = 1; i <= 6; i++)
                        listRegFolder.Add(GetSelectedRegistryRootKey(i));
                    int iPercentFinished = 0;
                    for (int i = 0; i < listRegFolder.Count; i++)
                    {
                        SearchKeyNames(listRegFolder[i], sKey);
                        foreach (string keyname in listRegFolder[i].GetSubKeyNames())
                        {
                            try
                            {
                                if (Program.mainForm.worker.CancellationPending) return;
                                RegistryKey sSubFolder = listRegFolder[i].OpenSubKey(keyname);
                                if (sSubFolder != null) SearchValue(sSubFolder, sKey);
                            }
                            catch
                            { }
                        }
                        Program.mainForm.worker.ReportProgress(++iPercentFinished);
                    }
                }
            }
            catch (System.IO.IOException)
            { }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void SearchValue(RegistryKey root, string sKey)
        {
            try
            {
                if (Program.mainForm.worker.CancellationPending) return;

                SearchValueNames(root, sKey);

                foreach (string keyname in root.GetSubKeyNames())
                {
                    if (Program.mainForm.worker.CancellationPending) return;
                    try
                    {
                        RegistryKey sSubFolder = root.OpenSubKey(keyname);
                        if (sSubFolder != null) SearchValue(sSubFolder, sKey);
                    }
                    catch
                    { }
                }
            }
            catch (System.IO.IOException)
            { }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void SearchValueNames(RegistryKey root, string sKey)
        {
            try
            {
                foreach (string sValueName in root.GetValueNames())
                {
                    if (Program.mainForm.worker.CancellationPending) return;
                    if (root.GetValue(sValueName) != null)
                    {
                        if (root.GetValue(sValueName).ToString() == sKey)
                        {
                            listKey.Add(root + "\\" + sValueName);
                            Program.mainForm.worker.ReportProgress(0, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static RegistryKey GetRegistryKey(int iSelectedIndex)
        {
            try
            {
                RegistryKey regKey = GetSelectedRegistryRootKey(iSelectedIndex);
                regKey = regKey.OpenSubKey(Program.mainForm.textBox3.Text.Trim());

                return regKey;
            }
            catch
            { return null; }
        }

        private static void SearchFolder(string keyname, RegistryKey root, string sFolder)
        {
            try
            {
                if (Program.mainForm.worker.CancellationPending) return;

                RegistryKey key;
                try
                {
                    key = root.OpenSubKey(keyname);
                }
                catch
                { return; }

                if (keyname == sFolder)
                {
                    listKey.Add(root.Name);
                    Program.mainForm.worker.ReportProgress(0, 1);
                }
                else
                {
                    for (int i = 0; i < key.GetSubKeyNames().Length; i++)
                    {
                        if (Program.mainForm.worker.CancellationPending) return;
                        SearchFolder(key.GetSubKeyNames()[i], key, sFolder);
                    }
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Select()
        {
            try
            {
                if (Program.mainForm.listBox1.SelectedIndex >= 0)
                {
                    Program.mainForm.groupBox2.Enabled = true;
                    Program.mainForm.textBox7.Enabled = Program.mainForm.textBox1.Enabled;
                    Program.mainForm.textBox5.Enabled = Program.mainForm.textBox4.Enabled;
                    Program.mainForm.textBox6.Enabled = Program.mainForm.textBox2.Enabled;
                    Program.mainForm.progressBar1.Value = 0;                    
                    if (Program.mainForm.textBox7.Enabled && Program.mainForm.textBox7.Text.Trim() != "") Program.mainForm.button5.Enabled = true;
                    else if (Program.mainForm.textBox5.Enabled && Program.mainForm.textBox5.Text.Trim() != "") Program.mainForm.button5.Enabled = true;
                    else if (Program.mainForm.textBox6.Enabled && Program.mainForm.textBox6.Text.Trim() != "") Program.mainForm.button5.Enabled = true;
                    else Program.mainForm.button5.Enabled = false;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        internal static void Replace()
        {
            try
            {
                int iCount = Program.mainForm.listBox1.SelectedIndices.Count;
                Program.mainForm.progressBar1.Value = 0;
                Program.mainForm.progressBar1.Maximum = iCount;

                for (int i = 0; i < iCount; i++)
                {
                    string sFolder = listKey[Program.mainForm.listBox1.SelectedIndices[i]];
                    string sRoot = "";
                    RegistryKey root;
                    string sSubFolder = "";
                    string sKey = "";
                    string sRegKey = "";
                    string sValue = "";
                    string sNew = "";

                    switch (eType)
                    {
                        case eSearchType.Folder:
                            GetRootFolder(ref sRoot, ref sFolder);
                                                        
                            sSubFolder = Program.mainForm.textBox1.Text;
                            sNew = Program.mainForm.textBox7.Text;

                            root = GetRootKey(sRoot);
                            sRegKey = root.Name;
                            if (sFolder != "") sRegKey += "\\" + sFolder + "\\" + sSubFolder;
                            else sRegKey += "\\" + sSubFolder;

                            try
                            {
                                RegistryKey regKey = root.OpenSubKey(sFolder, true);
                                if (!RegistryClass.RenameSubKey(regKey, sSubFolder, sNew)) throw new Exception("Error while renaming.");
                                regKey.Close();
                            }
                            catch (Exception ex)
                            { MessageBox.Show("Can not replace '" + sRegKey + "'." + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                            break;
                        case eSearchType.Key:
                            GetRootFolder(ref sRoot, ref sFolder);

                            sKey = Program.mainForm.textBox4.Text;
                            sNew = Program.mainForm.textBox5.Text;

                            root = GetRootKey(sRoot);
                            sRegKey = root.Name;
                            if (sFolder != "") sRegKey += "\\" + sFolder + "\\" + sKey;
                            else sRegKey += "\\" + sKey;

                            try
                            {
                                RegistryKey regKey = root.OpenSubKey(sFolder, true);
                                if (regKey == null) throw new Exception();
                                regKey.SetValue(sNew, regKey.GetValue(sKey), regKey.GetValueKind(sKey));
                                regKey.DeleteValue(sKey);
                                regKey.Close();
                            }
                            catch (Exception ex)
                            { MessageBox.Show("Can not replace '" + sRegKey + "'." + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                            break;
                        case eSearchType.Value:
                            GetRootFolderKey(ref sRoot, ref sFolder, ref sKey);

                            sValue = Program.mainForm.textBox2.Text;
                            sNew = Program.mainForm.textBox6.Text;

                            root = GetRootKey(sRoot);
                            sRegKey = root.Name;
                            if (sFolder != "") sRegKey += "\\" + sFolder + "\\" + sKey;
                            else sRegKey += "\\" + sKey;

                            try
                            {
                                RegistryKey regKey = root.OpenSubKey(sFolder, true);
                                if (regKey == null) throw new Exception();
                                regKey.SetValue(sKey, sNew);
                                regKey.Close();
                            }
                            catch (Exception ex)
                            { MessageBox.Show("Can not replace '" + sRegKey + "'." + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                            break;
                        default:
                            throw new Exception("Unknown case");
                    }

                    Program.mainForm.progressBar1.Value += 1;
                }
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void GetRootFolderKey(ref string sRoot, ref string sFolder, ref string sKey)
        {
            try
            {
                int iIndex = sFolder.LastIndexOf(sPathSeparator);
                if (iIndex < 0) throw new Exception("Can't find key name.");

                sKey = sFolder.Substring(iIndex + sPathSeparator.Length);
                sFolder = sFolder.Substring(0, iIndex);

                GetRootFolder(ref sRoot, ref sFolder);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private static void GetRootFolder(ref string sRoot, ref string sFolder)
        {
            try
            {
                int iIndex = sFolder.IndexOf(sPathSeparator);
                if (iIndex < 0) throw new Exception("Can't find root name.");

                sRoot = sFolder.Substring(0, iIndex);
                sFolder = sFolder.Substring(iIndex + sPathSeparator.Length);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }
    }
}
