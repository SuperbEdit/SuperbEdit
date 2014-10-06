﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperbEdit.Base;

namespace SuperbEdit.Actions
{
    [Export(typeof (IActionItem))]
    [ExportActionMetadata(Menu = "File", Order = 1, Owner = "Shell", RegisterInCommandWindow = true)]
    public class OpenFileAction : ActionItem
    {
        [Import] private Lazy<IShell> shell;

        public OpenFileAction() : base("Open", "Opens a file from the filesystem.")
        {
            
        }

        public override void Execute()
        {
            shell.Value.OpenFile();
        }
    }
}