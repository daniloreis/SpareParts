using System;
using System.Collections.Generic;
using System.Text;

namespace SparePartsApp.Helpers
{
    public interface IUpdateProvider
    {
        void CheckUpdate();

        string GetVersion();

        void CheckVersion();
    }
}
