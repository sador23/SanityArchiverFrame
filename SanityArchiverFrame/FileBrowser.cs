using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace SanityArchiverFrame
{
    public class FileBrowser
    {

        private List<FileInfo> rootFiles;
        private List<DirectoryInfo> rootdirs;
        
        private List<FileInfo> currentFiles;
        private List<DirectoryInfo> currentdirs;

        private String path;

        private String found="";

        public String copyPath { get; set; }
        public String copyName { get; set; }


        private DirectoryInfo info;


        public FileBrowser()
        {
            rootFiles = new List<FileInfo>();
            rootdirs = new List<DirectoryInfo>();
            currentdirs = new List<DirectoryInfo>();
            currentFiles = new List<FileInfo>();
            path = @"C:\";
            info = new DirectoryInfo(path);
            FillRoot(info);
            copyPath = "";
        }


        public void SetPath(bool addition, String name = "")
        {
            if (!path.Equals(@"C:\") || addition)
            {
                if (addition)
                {
                    path += name + @"\";
                }
                else
                {
                    path = path.Substring(0, path.LastIndexOf(@"\"));
                    path = path.Substring(0,path.LastIndexOf(@"\")+1);
                }
            }
            info = new DirectoryInfo(path);
            FillCurrentFiles(info);
        }


        public void FillRoot(DirectoryInfo info)
        {
            foreach(FileInfo fileInfo in info.GetFiles())
            {
                rootFiles.Add(fileInfo);

            }

            foreach (DirectoryInfo dirinfo in info.GetDirectories())
            {
                rootdirs.Add(dirinfo);
            }
        }

        public void FillCurrentFiles(DirectoryInfo info)
        {
            currentFiles.Clear();
            foreach (FileInfo fileInfo in info.GetFiles())
            {
                currentFiles.Add(fileInfo);
            }
            currentdirs.Clear();
            foreach (DirectoryInfo dirinfo in info.GetDirectories())
            {
                currentdirs.Add(dirinfo);
            }

        }

        public List<FileInfo> GetRootFiles()
        {
            return rootFiles;
        }

        public List<DirectoryInfo> GetRootDirs()
        {
            return rootdirs;
        }


        public List<FileInfo> GetCurrentFiles()
        {
            return currentFiles;
        }

        public List<DirectoryInfo> GetCurrentDirs()
        {
            return currentdirs;
        }


        public void Zip(String name)
        {
            string startPath = path;
            string zipPath = path + name + ".zip";
            File.SetAttributes(path + name, FileAttributes.Normal);
            ZipFile.CreateFromDirectory(startPath, zipPath);
            FillCurrentFiles(new DirectoryInfo(path));
        }

        public void UnZip(String name)
        {
            string extractPath = path + name.Substring(0, name.LastIndexOf("."));
            string zipPath = path + name;
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            catch (Exception e) { }
            finally
            {
                FillCurrentFiles(new DirectoryInfo(path));
            }
            
            FillCurrentFiles(new DirectoryInfo(path));
        }

        public void Encrypt(String name, String psw)
        {
            string EnPath = path +"Crypted"+ name;
            string normal = path + name;
            Encryption.EncryptFile(normal, EnPath, psw);
            FillCurrentFiles(new DirectoryInfo(path));
        }

        public void Decrypt(String name, String psw)
        {
            File.SetAttributes(path + name, FileAttributes.Normal);
            string EnPath = path + name;
            string normal = path + name.Substring(7);
            try
            {
                Encryption.DecryptFile(EnPath, normal, psw);
            }
            catch (Exception e ) { }
            finally
            {
                FillCurrentFiles(new DirectoryInfo(path));
            }
            
        }

        public void CreateFolder(String name)
        {
            try
            {
                if (!Directory.Exists(path + name))
                {
                    Directory.CreateDirectory(path + name);
                }
            }
            catch (Exception)
            {
                
            }
            FillCurrentFiles(new DirectoryInfo(path));
        }

        public void DeleteFile(String name)
        {
            if (name.Contains('.'))
            {
                if (File.Exists(path + name))
                {
                    File.Delete(path + name);
                }
                FillCurrentFiles(new DirectoryInfo(path));
            }
            else
            {
                var dir = new DirectoryInfo(path + name);
                dir.Attributes = dir.Attributes & ~FileAttributes.ReadOnly;
                dir.Delete(true);
                FillCurrentFiles(new DirectoryInfo(path));
            }
        }

        public void SetCopyPath(String name)
        {
            File.SetAttributes(path, FileAttributes.Normal);
            copyPath = path + name;
            copyName = name;
        }

        public void CopyFile(String name = "")
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Copy(copyPath, path+copyName);
        }


        public String RecursiveSearch(String name, DirectoryInfo info)
        {
            foreach(DirectoryInfo dirInfo in info.GetDirectories())
            {
                if (dirInfo.FullName.ToLower().Contains("Windows".ToLower())) continue;
                try
                {
                if (dirInfo.FullName.ToLower().Contains(name.ToLower())) return dirInfo.FullName;
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {

                    if (fileInfo.FullName.ToLower().Contains(name.ToLower())) return fileInfo.FullName;
                        
                }
                found = RecursiveSearch(name, dirInfo);
                }catch(Exception ex)
                {
                    continue;
                }
            }

            return found;
        }
    }
}
