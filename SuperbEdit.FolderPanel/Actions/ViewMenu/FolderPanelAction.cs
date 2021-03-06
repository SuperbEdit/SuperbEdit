﻿using System;
using System.ComponentModel.Composition;
using SuperbEdit.Base;
using SuperbEdit.FolderPanel.ViewModels;

namespace SuperbEdit.FolderPanel.Actions
{
    [ExportAction(Menu = "View", Order = 1, Owner = "FolderPanel", RegisterInCommandWindow = false)]
    public class FolderPanelAction : ShowHidePanelActionItem<FolderPanelViewModel>
    {       
        public FolderPanelAction()
            : base("Folder Panel", "Shows or hides the folder panel")
        {
        }
    }
}