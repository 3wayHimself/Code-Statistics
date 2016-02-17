﻿using CodeStatistics.Handling.General;
using CodeStatistics.Input;

namespace CodeStatistics.Handling.Languages {
    class JavaHandler : AbstractLanguageFileHandler{
        public override int Weight{
            get { return 50; }
        }

        public override void Process(File file, Variables.Root variables){
            base.Process(file,variables);

            // TODO
        }
    }
}
