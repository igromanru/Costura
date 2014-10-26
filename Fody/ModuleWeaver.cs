﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public XElement Config { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogError { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public string References { get; set; }
    public List<string> ReferenceCopyLocalPaths { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    public string AssemblyFilePath { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogError = s => { };
    }

    public void Execute()
    {
//#if DEBUG
//        if (!System.Diagnostics.Debugger.IsAttached)
//        {
//            System.Diagnostics.Debugger.Launch();
//        }
//#endif

        var intermediateOutputPath = Path.GetDirectoryName(AssemblyFilePath);
        LogInfo("IntermediateOutputPath resolved to :" + intermediateOutputPath);

        var config = new Configuration(Config);

        FindMsCoreReferences();

        FixResourceCase();
        ProcessNativeResources(!config.DisableCompression);
        EmbedResources(config);

        CalculateHash();
        ImportAssemblyLoader(config.CreateTemporaryAssemblies);
        ImportModuleLoader();

        AddChecksumsToTemplate();
        BuildUpNameDictionary(config.CreateTemporaryAssemblies, config.PreloadOrder);
    }
}