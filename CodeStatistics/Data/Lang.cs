﻿using System.Collections;
using System.Collections.Generic;

namespace CodeStatistics.Data{
    class Lang : IEnumerable<KeyValuePair<string,string>>{
        public static readonly Lang Get;

        static Lang(){ // figure out languages later
            Get = new Lang{
                { "Title", "Code Statistics" },
                { "TitleProject", "Code Statistics - Project" },
                { "TitleDebug", "Code Statistics - Project Debug" },

                { "MenuProjectFromFolder", "Project From Folder" },
                { "MenuProjectFromGitHub", "Project From GitHub" },
                { "MenuProjectFromArchive", "Project From Archive" },
                { "MenuViewSourceCode", "Source Code" },
                { "MenuViewAbout", "About" },

                { "LoadProjectSearchIO", "Searching Files and Folders..." },
                { "LoadProjectProcess", "Processing the Project..." },
                { "LoadProjectProcessingDone", "Project processing finished." },
                { "LoadProjectCancel", "Cancel" },
                { "LoadProjectClose", "Close" },
                { "LoadProjectGenerate", "Generate HTML" },

                { "LoadProjectDebug", "Debug" },
                { "LoadProjectBreakpoint", "Break" },

                { "DebugProjectReprocess", "Process Again" },
                { "DebugProjectLoadOriginal", "Load Original" },
            };
        }

        private readonly Dictionary<string,string> strings = new Dictionary<string,string>();

        public string this[string key]{
            get { string value; return strings.TryGetValue(key,out value) ? value : "<UNKNOWN>"; }
        }

        private Lang(){}

        private void Add(string key, string value){
            strings.Add(key,value);
        }

        public IEnumerator<KeyValuePair<string,string>> GetEnumerator(){
            return strings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return strings.GetEnumerator();
        }
    }
}