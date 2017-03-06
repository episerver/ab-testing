using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MergeTranslations
{
    /// <summary>
    /// Class that allows load/save/normalize and associate xml paths with documents so we can find stuff using a unique path
    /// </summary>
    class XmlFileManager
    {
        string path { get; set; }

        public XDocument doc { get; set; }

        public List<MyElement> ElementList { get; set; }

        XElement languageElement;

        public XmlFileManager(string path)
        {
            this.path = path;
            ElementList = new List<MyElement>();
            Init();
        }

        private void toLower(XContainer container)
        {
            foreach (XElement e in container.Descendants())
            {
                e.Name = e.Name.ToString().ToLower();
                toLower(e);
            }
        }

        /// <summary>
        /// Tolowers all names and sorts each node and each descendent. 
        /// </summary>
        public void Normalize()
        {
            // Read a document
            doc = XDocument.Load(path);
            toLower(doc);
            Sort(doc);
            Save();
            doc = XDocument.Load(path);
        }

        void Init()
        {
            Normalize();

            foreach (XElement element in doc.Descendants())
            {
                if (element.Name == "language")
                {
                    languageElement = element;
                    foreach (XElement element2 in element.Elements())
                    {
                        AddToElementList(element2, "/" + element2.Name.ToString() + "/");
                    }
                    break;
                }
            }
            Save(path);
        }

        private void Sort(XDocument xContainer)
        {
            foreach (XElement element in xContainer.Descendants())
            {
                var orderedElements = (from child in element.Elements()
                                       orderby child.Name.LocalName
                                       select child).ToList();  // ToList matters, since we remove all of the child elements next

                element.Elements().Remove();
                element.Add(orderedElements);
            }
        }

        private void AddToElementList(XElement element, string path, string space = "")
        {
            ElementList.Add(new MyElement { e = element, p = path });
            foreach (XElement element2 in element.Elements())
            {
                AddToElementList(element2, path + element2.Name + "/", space + "   ");
            }
        }

        public string getParentPath(MyElement element)
        {
            return element.p.Substring(0, element.p.IndexOf(element.e.Name.ToString()));
        }

        /// <summary>
        /// Removes the specified element
        /// </summary>
        /// <param name="el"></param>
        public void Remove(MyElement element)
        {
            foreach( var child in element.e.Descendants() )
            {
                var childPath = element.p + "/" + child.Name.ToString() + "/";
                var childElement = ElementList.FirstOrDefault(l => l.p == childPath);
                if( childElement != null)
                {
                    Remove(childElement);

                    childElement.e.Remove();
                    ElementList.Remove(childElement);
                }
            }

            element.e.Remove();            
            ElementList.Remove(element);
        }

        /// <summary>
        /// Adds an element node to the document
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public MyElement Add(MyElement element)
        {
            XElement newElement = new XElement(element.e);
            var parentPath = getParentPath(element);
            var parentElement = ElementList.FirstOrDefault(l => l.p == parentPath);
            if (parentElement != null)
            {
                // only add it once, if we added a node with sub nodes, its possible the node is already there. 
                if (parentElement.e.Descendants().FirstOrDefault(l => l.Name.ToString() == element.e.Name.ToString()) == null) 
                {
                    parentElement.e.Add(newElement);
                }
                // add the path to the element list if its not already there. 
                var path = parentPath + newElement.Name.ToString() + "/";
                var e = ElementList.FirstOrDefault(l => l.p == path);
                if( e == null )
                { 
                    ElementList.Add(new MyElement { e = newElement, p = path});
                }
            }
            else
            {
                var names = parentPath.Split('/');
                if (names.Length > 2)
                {
                    MyElement parent = new MyElement() { e = new XElement(names[names.Length - 2]), p = parentPath };
                    Add(parent);
                    Add(element);
                }
                else
                {
                    languageElement.Add(element.e);
                    ElementList.Add(element);
                }
            }
            return element;
        }

        /// <summary>
        /// Removes all nodes except language nodes
        /// </summary>
        public void Clean()
        {
            foreach(var me in ElementList)
            {
                me.e.Remove();
            }
            ElementList.Clear();
        }

        /// <summary>
        /// Saves document to the specified path, or the path used to create this instance
        /// </summary>
        /// <param name="newPath"></param>
        public void Save(string newPath = null)
        {
            Sort(doc);
            if (newPath != null)
            {
                doc.Save(newPath);
            }
            else
            {
                doc.Save(path);
            }
        }
    }

    /// <summary>
    /// An internal class that allows us to associate a path with an XElement
    /// </summary>
    class MyElement
    {
        /// <summary>
        /// the xelement
        /// </summary>
        public XElement e { get; set; }
        /// <summary>
        /// the path of the xelement
        /// </summary>
        public string p { get; set; }
    }
}
