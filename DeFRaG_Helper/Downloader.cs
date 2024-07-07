﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SevenZipExtractor;

namespace DeFRaG_Helper
{
    public static class Downloader
    {
        public static async Task DownloadFileAsync(string url, string destinationPath, IProgress<double> progress)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        do
                        {
                            var read = await downloadStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);

                                totalRead += read;
                                var totalReadInPercent = (double)totalRead / (double)response.Content.Headers.ContentLength.Value * 100;
                                //if (progress != null)
                                //{
                                //    progress.Report(totalReadInPercent);
                                //}
                                // Use MainWindow's instance to update the progress bar
                                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.UpdateProgressBar(totalReadInPercent));
                            }
                        } while (isMoreToRead);
                    }
                }
            }
        }



        public static async Task UnpackFile(string filename, string destinationFolder, IProgress<double> progress)
        {
            if (filename.EndsWith(".zip"))
            {
                using (var archive = new System.IO.Compression.ZipArchive(System.IO.File.OpenRead(filename)))
                {
                    var totalFiles = archive.Entries.Count;
                    for (int i = 0; i < totalFiles; i++)
                    {
                        var entry = archive.Entries[i];
                        var fullDestinationPath = Path.Combine(destinationFolder, entry.FullName);
                        if (entry.FullName.EndsWith("/")) // This is a directory
                        {
                            Directory.CreateDirectory(fullDestinationPath);
                        }
                        else // This is a file
                        {
                            // Ensure the destination directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(fullDestinationPath));
                            entry.ExtractToFile(fullDestinationPath, true);
                        }
                        progress?.Report((i + 1) * 100.0 / totalFiles);
                    }
                }
            }
            else if (filename.EndsWith(".rar") || filename.EndsWith(".7z"))
            {
                using (ArchiveFile archiveFile = new ArchiveFile(filename))
                {
                    archiveFile.Extract(destinationFolder);
                    // Unfortunately, we can't report progress for .rar and .7z files
                }
            }
            //after successful extraction, delete the zip file
            System.IO.File.Delete(filename);
        }



        //helper method to move the contents of a folder to another folder and delete the source folder
        public static async Task MoveFolderContents(string sourceFolder, string destinationFolder, IProgress<double> progress)
        {
            string sourceDirectory = Path.GetFullPath(sourceFolder);
            string destinationDirectory = Path.GetFullPath(destinationFolder);

            var allFiles = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);
            int totalFiles = allFiles.Length;
            int filesMoved = 0;

            foreach (string file in allFiles)
            {
                string destinationPath = Path.Combine(destinationDirectory, file.Substring(sourceDirectory.Length + 1));

                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                File.Move(file, destinationPath);
                filesMoved++;
                progress?.Report(filesMoved * 100.0 / totalFiles);
            }
            //delete the source folder
            Directory.Delete(sourceDirectory, true);
        }


    }
}

