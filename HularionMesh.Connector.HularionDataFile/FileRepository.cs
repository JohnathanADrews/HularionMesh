#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionMesh.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// A repository using a file as a data source.
    /// </summary>
    public class FileRepository
    {
        /// <summary>
        /// The mesh repository.
        /// </summary>
        public MeshRepository Repository { get; private set; }

        /// <summary>
        /// The IMeshServiceProvider.
        /// </summary>
        public HularionDataFileProvider FileProvider { get; private set; }

        /// <summary>
        /// The repository directory.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// The name of repository without directory.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The full repository filename with directory.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// If true, the mesh will be checked at a regular interval for updates. If it is updated and this is true, the file will be updates.
        /// </summary>
        public bool DoAutomaticUpdates { get { return FileProvider.DoAutomaticUpdates; } set { FileProvider.DoAutomaticUpdates = value; } }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="repository">The mesh repository.</param>
        /// <param name="fileProvider">The IMeshServiceProvider.</param>
        public FileRepository(MeshRepository repository, HularionDataFileProvider fileProvider)
        {
            Repository = repository;
            FileProvider = fileProvider;
        }

        /// <summary>
        /// Flushes the file, saving any changes.
        /// </summary>
        /// <param name="stopAutomaticUpdates">If true, automatic updates will be stopped. This should be set to true the last time the file is updated.</param>
        public void FlushFile(bool stopAutomaticUpdates = true)
        {
            if (stopAutomaticUpdates) { DoAutomaticUpdates = false; }
            FileProvider.Flush(true);
        }

    }
}
