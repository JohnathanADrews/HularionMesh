#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.Pattern.Functional;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Manages access to a file.
    /// </summary>
    public class FileAccessor
    {
        /// <summary>
        /// The period between write checks in milliseconds. Cannot go below 10ms.
        /// </summary>
        public int FileWriteCheckInterval { get { return fileWriteCheckInterval; } set { fileWriteCheckInterval = value < 10 ? 10 : value; } }

        private int fileWriteCheckInterval = 200;

        private string filename = null;

        /// <summary>
        /// Set by the caller to provide the file content when saved.
        /// </summary>
        public IProvider<string> FileProvider { get; set; }

        /// <summary>
        /// Set by the caller to indicate when an update is ready.
        /// </summary>
        public FileStatus CurrentFileStatus { get; private set; } = new FileStatus();


        private Thread runThread;
        private Mutex blockMutex { get; set; } = new Mutex();

        /// <summary>
        /// If true, updates will be processed to the file. Default is false.
        /// </summary>
        public bool DoAutomaticUpdates { get; set; } = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">The name of the file to access, including directory path.</param>
        public FileAccessor(string filename)
        {
            this.filename = filename;

            if (!File.Exists(filename))
            {
                var fileIndex = filename.LastIndexOf(@"\");
                var directory = filename.Substring(0, fileIndex);
                if(fileIndex >= 0 && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                blockMutex.WaitOne();
                using (var stream = File.Create(filename)) { }
                blockMutex.ReleaseMutex();
            }
            
            runThread = new Thread(() => { UpdateFile(); });
            runThread.Start();
        }


        private void UpdateFile()
        {
            while (true)
            {
                Thread.Sleep(fileWriteCheckInterval);
                if (!CurrentFileStatus.FileIsUpdated || FileProvider == null) { continue; }
                lock (CurrentFileStatus)
                {
                    if (CurrentFileStatus.FileIsUpdated && FileProvider != null && DoAutomaticUpdates)
                    {
                        var file = FileProvider.Provide();
                        blockMutex.WaitOne();
                        File.WriteAllText(this.filename, file);
                        blockMutex.ReleaseMutex();
                        CurrentFileStatus.FileIsUpdated = false;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the entire content of the file as a string.
        /// </summary>
        /// <returns>The content of the file.</returns>
        public string ReadEntireFile()
        {
            blockMutex.WaitOne();
            var result = File.ReadAllText(this.filename);
            blockMutex.ReleaseMutex();
            return result;
        }

        /// <summary>
        /// Writes the content to the file.
        /// </summary>
        /// <param name="content">The content to write.</param>
        /// <param name="stopAutomaticUpdates">Stops automatic updates to prevent an incomplete write.</param>
        public void WriteEntireFile(string content, bool stopAutomaticUpdates)
        {
            if (stopAutomaticUpdates) { DoAutomaticUpdates = false; }
            blockMutex.WaitOne();
            File.WriteAllText(this.filename, content);
            blockMutex.ReleaseMutex();
        }

    }
}
