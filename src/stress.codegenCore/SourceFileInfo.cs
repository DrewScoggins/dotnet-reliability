﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stress.codegen
{
    // enum with action
    // Compile, Binplace, Resource
    public enum SourceFileAction
    {
        None,
        Binplace,
        Compile,
        Resource
    }

    public class SourceFileInfo
    {
        public string RelativePath { get; set; }

        public SourceFileAction SourceFileAction { get; set; }

        public SourceFileInfo()
        {
            RelativePath = "";
            SourceFileAction = SourceFileAction.None;
        }

        public SourceFileInfo(string relativePath, SourceFileAction sourceFileAction)
        {
            RelativePath = relativePath;
            SourceFileAction = sourceFileAction;
        }
    }
}
