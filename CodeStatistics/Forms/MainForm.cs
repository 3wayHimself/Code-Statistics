﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using CodeStatistics.Data;
using CodeStatistics.Input.Methods;
using CodeStatistics.Input;

namespace CodeStatistics.Forms{
    partial class MainForm : Form{
        public IInputMethod InputMethod{ get; private set; }

        public MainForm(){
            InitializeComponent();
            
            this.Text = Lang.Get["Title"];
            btnProjectFolder.Text = Lang.Get["MenuProjectFromFolder"];
            btnProjectGitHub.Text = Lang.Get["MenuProjectFromGitHub"];
            btnViewSourceCode.Text = Lang.Get["MenuViewSourceCode"];
            btnViewAbout.Text = Lang.Get["MenuViewAbout"];
            btnProjectArchive.Text = Lang.Get["MenuProjectFromArchive"];
        }

        // Drag Events

        private void OnDragEnter(object sender, DragEventArgs e){
            e.Effect = e.Data.GetDataPresent("FileDrop") ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e){
            if (e.Data.GetDataPresent("FileDrop")){
                object fileDropData = e.Data.GetData("FileDrop");
                string[] files = fileDropData as string[];

                if (files == null){
                    string file = fileDropData as string;
                    if (file != null)files = new[]{ file };
                }

                if (files == null || files.Length == 0)return;
                
                InputMethod = new FileSearch(files);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        // Button Click Events

        private void btnProjectFolder_Click(object sender, EventArgs e){
            string[] folders = MultiFolderDialog.Show(this);
            
            if (folders.Length != 0){
                InputMethod = new FileSearch(folders);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnViewSourceCode_Click(object sender, EventArgs e){
            Process.Start("https://github.com/chylex/Code-Statistics");
        }
    }
}
