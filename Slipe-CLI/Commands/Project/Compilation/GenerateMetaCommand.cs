﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Slipe.Commands.Project
{
    class GenerateMetaCommand : ProjectCommand
    {
        public override string Template => "meta-generate";

        private XmlDocument meta;
        private XmlElement root;

        public override void Run()
        {
            meta = new XmlDocument();
            root = meta.CreateElement("meta");

            CreateSystemElements(config);
            // CreateMTAElements();


            foreach(SlipeModule module in config.modules)
            {
                IndexDirectory(module.path + "/Lua/Compiled/Server", "server");
                IndexDirectory(module.path + "/Lua/Compiled/Client", "client");
            }

            IndexDirectory("./Dist/Server", "server");
            IndexDirectory("./Dist/Client", "client");

            CreateMainElements();
            CreateFileElements(config);
            CreateMinVersion();
            
            meta.AppendChild(root);
            meta.Save("./meta.xml");

        }

        private void CreateSystemElements(SlipeConfig config)
        {
            foreach (string systemComponent in config.systemComponents)
            {
                XmlElement element = meta.CreateElement("script");
                element.SetAttribute("src", "Slipe/Lua/System/" + systemComponent);
                element.SetAttribute("type", "shared");
                root.AppendChild(element);
            }
            foreach(SlipeModule module in config.modules)
            {
                if (module.systemComponents != null)
                {
                    foreach(string systemComponent in module.systemComponents)
                    {
                        XmlElement element = meta.CreateElement("script");
                        element.SetAttribute("src", module.path + "/Lua/SystemComponents/" + systemComponent);
                        element.SetAttribute("type", "shared");
                        root.AppendChild(element);
                    }
                }
                if (module.backingLua != null)
                {
                    foreach(string backingFile in module.backingLua)
                    {
                        XmlElement element = meta.CreateElement("script");
                        element.SetAttribute("src", module.path + "/Lua/Backing/" + backingFile);
                        element.SetAttribute("type", "shared");
                        root.AppendChild(element);
                    }
                }
            }
        }

        private void IndexDirectory(string directory, string scriptType)
        {
            Console.WriteLine("Indexing {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            foreach(string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Replace("\\", "/");
                if (relativePath.StartsWith("."))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (! file.EndsWith("manifest.lua"))
                {
                    XmlElement element = meta.CreateElement("script");
                    element.SetAttribute("src", relativePath);
                    element.SetAttribute("type", scriptType);
                    root.AppendChild(element);
                } else
                {
                    XmlElement element = meta.CreateElement("file");
                    element.SetAttribute("src", relativePath);
                    root.AppendChild(element);
                }
            }
        }

        private void CreateMainElements()
        {
            XmlElement element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/Main/main.lua");
            element.SetAttribute("type", "shared");
            root.AppendChild(element);
        }

        private void CreateMinVersion()
        {
            XmlElement element = meta.CreateElement("min_mta_version");
            element.SetAttribute("server", config.serverMinVersion);
            element.SetAttribute("client", config.clientMinVersion);
            root.AppendChild(element);
        }

        private void CreateFileElements(SlipeConfig config)
        {
            foreach(SlipeAssetDirectory directory in config.assetDirectories)
            {
                IndexDirectoryForFiles(directory.path, directory.downloads);
            }
            foreach(SlipeModule module in config.modules)
            {
                foreach (SlipeAssetDirectory directory in module.assetDirectories)
                {
                    IndexDirectoryForFiles(module.path + "/" + directory.path, directory.downloads);
                }
            }
        }

        private void IndexDirectoryForFiles(string directory, bool downloads = true)
        {
            Console.WriteLine("Indexing {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Replace("\\", "/");
                if (relativePath.StartsWith("."))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }
                XmlElement element = meta.CreateElement("file");
                element.SetAttribute("src", relativePath);
                element.SetAttribute("download", downloads.ToString().ToLower());
                root.AppendChild(element);
            }
        }
    }
}
