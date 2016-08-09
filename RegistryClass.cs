using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace RegistrySearch
{
    public static class RegistryClass
    {
        public static string Read(RegistryKey regKey, string sSubKey, string sKeyName)
        {
            try
            {
                //subKey as read-only
                RegistryKey regSubKey = regKey.OpenSubKey(sSubKey);

                // If the RegistrySubKey doesn't exist -> (null)
                if (regSubKey == null)
                    return null;
                else
                    // If the RegistryKey exists I get its value or null is returned.
                    return regSubKey.GetValue(sKeyName.ToUpper()).ToString();
            }
            catch
            { return null; }
        }

        public static bool Write(RegistryKey regKey, string sSubKey, string sKeyName, object objValue)
        {
            try
            {
                // I have to use CreateSubKey 
                // (create or open it if already exits), 
                // because OpenSubKey open a subKey as read-only
                RegistryKey regSubKey = regKey.CreateSubKey(sSubKey);

                // Save the value
                regSubKey.SetValue(sKeyName.ToUpper(), objValue);

                return true;
            }
            catch
            { return false; }
        }

        public static int ValueCount(RegistryKey regKey, string sSubKey)
        {
            try
            {
                //subkey
                RegistryKey regSubKey = regKey.OpenSubKey(sSubKey);

                // If the RegistryKey exists...
                if (regSubKey != null)
                    return regSubKey.ValueCount;
                else
                    return 0;
            }
            catch
            { return -1; }
        }

        public static bool DeleteSubKeyTree(RegistryKey regKey, string sSubKey)
        {
            try
            {
                //subkey
                RegistryKey regSubKey = regKey.OpenSubKey(sSubKey);

                // If the RegistryKey exists, I delete it
                if (regSubKey != null)
                    regKey.DeleteSubKeyTree(sSubKey);

                return true;
            }
            catch
            { return false; }
        }

        public static int SubKeyCount(RegistryKey regKey, string sSubKey)
        {
            try
            {
                //subkey
                RegistryKey regSubKey = regKey.OpenSubKey(sSubKey);

                // If the RegistryKey exists...
                if (regSubKey != null)
                    return regSubKey.SubKeyCount;
                else
                    return 0;
            }
            catch
            { return -1; }
        }

        public static void SearchSubKey(RegistryKey root, String searchKey)
        {
            foreach (string keyname in root.GetSubKeyNames())
            {
                try
                {
                    using (RegistryKey key = root.OpenSubKey(keyname))
                    {
                        if (keyname == searchKey)
                            MessageBox.Show("Registry key found : " + key.Name + " contains " + key.ValueCount.ToString() + " values.", "Registry");

                        SearchSubKey(key, searchKey);
                    }
                }
                catch 
                { return; }
            }
        }

        public static void SearchValue(RegistryKey root, String searchKey)
        {
            foreach (string keyname in root.GetSubKeyNames())
            {
                try
                {
                    using (RegistryKey key = root.OpenSubKey(keyname))
                    {
                        if (keyname == searchKey)
                        {
                            foreach (string valuename in key.GetValueNames())
                            {
                                if (key.GetValue(valuename) is String)
                                {
                                    Console.WriteLine("  Value : {0} = {1}",
                                        valuename, key.GetValue(valuename));
                                }
                            }
                        }
                        SearchValue(key, searchKey);
                    }
                }
                catch 
                { return; }
            }
        }
        
        /// <summary>
        /// Renames a subkey of the passed in registry key since 
        /// the Framework totally forgot to include such a handy feature.
        /// </summary>
        /// <param name="regKey">The RegistryKey that contains the subkey 
        /// you want to rename (must be writeable)</param>
        /// <param name="subKeyName">The name of the subkey that you want to rename
        /// </param>
        /// <param name="newSubKeyName">The new name of the RegistryKey</param>
        /// <returns>True if succeeds</returns>
        public static bool RenameSubKey(RegistryKey parentKey, string subKeyName, string newSubKeyName)
        {
            CopyKey(parentKey, subKeyName, newSubKeyName);
            parentKey.DeleteSubKeyTree(subKeyName);
            return true;
        }

        /// <summary>
        /// Copy a registry key.  The parentKey must be writeable.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="keyNameToCopy"></param>
        /// <param name="newKeyName"></param>
        /// <returns></returns>
        public static bool CopyKey(RegistryKey parentKey, string keyNameToCopy, string newKeyName)
        {
            //Create new key
            RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName);

            //Open the sourceKey we are copying from
            RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy);

            RecurseCopyKey(sourceKey, destinationKey);

            return true;
        }

        private static void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            //copy all the values
            foreach (string valueName in sourceKey.GetValueNames())
            {        
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            //For Each subKey 
            //Create a new subKey in destinationKey 
            //Call myself 
            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName);
                RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
                RecurseCopyKey(sourceSubKey, destSubKey);
            }
        }

    }
}
