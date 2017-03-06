using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MergeTranslations
{
    /// <summary>
    /// Class that contains the translation processing methods. 
    /// </summary>
    class TranslationProcessor
    {
        public void Normalize(string src)
        {
            XmlFileManager srcFiler = new XmlFileManager(src);
            srcFiler.Normalize();
        }

        /// <summary>
        /// Give the specified src file and the dest file, keys will be added to the dest file and if specified
        /// diff files will be generated and created in the diff folder
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="diff_folder"></param>
        public void AddKeys(string src, string dest, string diff_folder = null)
        {
            if (!File.Exists(src))
                throw new Exception("Src file does not exist : " + src);
            if (!File.Exists(dest))
                throw new Exception("dest file does not exist : " + dest);

            if (!File.Exists(diff_folder))
            {
                Console.WriteLine("Warning : no diff folder specified to store added keys.");
            }

            XmlFileManager srcFiler = new XmlFileManager(src);
            XmlFileManager destFiler = new XmlFileManager(dest);
            List<MyElement> elementsToAdd = new List<MyElement>();

            foreach (var srcElement in srcFiler.ElementList)
            {
                // use linq find e in the destFiler, if found add to elementsToAdd
                MyElement destElement = destFiler.ElementList.FirstOrDefault(el => el.p == srcElement.p );
                if(destElement == null)
                {
                    elementsToAdd.Add(srcElement);
                    destFiler.Add(srcElement);
                }
            }

            // Save the updated file
            destFiler.Save();

            // Store the diff files if folder specified.
            if (diff_folder != null && elementsToAdd.Count != 0 )
            {
                XmlFileManager difFiler = new XmlFileManager(dest);
                difFiler.Clean();
                foreach (var e in elementsToAdd)
                {
                    XAttribute attribute = new XAttribute("path", e.p);
                    e.e.Add(attribute);
                    difFiler.Add(e);
                }
                var filename = diff_folder + Path.GetFileNameWithoutExtension(dest) + ".DIFF";
                difFiler.Save(filename);
            }
        }

        public void RemoveKeys(string src, string dest)
        {
            if (!File.Exists(src))
                throw new Exception("Src file does not exist : " + src);
            if (!File.Exists(dest))
                throw new Exception("dest file does not exist : " + dest);

            XmlFileManager srcFiler = new XmlFileManager(src);
            XmlFileManager destFiler = new XmlFileManager(dest);
            List<MyElement> elementsToRemove = new List<MyElement>();

            foreach (var destElement in destFiler.ElementList)
            {
                // use linq find e in the destFiler, if found add to elementsToAdd
                MyElement srcElement = srcFiler.ElementList.FirstOrDefault(el => el.p == destElement.p);
                if (srcElement == null)
                {
                    elementsToRemove.Add(destElement);
                }
            }

            if( elementsToRemove.Count() != 0 )
            {
                foreach (var e in elementsToRemove)
                {
                    destFiler.Remove(e);
                }
                destFiler.Save();
            }
        }

        public void MergeKeys(string src, string dest)
        {
            if (!File.Exists(src))
                throw new Exception("Src file does not exist : " + src);
            if (!File.Exists(dest))
                throw new Exception("dest file does not exist : " + dest);

            XmlFileManager srcFiler = new XmlFileManager(src);
            XmlFileManager destFiler = new XmlFileManager(dest);
            List<MyElement> elementsToRemove = new List<MyElement>();

            // For each key in the src, update the dest
            foreach ( var e in srcFiler.ElementList)
            {
                var path = e.e.Attribute("path");
                if (path != null)
                {
                    MyElement destElement = destFiler.ElementList.FirstOrDefault(el => el.p == path.Value);
                    if (destElement != null && destElement.e.Parent != null)
                    {
                        destElement.e.Parent.SetElementValue(e.e.Name, e.e.Value);
                        elementsToRemove.Add(e);
                    }
                }
            }

            // Incase there are any failures, we just remove the nodes that we merged into the translation files. 
            foreach(var e in elementsToRemove)
            {
                srcFiler.Remove(e);
            }

            srcFiler.Save();
            destFiler.Save();
        }
    }
}
