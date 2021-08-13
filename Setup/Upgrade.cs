using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;

namespace Setup
{
    [RunInstaller(true)]
    public partial class Upgrade : System.Configuration.Install.Installer
    {
        public Upgrade()
        {
            InitializeComponent();
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(System.Collections.IDictionary savedState)
        {

            base.Commit(savedState);

            //The following code will register the gadget with SideShow       
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "quicklauncher.exe";
            myProcess.StartInfo.Arguments = "/Commit";
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.Start();
        }
    }
}
