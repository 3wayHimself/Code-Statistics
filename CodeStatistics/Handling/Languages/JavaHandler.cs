﻿using CodeStatistics.Input;

namespace CodeStatistics.Handling.Languages {
    class JavaHandler : IFileHandler{
        public int Weight{
            get { return 50; }
        }

        public void Process(File file, Variables.Root variables){
            
        }
    }
}