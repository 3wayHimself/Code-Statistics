﻿using System;
using System.Threading;
using CodeStatistics.Data;
using CodeStatistics.Forms.Project;
using CodeStatistics.Input.Helpers;

namespace CodeStatistics.Input.Methods{
    class ArchiveExtraction : IInputMethod{
        public const string FilterArchives = "|*.zip";

        private readonly string file, extractPath;
        private CancellationTokenSource cancel;

        public ArchiveExtraction(string file, string extractPath){
            this.file = file;
            this.extractPath = extractPath;
        }

        public void BeginProcess(ProjectLoadForm.UpdateCallbacks callbacks){
            callbacks.UpdateInfoLabel(Lang.Get["LoadArchiveExtracting"]);

            ZipArchive archive = new ZipArchive(file, extractPath);

            cancel = archive.ExtractAsync(path => {
                callbacks.OnReady(new FileSearch(new string[]{ path }));
                archive.Dispose();
            });
        }

        public void CancelProcess(Action onCancelFinish){
            if (cancel != null){
                cancel.Cancel();
            }

            onCancelFinish();
        }
    }
}
