using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EchoToolCMD.Modes;

namespace EchoToolGui
{
    /// <summary>
    /// GUI Component
    /// </summary>
    public partial class ETGui : Form
    {
        /// <summary>
        /// This wraps a GUI around the command line tools
        /// </summary>
        public ETGui()
        {
            InitializeComponent();
        }
    }
}
