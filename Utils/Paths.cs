﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Surfer.Utils
{
    public class Paths
    {
        private static FileVersionInfo VersionInfo
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            }
        }
        public static string AppDataPath {
            get
            {
                
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), VersionInfo.CompanyName, VersionInfo.ProductName);
            }
        }
        public static string AppData(string file = "")
        {
            return (string.IsNullOrEmpty(file) || string.IsNullOrWhiteSpace(file)) ? AppDataPath : Path.Combine(AppDataPath, file);
        }
    }
}