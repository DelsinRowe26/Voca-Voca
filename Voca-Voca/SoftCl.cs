using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voca_Voca
{
    internal class SoftCl
    {
        public static bool IsSoftwareInstalled(string name)
        {
            bool result = false;

            CheckRegistry(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (!result) CheckRegistry(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

            void CheckRegistry(string path)
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key == null) return;
                    foreach (var subkey_name in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkey_name))
                        {
                            if (subkey != null && subkey.GetValue("DisplayName")?.ToString() == name)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
