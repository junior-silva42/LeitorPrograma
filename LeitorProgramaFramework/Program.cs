using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LeitorProgramaFramework
{
    static class  Program
    {
        static void Main(string[] args)
        {
            List<string> keys = new List<string>() {
              @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
              @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            List<InstalledProgram> _program = new List<InstalledProgram>();
            _program.AddRange(FindPrograms(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64), keys));
            _program.AddRange(FindPrograms(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64), keys));

            _program = ProcessPrograms(_program);

        }

        private static List<InstalledProgram> ProcessPrograms(List<InstalledProgram> _program)
        {
            _program = _program.Where(s => !string.IsNullOrWhiteSpace(s.DisplayName)).Distinct().ToList();
            _program = _program.Where(x => x.Publisher?.ToUpper() != "MICROSOFT CORPORATION").ToList();
            return _program;
        }

        private static List<InstalledProgram> FindPrograms(RegistryKey regKey, List<string> keys)
        {
            List<InstalledProgram> programs = new List<InstalledProgram>();
            foreach (string key in keys)
            {
                using (RegistryKey rk = regKey.OpenSubKey(key))
                {
                    if (rk == null)
                    {
                        continue;
                    }
                   
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey subkey = rk.OpenSubKey(skName))
                        {
                            try { 
                              
                                if (subkey.GetValue("DisplayName") != null)
                                {
                                    programs.Add(new InstalledProgram
                                    {
                                        DisplayName = (string)subkey.GetValue("DisplayName"),
                                        Version = (string)subkey.GetValue("DisplayVersion"),
                                        InstalledDate = (string)subkey.GetValue("InstallDate"),
                                        Publisher = (string)subkey.GetValue("Publisher"),
                                        UnninstallCommand = (string)subkey.GetValue("UninstallString"),
                                        ModifyPath = (string)subkey.GetValue("ModifyPath")
                                    });
                                }
                            }
                            catch (Exception ex)
                            { }
                        }
                    }
                   
                }
            }
         
            return programs;
        }
    }
}
