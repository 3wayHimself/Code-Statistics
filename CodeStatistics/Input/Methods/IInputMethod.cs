﻿using CodeStatistics.Forms;

namespace CodeStatistics.Input.Methods{
    public interface IInputMethod{
        void BeginProcess(ProjectLoadForm.UpdateCallbacks callbacks);
    }
}