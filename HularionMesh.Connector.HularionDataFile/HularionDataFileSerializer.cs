#region License
/*
MIT License

Copyright (c) 2023 Johnathan A Drews

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using HularionCore.TypeGraph;
using HularionMesh.Domain;
using HularionMesh.DomainLink;
using HularionMesh.DomainValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HularionText.Language.Json;
using HularionText.Language.Json.Elements;
using HularionCore.Pattern.Identifier;

namespace HularionMesh.Connector.HularionDataFile
{
    /// <summary>
    /// Serializes mesh services content to and from a file.
    /// </summary>
    public class HularionDataFileSerializer
    {

        MeshJsonSerializer serializer = new MeshJsonSerializer();

        private const string jsfMake = "JSF";
        private const string version_1_0_0 = "1.0.0";

        /// <summary>
        /// Constructor.
        /// </summary>
        public HularionDataFileSerializer()
        {
        }

        /// <summary>
        /// Creates the first line of a data file.
        /// </summary>
        /// <param name="make">The kind of file content to follow.</param>
        /// <param name="version">The version of the of the make on which the file will be created.</param>
        /// <returns></returns>
        public string MakeFirstLine(string make, string version)
        {
            var key = new ObjectKey();
            key.SetPart("HDF", "HularionDataFile");
            key.SetPart("make", make);
            key.SetPart("version", version);
            var firstLine = key.Serialized;
            return firstLine;
        }

        public string Serialize(MeshServicesFile file)
        {
            var firstLine = MakeFirstLine(jsfMake, version_1_0_0);
            var text = serializer.Serialize(file, JsonSerializationSpacing.Expanded);
            return String.Format("{0}\n{1}", firstLine, text);
        }

        /// <summary>
        /// Deserialized the file content to a mesh-readable format.
        /// </summary>
        /// <param name="content">The content to deserialize.</param>
        /// <returns>The mesh-readable content.</returns>
        public MeshServicesFile Deserialize(string content)
        {
            if (String.IsNullOrWhiteSpace(content)) { return new MeshServicesFile(); }
            var newlineIndex = content.IndexOf("\n");
            if(newlineIndex < 0) { return new MeshServicesFile(); }
            //For now, just ignore the first line since ther is only one version.
            content = content.Substring(newlineIndex);
            var file = serializer.Deserialize(content);
            return file;
        }

    }
}
